using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveResults.Model
{
    public interface IEmmaClient
    {
        void SetCompetitionId(int compId);
        RadioControl[] GetAllRadioControls();
        RadioControl[] GetRadioControlsForClass(string className);
        Runner GetRunner(int dbId);
        string[] GetClasses();
        Runner[] GetAllRunners();
        Runner[] GetRunnersInClass(string className);
        void UpdateRunnerInfo(int id, string name, string club, string Class, string sourceId, string bib);
        void AddRunner(Runner r);
        void RemoveRunner(Runner r);
        void SetRadioControl(string className, int code, string controlName, int order);
        bool IsRunnerAdded(int runnerID);
        void SetRunnerResult(int runnerID, int time, int status, DateTime? passingTime);
        void SetRunnerSplit(int runnerID, int controlcode, int time, DateTime passingTime);
        void SetRunnerStartTime(int runnerID, int starttime);
        void MergeRadioControls(RadioControl[] radios);
        void MergeRunners(Runner[] runners);
        void UpdateCurrentResultsFromNewSet(Runner[] runners);
        void Stop();

    }
}
