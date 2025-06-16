using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LiveResults.Model
{
    public class EmmaApiClient : IDisposable, IEmmaClient
    {
        public delegate void ResultChangedDelegate(Runner runner, int position);
        public event ResultChangedDelegate ResultChanged;

        void FireResultChanged(Runner r, int position)
        {
            ResultChanged?.Invoke(r, position);
        }


        private static readonly Dictionary<int, Dictionary<string, int>> m_compsSourceToIdMapping =
            new Dictionary<int, Dictionary<string, int>>();
        private static readonly Dictionary<int, int> m_compsNextGeneratedId = new Dictionary<int, int>();

        private static readonly Dictionary<int, int[]> m_runnerPreviousDaysTotalTime = new Dictionary<int, int[]>();

        public static int GetIdForSourceIdInCompetition(int compId, string sourceId)
        {
            if (!m_compsSourceToIdMapping.ContainsKey(compId))
            {
                m_compsSourceToIdMapping.Add(compId, new Dictionary<string, int>());
                m_compsNextGeneratedId.Add(compId, -1);
            }
            if (!m_compsSourceToIdMapping[compId].ContainsKey(sourceId))
            {
                int id = m_compsNextGeneratedId[compId]--;
                m_compsSourceToIdMapping[compId][sourceId] = id;
                return id;
            }
            else
            {
                return m_compsSourceToIdMapping[compId][sourceId];
            }
        }


        public struct EmmaApiServer
        {
            public string Host;
        }
        public static EmmaApiServer[] GetServersFromConfig()
        {
            var servers = new List<EmmaApiServer>();
            int sNum = 1;
            while (true)
            {
                string server = ConfigurationManager.AppSettings["emmaApiServer" + sNum];
                if (server == null)
                    break;

                var s = new EmmaApiServer();
                s.Host = server;

                servers.Add(s);
                sNum++;

            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["serverapipollurl"]))
            {
                try
                {
                    WebRequest wq = WebRequest.Create(ConfigurationManager.AppSettings["serverapipollurl"]);
                    wq.Method = "POST";
                    byte[] data = Encoding.ASCII.GetBytes("key=" + ConfigurationManager.AppSettings["serverapipollkey"]);
                    wq.ContentLength = data.Length;
                    wq.ContentType = "application/x-www-form-urlencoded";
                    Stream st = wq.GetRequestStream();
                    st.Write(data, 0, data.Length);
                    st.Flush();
                    st.Close();
                    WebResponse ws = wq.GetResponse();
                    Stream responseStream = ws.GetResponseStream();
                    if (responseStream != null)
                    {
                        var sr = new StreamReader(responseStream);
                        string resp = sr.ReadToEnd();
                        if (resp.Trim().Length > 0)
                        {
                            string[] lines = resp.Trim().Split('\n');
                            foreach (string line in lines)
                            {
                                var s = new EmmaApiServer();
                                s.Host = line;
                                
                                servers.Add(s);
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    System.Windows.Forms.MessageBox.Show("Could not connect to " + new Uri(ConfigurationManager.AppSettings["serverpollurl"]).Host + " to query connection, error was: " + ee.Message + "\r\n\r\nStacktrace: " + ee.StackTrace);
                }
            }

            return servers.ToArray();
        }

        public event LogMessageDelegate OnLogMessage;
        private int m_compID;
        private string m_user;
        private string m_password;
        private string m_sessionID;

        private readonly Dictionary<int, Runner> m_runners;
        private readonly Dictionary<string, RadioControl[]> m_classRadioControls;
        private readonly List<DbItem> m_itemsToUpdate;
        private readonly bool m_assignIDsInternally;
        private int m_nextInternalId = 1;
        private string m_server_url;
        public EmmaApiClient(string server, int competitionID, bool assignIDsInternally = false)
        {
            m_runners = new Dictionary<int, Runner>();
            m_classRadioControls = new Dictionary<string, RadioControl[]>();
            m_itemsToUpdate = new List<DbItem>();
            m_assignIDsInternally = assignIDsInternally;

            m_compID = competitionID;
            m_server_url = server;
        }

        public void SetCompetitionId(int compId)
        {
            m_compID = compId;
        }

        public void SetCompetitionCredentials(string user,  string password)
        {
            m_user = user;
            m_password = password;
        }

        private void ResetUpdated()
        {
            foreach (Runner r in m_runners.Values)
            {
                r.RunnerUpdated = false;
                r.ResultUpdated = false;
                r.ResetUpdatedSplits();
            }
        }

        public RadioControl[] GetAllRadioControls()
        {
            Dictionary<int, RadioControl> radios = new Dictionary<int, RadioControl>();
            foreach (var kvp in m_classRadioControls)
            {
                foreach (var radioControl in kvp.Value)
                {
                    if (!radios.ContainsKey(radioControl.Code))
                    {
                        radios.Add(radioControl.Code, radioControl);
                    }
                }

            }
            return radios.Values.ToArray();
        }

        public RadioControl[] GetRadioControlsForClass(string className)
        {
            return m_classRadioControls.ContainsKey(className) ? m_classRadioControls[className] : null;

        }



        private void FireLogMsg(string msg)
        {
            if (OnLogMessage != null)
                OnLogMessage(msg);
        }

        public Runner GetRunner(int dbId)
        {
            if (!IsRunnerAdded(dbId))
                return null;
            return m_runners[dbId];
        }

        public string[] GetClasses()
        {
            Dictionary<string, string> classes = new Dictionary<string, string>();
            foreach (var r in m_runners)
            {
                if (!classes.ContainsKey(r.Value.Class))
                    classes.Add(r.Value.Class, "");
            }

            return classes.Keys.ToArray();
        }

        public Runner[] GetAllRunners()
        {
            return m_runners.Values.ToArray();
        }

        public Runner[] GetRunnersInClass(string className)
        {
            return m_runners.Values.Where(x => x.Class == className).ToArray();
        }

        public async Task<bool> ServerLogin()
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(m_server_url)
            };
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("method", "authenticate"),
                new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                new KeyValuePair<string, string>("user", m_user),
                new KeyValuePair<string, string>("password", m_password),
            });
            HttpResponseMessage response = await client.PostAsync("adm/uploadApi.php", formContent);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            // Deserialize the JSON into the C# object
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            LoginResponse res = JsonSerializer.Deserialize<LoginResponse>(responseBody, options);
            if (res.Status == "OK")
            {
                m_sessionID = res.Session_id;
                return true;
            }
            return await Task.FromResult(false);
        }

        public Task<bool> ServerLogin(string username, string password)
        {
            SetCompetitionCredentials(username, password);
            return ServerLogin();
        }

        public async Task<bool> ServerSessionOK()
        {
            return await Task.FromResult(false);
        }

        private bool m_continue;
        private bool m_currentlyBuffering;
        private Thread m_mainTh;
        public async Task Start()
        {
            FireLogMsg("Buffering existing results..");
            int numRunners = 0;
            int numResults = 0;
            var sessionOK = await ServerSessionOK();
            if (!sessionOK)
            {
                await ServerLogin();
            }
            try
            {
                m_currentlyBuffering = true;
                HttpClient client = new HttpClient()
                {
                    BaseAddress = new Uri(m_server_url)
                };

                if (!m_compsSourceToIdMapping.ContainsKey(m_compID))
                {
                    m_compsSourceToIdMapping.Add(m_compID, new Dictionary<string, int>());
                    m_compsNextGeneratedId.Add(m_compID, -1);
                }
                CompetitionData res;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, "adm/uploadApi.php?comp=" + m_compID + "&method=getcompetitionresultdata"))
                {
                    requestMessage.Headers.Add("APISESSIONID", m_sessionID);

                    HttpResponseMessage response = await client.SendAsync(requestMessage);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the JSON into the C# object
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    res = JsonSerializer.Deserialize<CompetitionData>(responseBody, options);
                }

                #region splitcontrols
                Dictionary<string, List<RadioControl>> tmpRadios = new Dictionary<string, List<RadioControl>>();
                foreach (var sc in res.splitcontrols)
                {
                    string className = sc.Key;
                    foreach (var radioControl in sc.Value)
                    {
                        int corder = radioControl.Corder;
                        int code = radioControl.Code;
                        string name = radioControl.Name;

                        if (!tmpRadios.ContainsKey(className))
                            tmpRadios.Add(className, new List<RadioControl>());

                        tmpRadios[className].Add(new RadioControl() { ClassName = className, Code = code, ControlName = name, Order = corder });
                    }
                }
                foreach (var kvp in tmpRadios)
                {
                    m_classRadioControls.Add(kvp.Key, kvp.Value.ToArray());
                }
                #endregion

                #region runneraliases
                Dictionary<int, string> idToAliasDictionary = new Dictionary<int, string>();
                foreach (var alias in res.runneraliases)
                {
                    var sourceId = alias.Sourceid;
                    if (sourceId == null)
                        continue;
                    int id = alias.Id;
                    if (!m_compsSourceToIdMapping[m_compID].ContainsKey(sourceId))
                    {
                        m_compsSourceToIdMapping[m_compID].Add(sourceId, id);
                        if (id <= m_compsNextGeneratedId[m_compID])
                            m_compsNextGeneratedId[m_compID] = id - 1;
                    }
                }

                foreach (var kvp in m_compsSourceToIdMapping[m_compID])
                {
                    if (!idToAliasDictionary.ContainsKey(kvp.Value))
                        idToAliasDictionary.Add(kvp.Value, kvp.Key);
                }
                #endregion


                #region results
                foreach (var reader in res.results)
                {
                    var dbid = reader.Dbid;
                    var control = reader.Control;
                    var time = reader.Time;
                    var bib = reader.Bib;
                    DateTime? passingTime = reader.PassingTime;
                    var sourceId = idToAliasDictionary.ContainsKey(dbid) ? idToAliasDictionary[dbid] : null;
                    if (!IsRunnerAdded(dbid))
                    {
                        var r = new Runner(dbid, reader.Name, reader.Club, reader.Classname, sourceId, bib);
                        AddRunner(r);
                        numRunners++;
                    }
                    switch (control)
                    {
                        case 1000:
                            SetRunnerResult(dbid, time, reader.Status, passingTime);
                            numResults++;
                            break;
                        case 100:
                            SetRunnerStartTime(dbid, time);
                            numResults++;
                            break;
                        default:
                            numResults++;
                            SetRunnerSplit(dbid, control, time, passingTime == null ? DateTime.MinValue : passingTime.Value);
                            break;
                    }
                }
                #endregion

                ResetUpdated();
            }
            catch (Exception ee)
            {
                FireLogMsg(ee.Message);
                Thread.Sleep(1000);
            }
            finally
            {
                m_itemsToUpdate.Clear();
                m_currentlyBuffering = false;
                FireLogMsg("Done - Buffered " + m_runners.Count + " existing runners and " + numResults + " existing results from server "+m_server_url);
            }

            m_continue = true;
            m_mainTh = new Thread(Run);
            m_mainTh.Name = "Main MYSQL Thread [" + m_server_url + "]";
            m_mainTh.Start();

            if (m_assignIDsInternally)
            {
                m_nextInternalId = m_runners.Count > 0 ? m_runners.Keys.Max() + 1 : 1;
            }
        }


        public void UpdateRunnerInfo(int id, string name, string club, string Class, string sourceId, string bib)
        {
            if (m_runners.ContainsKey(id))
            {
                var cur = m_runners[id];
                if (cur == null)
                    return;
                bool isUpdated = false;
                if (cur.Name != name)
                {
                    cur.Name = name;
                    isUpdated = true;
                }
                if (cur.Class != Class)
                {
                    cur.Class = Class;
                    isUpdated = true;
                }
                if (cur.Club != club)
                {
                    cur.Club = club;
                    isUpdated = true;
                }
                if (cur.Bib != bib)
                {
                    //FireLogMsg("Bib changed:" + cur.Bib + (cur.Bib == null ? "(null)" : "")+ " != " + bib + (bib == null ? "(null)" : ""));
                    if (!(string.IsNullOrEmpty(cur.Bib) && string.IsNullOrEmpty(bib)))
                    {
                        cur.Bib = bib;
                        isUpdated = true;
                    }
                }

                if (string.IsNullOrEmpty(sourceId))
                    sourceId = null;
                if (string.IsNullOrEmpty(cur.SourceId))
                    cur.SourceId = null;
                if (cur.SourceId != sourceId && sourceId != id.ToString(CultureInfo.InvariantCulture))
                {
                    cur.SourceId = sourceId;
                    isUpdated = true;
                }
                if (isUpdated)
                {
                    cur.RunnerUpdated = true;
                    m_itemsToUpdate.Add(cur);

                    if (!m_currentlyBuffering)
                    {
                        FireLogMsg("Runnerinfo changed [" + cur.Name + "]");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a Runner to this competition
        /// </summary>
        /// <param name="r"></param>
        public void AddRunner(Runner r)
        {
            if (!m_runners.ContainsKey(r.ID))
            {
                m_runners.Add(r.ID, r);

                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner added [" + r.Name + "]");
                }
            }
        }

        /// <summary>
        /// Adds a Runner to this competition
        /// </summary>
        /// <param name="r"></param>
        public void RemoveRunner(Runner r)
        {
            if (m_runners.ContainsKey(r.ID))
            {
                m_runners.Remove(r.ID);
                m_itemsToUpdate.Add(new DelRunner { RunnerID = r.ID });
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner deleted [" + r.Name + ", " + r.Class + "]");
                }
            }
        }

        public void SetRadioControl(string className, int code, string controlName, int order)
        {
            if (!m_classRadioControls.ContainsKey(className))
                m_classRadioControls.Add(className, new RadioControl[0]);

            var radios = new List<RadioControl>();
            radios.AddRange(m_classRadioControls[className]);
            radios.Add(new RadioControl { ClassName = className, Code = code, ControlName = controlName, Order = order });
            m_classRadioControls[className] = radios.ToArray();
            //m_classRadioControls.Add(className,new RadioControl[] { );
            m_itemsToUpdate.Add(new RadioControl
            {
                ClassName = className,
                Code = code,
                ControlName = controlName,
                Order = order
            });
        }

        public int UpdatesPending
        {
            get
            {
                return m_itemsToUpdate.Count;
            }
        }

        /// <summary>
        /// Returns true if a runner with the specified runnerid exist in the competition
        /// </summary>
        /// <param name="runnerID"></param>
        /// <returns></returns>
        public bool IsRunnerAdded(int runnerID)
        {
            return m_runners.ContainsKey(runnerID);
        }

        /// <summary>
        /// Sets the result for the runner with runnerID
        /// </summary>
        /// <param name="runnerID"></param>
        /// <param name="time"></param>
        /// <param name="status"></param>
        public void SetRunnerResult(int runnerID, int time, int status, DateTime? passingTime)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerResult]");

            var r = m_runners[runnerID];

            if (r.HasResultChanged(time, status))
            {
                r.SetResult(time, status, passingTime);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireResultChanged(r, 1000);
                    FireLogMsg("Runner result changed: [" + r.Name + ", " + r.Time + "]");
                }
            }
        }

        public void SetRunnerSplit(int runnerID, int controlcode, int time, DateTime passingTime)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerResult]");
            var r = m_runners[runnerID];

            if (r.HasSplitChanged(controlcode, time))
            {
                r.SetSplitTime(controlcode, time, passingTime);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireResultChanged(r, controlcode);
                    FireLogMsg("Runner Split Changes: [" + r.Name + ", {cn: " + controlcode + ", t: " + time + "}]");
                }
            }

        }

        public void SetRunnerStartTime(int runnerID, int starttime)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerStartTime]");
            var r = m_runners[runnerID];

            if (r.HasStartTimeChanged(starttime))
            {
                r.SetStartTime(starttime);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner starttime Changed: [" + r.Name + ", t: " + starttime + "}]");
                }
            }

        }

        public void MergeRadioControls(RadioControl[] radios)
        {
            if (radios == null)
                return;

            foreach (var kvp in radios.GroupBy(x => x.ClassName))
            {
                RadioControl[] controls = kvp.OrderBy(x => x.Order).ToArray();
                if (m_classRadioControls.ContainsKey(kvp.Key))
                {
                    RadioControl[] existingRadios = m_classRadioControls[kvp.Key];
                    for (int i = 0; i < controls.Length; i++)
                    {
                        if (existingRadios.Length > i)
                        {
                            if (existingRadios[i].Order != controls[i].Order
                                || existingRadios[i].Code != controls[i].Code
                                || existingRadios[i].ControlName != controls[i].ControlName)
                            {
                                m_itemsToUpdate.Add(new DelRadioControl() { ToDelete = existingRadios[i] });
                                m_itemsToUpdate.Add(controls[i]);
                            }
                        }
                        else
                        {
                            m_itemsToUpdate.Add(controls[i]);
                        }
                    }
                    if (existingRadios.Length > controls.Length)
                    {
                        for (int i = controls.Length; i < existingRadios.Length; i++)
                        {
                            m_itemsToUpdate.Add(new DelRadioControl() { ToDelete = existingRadios[i] });
                        }
                    }
                    m_classRadioControls[kvp.Key] = controls;

                }
                else
                {
                    foreach (var control in controls)
                    {
                        m_itemsToUpdate.Add(control);
                    }
                    m_classRadioControls.Add(kvp.Key, controls);
                }
            }
        }

        public void MergeRunners(Runner[] runners)
        {
            if (runners == null)
                return;

            foreach (var r in runners)
            {
                if (!IsRunnerAdded(r.ID))
                {
                    AddRunner(new Runner(r.ID, r.Name, r.Club, r.Class, r.SourceId, r.Bib));
                }
                else
                {
                    UpdateRunnerInfo(r.ID, r.Name, r.Club, r.Class, r.SourceId, r.Bib);
                }


                UpdateRunnerTimes(r);
            }
        }

        public void UpdateCurrentResultsFromNewSet(Runner[] runners)
        {
            if (runners == null)
                return;

            var existingClassGroups = m_runners.Values.GroupBy(x => x.Class, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.ToArray(), StringComparer.OrdinalIgnoreCase);
            foreach (var classGroup in runners.GroupBy(x => x.Class))
            {
                if (existingClassGroups.ContainsKey(classGroup.Key))
                {
                    var existingClass = existingClassGroups[classGroup.Key];
                    var duplicateCounter = new Dictionary<string, int>();
                    foreach (var runner in classGroup)
                    {
                        string duplValue = (runner.Name + ":" + runner.Club).ToLower();
                        if (!duplicateCounter.ContainsKey(duplValue))
                        {
                            duplicateCounter.Add(duplValue, 0);
                        }

                        duplicateCounter[duplValue]++;
                        int findInstance = duplicateCounter[duplValue];

                        /*Find existing*/
                        Runner currentRunner = null;
                        int instNum = 0;
                        foreach (var existingRunner in existingClass)
                        {

                            if (string.Compare(existingRunner.Name, runner.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                                string.Compare(existingRunner.Club, runner.Club, StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                instNum++;
                                if (instNum == findInstance)
                                {
                                    currentRunner = existingRunner;
                                    break;
                                }
                            }
                        }
                        if (currentRunner != null)
                        {
                            runner.ID = currentRunner.ID;
                            UpdateRunnerInfo(runner.ID, runner.Name, runner.Club, runner.Class, runner.SourceId, runner.Bib);
                        }
                        else
                        {
                            //New runner
                            runner.ID = m_nextInternalId++;
                            var newRunner = new Runner(runner.ID, runner.Name, runner.Club, runner.Class, runner.SourceId, runner.Bib);
                            AddRunner(newRunner);
                        }
                        UpdateRunnerTimes(runner);
                    }

                    /*Detect runners that are removed*/
                    /*duplicateCounter = new Dictionary<string, int>();
                    foreach (var existingRunner in existingClass)
                    {
                        string duplValue = (existingRunner.Name + ":" + existingRunner.Club).ToLower();
                        if (!duplicateCounter.ContainsKey(duplValue))
                        {
                            duplicateCounter.Add(duplValue, 0);
                        }

                        duplicateCounter[duplValue]++;
                        int findInstance = duplicateCounter[duplValue];
                        bool exists = false;
                        int instNum = 0;
                        foreach (var runner in classGroup)
                        {
                            if (string.Compare(existingRunner.Name, runner.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                                string.Compare(existingRunner.Club, runner.Club, StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                 instNum++;
                                 if (instNum == findInstance)
                                 {
                                     exists = true;
                                     break;
                                 }
                            }
                        }
                        if (!exists)
                        {
                            //Remove runner
                            RemoveRunner(existingRunner);
                        }
                    }*/
                }
                else
                {
                    //new class, add all
                    foreach (var runner in classGroup)
                    {
                        runner.ID = m_nextInternalId++;
                        var newRunner = new Runner(runner.ID, runner.Name, runner.Club, runner.Class, runner.SourceId, runner.Bib);
                        AddRunner(newRunner);
                        UpdateRunnerTimes(runner);
                    }
                }
            }
        }

        private void UpdateRunnerTimes(Runner runner)
        {
            if (runner.StartTime >= 0)
                SetRunnerStartTime(runner.ID, runner.StartTime);

            SetRunnerResult(runner.ID, runner.Time, runner.Status, runner.FinishTime);

            var spl = runner.SplitTimes;
            if (spl != null)
            {
                foreach (var s in spl)
                {
                    SetRunnerSplit(runner.ID, s.Control, s.Time, s.PassingTime);
                }
            }
        }


        public void Stop()
        {
            m_continue = false;
        }

        private async void Run()
        {
            bool runOffline = ConfigurationManager.AppSettings["runoffline"] == "true";
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(m_server_url),
            };
            while (m_continue)
            {
                try
                {
                    while (m_continue)
                    {
                        if (m_itemsToUpdate.Count > 0)
                        {
                            if (runOffline)
                            {
                                m_itemsToUpdate.RemoveAt(0);
                                continue;
                            }
                            var item = m_itemsToUpdate[0];
                            if (item is RadioControl)
                            {
                                var r = item as RadioControl;
                                var formContent = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("method", "updateradiocontrol"),
                                    new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                    new KeyValuePair<string, string>("classname", r.ClassName),
                                    new KeyValuePair<string, string>("corder", Convert.ToString(r.Order)),
                                    new KeyValuePair<string, string>("code", Convert.ToString(r.Code)),
                                    new KeyValuePair<string, string>("cname", r.ControlName),
                                });
                                formContent.Headers.Add("APISESSIONID", m_sessionID);

                                try
                                {
                                    var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                    response.EnsureSuccessStatusCode();
                                }
                                catch (Exception ee)
                                {
                                    //Move failing runner last
                                    m_itemsToUpdate.Add(r);
                                    m_itemsToUpdate.RemoveAt(0);
                                    throw new ApplicationException("Could not add radiocontrol " + r.ControlName + ", " + r.ClassName + ", " + r.Code + " to server due to: " + ee.Message, ee);
                                }
                            }
                            else if (item is DelRadioControl)
                            {
                                var dr = item as DelRadioControl;
                                var r = dr.ToDelete;

                                var formContent = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("method", "deleteradiocontrol"),
                                    new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                    new KeyValuePair<string, string>("classname", r.ClassName),
                                    new KeyValuePair<string, string>("corder", Convert.ToString(r.Order)),
                                    new KeyValuePair<string, string>("code", Convert.ToString(r.Code)),
                                    new KeyValuePair<string, string>("cname", r.ControlName),
                                });
                                formContent.Headers.Add("APISESSIONID", m_sessionID);
                                try
                                {
                                    var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                    response.EnsureSuccessStatusCode();
                                }
                                catch (Exception ee)
                                {
                                    //Move failing runner last
                                    m_itemsToUpdate.Add(r);
                                    m_itemsToUpdate.RemoveAt(0);
                                    throw new ApplicationException("Could not delete radiocontrol " + r.ControlName + ", " + r.ClassName + ", " + r.Code + " to server due to: " + ee.Message, ee);
                                }
                            }
                            else if (item is DelRunner)
                            {
                                var dr = item as DelRunner;
                                var r = dr.RunnerID;
                                var formContent = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("method", "deleterunner"),
                                    new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                    new KeyValuePair<string, string>("dbid", Convert.ToString(r)),
                                });
                                formContent.Headers.Add("APISESSIONID", m_sessionID);
                                try
                                {
                                    var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                    response.EnsureSuccessStatusCode();
                                }
                                catch (Exception ee)
                                {
                                    //Move failing runner last
                                    m_itemsToUpdate.Add(dr);
                                    m_itemsToUpdate.RemoveAt(0);
                                    throw new ApplicationException("Could not delete runner " + r + " on server due to: " + ee.Message, ee);
                                }
                            }
                            else if (item is Runner)
                            {
                                var r = item as Runner;
                                if (r.RunnerUpdated)
                                {
                                    var formContent = new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("method", "updaterunner"),
                                        new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                        new KeyValuePair<string, string>("name", Convert.ToString(r.Name)),
                                        new KeyValuePair<string, string>("club", Convert.ToString(r.Club ?? "")),
                                        new KeyValuePair<string, string>("classname", Convert.ToString(r.Class)),
                                        new KeyValuePair<string, string>("dbid", Convert.ToString(r.ID)),
                                        new KeyValuePair<string, string>("sourceid", Convert.ToString(r.SourceId)),
                                        new KeyValuePair<string, string>("bib", r.Bib != null ? Convert.ToString(r.Bib) : null),
                                    });
                                    formContent.Headers.Add("APISESSIONID", m_sessionID);

                                    try
                                    {
                                        var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                        response.EnsureSuccessStatusCode();
                                    }
                                    catch (Exception ee)
                                    {
                                        //Move failing runner last
                                        m_itemsToUpdate.Add(r);
                                        m_itemsToUpdate.RemoveAt(0);
                                        throw new ApplicationException(
                                            "Could not add runner " + r.Name + ", " + r.Club + ", " + r.Class + " to server due to: " + ee.Message, ee);
                                    }

                                    FireLogMsg("Runner " + r.Name + " updated in DB");
                                    r.RunnerUpdated = false;
                                }
                                if (r.ResultUpdated)
                                {
                                    var formContent = new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("method", "updaterunnerresults"),
                                        new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                        new KeyValuePair<string, string>("dbid", Convert.ToString(r.ID)),
                                        new KeyValuePair<string, string>("time", Convert.ToString(r.Time)),
                                        new KeyValuePair<string, string>("status", Convert.ToString(r.Status)),
                                        new KeyValuePair<string, string>("finishTime", r.FinishTime == null ? "" : r.FinishTime.Value.ToString("yyyy-MM-dd H:mm:ss")),
                                    });
                                    formContent.Headers.Add("APISESSIONID", m_sessionID);
                                    var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                    response.EnsureSuccessStatusCode();
                                    FireLogMsg("Runner " + r.Name + "s result updated in DB");
                                    r.ResultUpdated = false;
                                }
                                if (r.StartTimeUpdated)
                                {
                                    var formContent = new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("method", "updaterunnerstarttime"),
                                        new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                        new KeyValuePair<string, string>("dbid", Convert.ToString(r.ID)),
                                        new KeyValuePair<string, string>("starttime", Convert.ToString(r.StartTime)),
                                        new KeyValuePair<string, string>("status", Convert.ToString(r.Status)),
                                    });
                                    formContent.Headers.Add("APISESSIONID", m_sessionID);
                                    var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                    response.EnsureSuccessStatusCode();
                                    FireLogMsg("Runner " + r.Name + "s starttime updated in DB");
                                    r.StartTimeUpdated = false;
                                }
                                if (r.HasUpdatedSplitTimes())
                                {
                                    List<SplitTime> splitTimes = r.GetUpdatedSplitTimes();
                                    foreach (SplitTime t in splitTimes)
                                    {
                                        var formContent = new FormUrlEncodedContent(new[]
                                        {
                                            new KeyValuePair<string, string>("method", "updaterunnersplittimes"),
                                            new KeyValuePair<string, string>("comp", Convert.ToString(m_compID)),
                                            new KeyValuePair<string, string>("dbid", Convert.ToString(r.ID)),
                                            new KeyValuePair<string, string>("time", Convert.ToString(t.Time)),
                                            new KeyValuePair<string, string>("code", Convert.ToString(t.Control)),
                                            new KeyValuePair<string, string>("passingTime", Convert.ToString(t.PassingTime)),
                                        });
                                        formContent.Headers.Add("APISESSIONID", m_sessionID);
                                        var response = await client.PostAsync("/adm/uploadApi.php", formContent);
                                        response.EnsureSuccessStatusCode();
                                        t.Updated = false;
                                        FireLogMsg("Runner " + r.Name + " splittime{" + t.Control + "}" + t.Time + " updated in DB");
                                    }
                                }
                            }

                            m_itemsToUpdate.RemoveAt(0);
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                }
                catch (Exception ee)
                {
                    FireLogMsg("Error: " + ee.Message + (m_server_url != null ? " [" + m_server_url + "]" : ""));
                    System.Diagnostics.Debug.Write(ee.Message);
                    Thread.Sleep(1000);
                }
            }
        }

        public override string ToString()
        {
            return (m_server_url != null ? m_server_url : "Detached") + " (" + UpdatesPending + ")";
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
        }

        #endregion
    }

    class LoginResponse
    {
        public string Status { get; set; }
        public string Session_id { get; set; }
        public string Message { get; set; }
    }
}
