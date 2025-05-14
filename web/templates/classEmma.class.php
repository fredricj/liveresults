<?php
$CHARSET = 'utf-8';

class Emma
{
	public static string $db_server = "127.0.0.1";
	public static string $db_database = "liveresultat";
	public static string $db_user = "liveresultat";
	public static string $db_pw = "web";
	public static string $MYSQL_CHARSET = "utf8mb4";
	var int $m_CompId;
	
	var string $m_CompName;
	
	var string $m_CompDate;
	var int $m_TimeDiff = 0;
	var bool $m_IsMultiDayEvent = false;
	var int $m_MultiDayStage = -1;
	var int $m_MultiDayParent = -1;
	
	var string $m_VideoFormat = "";
	var string $m_VideoUrl = "";
	var ?string $m_TwitterFeed = "";
	
	var ?mysqli $m_Conn;
	
	private static function openConnection(): mysqli
	{
		$conn = mysqli_connect(self::$db_server, self::$db_user, self::$db_pw, self::$db_database);
		if (mysqli_connect_errno()) {
			printf("Connect failed: %s\n", mysqli_connect_error());
			exit();
		}
		mysqli_set_charset($conn, self::$MYSQL_CHARSET);
		return $conn;
	}
	
	public static function GetCompetitions(): array
	{
		$conn = self::openConnection();
		$result = mysqli_query($conn, "select compName, compDate,tavid,organizer,timediff,multidaystage,multidayparent from login where public = 1 order by compDate desc");
		$ret = array();
		while ($tmp = mysqli_fetch_array($result)) {
			$ret[] = $tmp;
		}
		mysqli_free_result($result);
		return $ret;
	}
	
	public static function GetCompetitionsToday(): array
	{
		$conn = self::openConnection();
		$result = $conn->execute_query("select compName, compDate,tavid,organizer,timediff,multidaystage,multidayparent from login where public = 1 and compDate = ?", [date("Y-m-d")]);
		$ret = array();
		while ($tmp = mysqli_fetch_array($result)) {
			$ret[] = $tmp;
		}
		mysqli_free_result($result);
		return $ret;
	}
	
	public static function GetRadioControls($compid): array
	{
		$conn = self::openConnection();
		$result = $conn->execute_query("select * from splitcontrols where tavid=? order by classname, corder", [$compid]);
		$ret = array();
		while ($tmp = mysqli_fetch_array($result)) {
			$ret[] = $tmp;
		}
		mysqli_free_result($result);
		return $ret;
	}
	
	public static function DelRadioControl($compid, $code, $classname): void
	{
		$conn = self::openConnection();
		$conn->execute_query("delete from splitcontrols where tavid=? and code=? and classname=?", [$compid, $code, $classname]);
	}
	
	public static function DeleteRadioControl($compid, $classname, $corder, $code, $cname): void
	{
		$conn = self::openConnection();
		$q = "DELETE FROM splitcontrols
       			WHERE tavid = ? and classname = ? and corder = ? and code = ? and name = ?";
		$conn->execute_query($q, [$compid, $classname, $corder, $code, $cname]);
	}
	
	public static function DelAllRadioControls($compid): void
	{
		$conn = self::openConnection();
		$conn->execute_query("delete from splitcontrols where tavid=?", [$compid]);
	}
	
	
	public static function CreateCompetition($name, $org, $date, $email, $password): int
	{
		$conn = self::openConnection();
		$password_hash = password_hash($password, PASSWORD_DEFAULT);
		$res = mysqli_query($conn, "select max(tavid)+1 from login");
		list($id) = mysqli_fetch_row($res);
		if ($id < 10000) {
			$id = 10000;
		}
		$result = $conn->execute_query("insert into login (tavid,user,pass,compName,organizer,compDate,public) values(?,?,?,?,?,?,0)", [$id, $email, $password_hash, $name, $org, $date]) or die(mysqli_error($conn));
		return $conn->insert_id;
	}
	
	public static function CreateCompetitionFull($name, $org, $date, $email, $password, $country): int
	{
		$conn = self::openConnection();
		$password_hash = password_hash($password, PASSWORD_DEFAULT);
		$res = mysqli_query($conn, "select max(tavid)+1 from login");
		list($id) = mysqli_fetch_row($res);
		if ($id < 10000) {
			$id = 10000;
		}
		$conn->execute_query("insert into login (tavid, user,pass,compName,organizer,compDate,public, country) values(?,?,?,?,?,?,0,?)", [$id, $email, $password_hash, $name, $org, $date, $country]) or die(mysqli_error($conn));
		return $conn->insert_id;	}
	
	public function GetCompetitionRunnerAliases(): array
	{
		$res = $this->m_Conn->execute_query("select sourceid,id from runneraliases where compid = ?", [$this->m_CompId]);
		$out = [];
		while ($row = $res->fetch_assoc()) {
			$out[] = $row;
		}
		return $out;
	}
	
	public function GetCompetitionResults(): array
	{
		$q = "SELECT runners.dbid,control,time,name,club,class as classname,status,bib
				FROM runners
				JOIN results USING (dbid, tavid)
				WHERE results.tavid = ?";
		$res = $this->m_Conn->execute_query($q, [$this->m_CompId]);
		$rows = [];
		while ($row = $res->fetch_assoc()) {
			$rows[] = $row;
		}
		return $rows;
	}
	
	
	public static function AddRadioControl($compid, $classname, $name, $code): void
	{
		$conn = self::openConnection();
		$res = $conn->execute_query("select count(*)+1 from splitcontrols where classname=? and tavid=?", [$classname, $compid]);
		list($id) = mysqli_fetch_row($res);
		$conn->execute_query( "insert into splitcontrols(tavid,classname,name,code,corder) values(?, ?, ?, ?, ?)", [$compid, $classname, $name, $code, $id]) or die(mysqli_error($conn));
	}
	
	public static function UpdateRadioControl($compid, $classname, $name, $code, $corder): void
	{
		$conn = self::openConnection();
		$q = "REPLACE INTO splitcontrols(tavid,classname,corder,code,name) VALUES (?,?,?,?,?)";
		$conn->execute_query($q, [$compid, $classname, $corder, $code, $name]);
	}
	
	public static function UpdateCompetition($id, $name, $org, $date, $public, $timediff): void
	{
		$conn = self::openConnection();
		$sql = "update login set compName = ?, organizer=?, compDate =?,timediff=?, public=? where tavid=?";
		$conn->execute_query($sql, [$name, $org, $date, $timediff, (!isset($public) ? "0" : "1"), $id]) or die(mysqli_error($conn));
	}
	
	public static function GetAllCompetitions(): array
	{
		$conn = self::openConnection();
		$result = mysqli_query($conn, "select compName, compDate,tavid,timediff,organizer,public from login order by compDate desc");
		$ret = array();
		while ($tmp = mysqli_fetch_array($result)) {
			$ret[] = $tmp;
		}
		mysqli_free_result($result);
		return $ret;
	}
	
	public static function GetCompetition($compid): ?array
	
	{
		$conn = self::openConnection();
		$result = $conn->execute_query("select compName, compDate,tavid,organizer,public,timediff, timezone, videourl, videotype,multidaystage,multidayparent from login where tavid=?", [$compid]);
		$ret = null;
		while ($tmp = mysqli_fetch_array($result)) {
			$ret = $tmp;
		}
		mysqli_free_result($result);
		return $ret;
	}
	
	
	function __construct($compID)
	
	{
		$this->m_CompId = $compID;
		$this->m_Conn = self::openConnection();
		$result = $this->m_Conn->execute_query("select * from login where tavid = ?", [$compID]);
		if ($tmp = mysqli_fetch_array($result)) {
			$this->m_CompName = $tmp["compName"];
			$this->m_CompDate = date("Y-m-d", strtotime($tmp["compDate"]));
			$this->m_TimeDiff = $tmp["timediff"] * 3600;
			if (isset($tmp["videourl"]))
				$this->m_VideoUrl = $tmp["videourl"];
			if (isset($tmp["videotype"]))
				$this->m_VideoFormat = $tmp["videotype"];
			if (isset($tmp["twitter"]))
				$this->m_TwitterFeed = $tmp["twitter"];
			if (isset($tmp['multidaystage'])) {
				if ($tmp['multidaystage'] != null && $tmp['multidayparent'] != null && $tmp['multidaystage'] > 1) {
					$this->m_IsMultiDayEvent = true;
					$this->m_MultiDayStage = $tmp['multidaystage'];
					$this->m_MultiDayParent = $tmp['multidayparent'];
				}
			}
		}
	}
	
	public static function validLoginForComp(int $compid, string $email, string $password): bool
	{
		$conn = self::openConnection();
		$result = $conn->execute_query("SELECT user, pass FROM login WHERE tavid=?", [$compid]);
		if ($row = $result->fetch_assoc()) {
			$corr =  (int)hash_equals($email, $row["user"]);
			$corr &= (int)password_verify($password, $row["pass"]);
			return (bool)$corr;
		} else {
			return false;
		}
	}
	
	function IsMultiDayEvent(): bool
	{
		return $this->m_IsMultiDayEvent;
	}
	
	function HasVideo(): bool
	{
		return $this->m_VideoFormat != "";
	}
	
	function HasTwitter(): bool
	{
		return $this->m_TwitterFeed != "";
	}
	
	function GetVideoEmbedCode(): string
	{
		if ($this->m_VideoFormat == "bambuser") {
			return '<iframe src="http://embed.bambuser.com/channel/'.$this->m_VideoUrl.'" width="460" height="403" frameborder="0">Your browser does not support iframes.</iframe>';
		}
		return "";
	}
	
	function GetTwitterFeed(): ?string
	{
		return $this->m_TwitterFeed;
	}
	
	function CompName(): string
	{
		return $this->m_CompName;
	}
	
	function CompDate(): string
	{
		return $this->m_CompDate;
	}
	
	function TimeZoneDiff(): float|int
	{
		return $this->m_TimeDiff / 3600;
	}
	
	
	function Classes(): array
	{
		$ret = array();
		$result = $this->m_Conn->execute_query("SELECT Class From runners where TavId = ? Group By Class", [$this->m_CompId]);
		if ($result) {
			while ($row = mysqli_fetch_array($result)) {
				$ret[] = $row;
			}
			mysqli_free_result($result);
		} else
			die(mysqli_error($this->m_Conn));
		return $ret;
	}
	
	function getAllSplitControls(): array
	{
		$ret = array();
		$result = $this->m_Conn->execute_query("SELECT code, name, classname, corder from splitcontrols where tavid = ? order by corder", [$this->m_CompId]);
		if ($result) {
			while ($tmp = mysqli_fetch_assoc($result)) {
				$ret[] = $tmp;
			}
			mysqli_free_result($result);
		} else {
			echo(mysqli_error($this->m_Conn));
		}
		return $ret;
	}
	
	
	function getSplitControlsForClass($className): array
	{
		$ret = array();
		$result = $this->m_Conn->execute_query("SELECT code, name from splitcontrols where tavid = ? and classname = ? order by corder", [$this->m_CompId, $className]);
		if ($result) {
			while ($tmp = mysqli_fetch_array($result)) {
				$ret[] = $tmp;
			}
			mysqli_free_result($result);
		} else {
			echo(mysqli_error($this->m_Conn));
		}
		return $ret;
	}
	
	function getResultsForClass($className): ?array
	{
		return $this->getSplitsForClass($className, 1000);
	}
	
	
	function getLastPassings($num): array
	{
		$ret = array();
		$q = "SELECT runners.Name, runners.class, runners.Club, results.Time,results.Status,
				results.Changed, results.Control, splitcontrols.name as pname
				FROM results
				INNER JOIN runners on results.DbId = runners.DbId
				LEFT JOIN splitcontrols on (splitcontrols.code = results.Control and splitcontrols.tavid=results.tavid and runners.class = splitcontrols.classname)
				WHERE results.TavId = ? AND runners.TavId = results.TavId and
					results.Status <> -1 AND results.Time <> -1 AND results.Status <> 9 and results.Status <> 10
					and results.control <> 100 and (results.control = 1000 or splitcontrols.tavid is not null)
				ORDER BY results.changed desc
				limit ?";
		if ($result = $this->m_Conn->execute_query($q, [$this->m_CompId, $num])) {
			while ($row = mysqli_fetch_array($result)) {
				$ret[] = $row;
				if ($this->m_TimeDiff != 0) {
					$ret[sizeof($ret) - 1]["Changed"] = date("Y-m-d H:i:s", strtotime($ret[sizeof($ret) - 1]["Changed"]) + $this->m_TimeDiff);
				}
			}
			mysqli_free_result($result);
		} else {
			die(mysqli_error($this->m_Conn));
		}
		return $ret;
	}
	
	function getSplitsForClass($className, $split): array
	{
		$ret = array();
		$q = "SELECT runners.Name, runners.Club, results.Time,results.Status, results.Changed
				From runners
				JOIN results USING (dbid, tavid)
				where runners.TavId = ? AND runners.Class = ? and
				      results.Status <> -1 AND (results.Time <> -1 or (results.Time = -1 and (results.Status = 2 or results.Status=3))) AND results.Control = ?
				ORDER BY results.Status, results.Time";
		if ($result = $this->m_Conn->execute_query($q, [$this->m_CompId, $className, $split])) {
			while ($row = mysqli_fetch_array($result)) {
				$ret[] = $row;
			}
			mysqli_free_result($result);
		} else {
			die(mysqli_error($this->m_Conn));
		}
		return $ret;
	}
	
	function getClubResults($compId, $club): array
	{
		$ret = array();
		$q = "SELECT runners.Name, runners.Club, results.Time, runners.Class ,results.Status, results.Changed, results.DbID, results.Control
				, (select count(*)+1 from results sr, runners sru where sr.tavid=sru.tavid and sr.dbid=sru.dbid and sr.tavid=results.TavId and sru.class = runners.class and sr.status = 0 and sr.time < results.time and sr.Control=1000) as place
				, results.Time - (select min(time) from results sr, runners sru where sr.tavid=sru.tavid and sr.dbid=sru.dbid and sr.tavid=results.TavId and sru.class = runners.class and sr.status = 0 and sr.Control=1000) as timeplus
				From runners,results
				where results.DbID = runners.DbId AND results.TavId = ? AND runners.TavId = ? and runners.Club = ? and (results.Control=1000 or results.Control=100)
				ORDER BY runners.Class, runners.Name";
		if ($result = $this->m_Conn->execute_query($q, [$this->m_CompId, $this->m_CompId, $club])) {
			while ($row = mysqli_fetch_array($result)) {
				$dbId = $row['DbID'];
				if (!isset($ret[$dbId])) {
					$ret[$dbId] = array();
					$ret[$dbId]["DbId"] = $dbId;
					$ret[$dbId]["Name"] = $row['Name'];
					$ret[$dbId]["Club"] = $row['Club'];
					$ret[$dbId]["Class"] = $row['Class'];
					$ret[$dbId]["Time"] = "";
					$ret[$dbId]["TimePlus"] = "";
					$ret[$dbId]["Status"] = "9";
					$ret[$dbId]["Changed"] = "";
					$ret[$dbId]["Place"] = "";
				}
				
				$split = $row['Control'];
				if ($split == 1000) {
					$ret[$dbId]["Time"] = $row['Time'];
					$ret[$dbId]["Status"] = $row['Status'];
					$ret[$dbId]["Changed"] = $row['Changed'];
					$ret[$dbId]["Place"] = $row['place'];
					$ret[$dbId]["TimePlus"] = $row['timeplus'];
				} elseif ($split == 100) {
					$ret[$dbId]["start"] = $row['Time'];
				}
			}
			mysqli_free_result($result);
		} else {
			die(mysqli_error($this->m_Conn));
		}
		return $ret;
	}
	
	function getAllSplitsForClass($className): array
	{
		$ret = array();
		$q = "SELECT runners.Name, runners.Club, results.Time ,results.Status, results.Changed, results.DbID, results.Control
					FROM runners
					JOIN results USING (dbid, tavid)
					WHERE runners.TavId = ? AND runners.Class = ? ORDER BY results.Dbid";
		if ($result = $this->m_Conn->execute_query($q, [$this->m_CompId, $className])) {
			while ($row = mysqli_fetch_array($result)) {
				$dbId = $row['DbID'];
				if (!isset($ret[$dbId])) {
					$ret[$dbId] = array();
					$ret[$dbId]["DbId"] = $dbId;
					$ret[$dbId]["Name"] = $row['Name'];
					$ret[$dbId]["Club"] = $row['Club'];
					$ret[$dbId]["Time"] = "";
					$ret[$dbId]["Status"] = "9";
					$ret[$dbId]["Changed"] = "";
				}
				$split = $row['Control'];
				if ($split == 1000) {
					$ret[$dbId]["Time"] = $row['Time'];
					$ret[$dbId]["Status"] = $row['Status'];
					$ret[$dbId]["Changed"] = $row['Changed'];
				} elseif ($split == 100) {
					$ret[$dbId]["start"] = $row['Time'];
				} else {
					$ret[$dbId][$split."_time"] = $row['Time'];
					$ret[$dbId][$split."_status"] = $row['Status'];
					$ret[$dbId][$split."_changed"] = $row['Changed'];
				}
			}
			mysqli_free_result($result);
		} else {
			die(mysqli_error($this->m_Conn));
		}
		function timeSorter($a, $b)
		{
			if ($a['Status'] != $b['Status']) {
				return $a['Status'] - $b['Status'];
			} else {
				return $a['Time'] - $b['Time'];
			}
		}
		
		usort($ret, 'timeSorter');
		return $ret;
	}
	
	function getTotalResultsForClass($className): array
	{
		$ret = array();
		$ar = array();
		if ($this->m_MultiDayParent == -1) {
			$comps = "(".$this->m_CompId.")";
		} else {
			$q = "Select TavId,multidaystage from login where MultiDayParent = ? and MultiDayStage <=? order by multidaystage";
			$comps = "(";
			if ($result = $this->m_Conn->execute_query($q, [$this->m_MultiDayParent, $this->m_MultiDayStage])) {
				$f = 1;
				while ($row = mysqli_fetch_array($result)) {
					$ar[$row["TavId"]] = $row["TavId"];
					if ($f == 0)
						$comps .= ",";
					$comps .= $row["TavId"];
					$f = 0;
				}
			}
			mysqli_free_result($result);
			$comps .= ")";
		}
		
		$q = "SELECT results.Time, results.Status, results.TavId, results.DbID
				From runners
				JOIN results USING (dbid, tavid)
				where results.Control = 1000 and results.TavId in $comps AND runners.Class = ?
				ORDER BY results.Dbid";
		if ($result = $this->m_Conn->execute_query($q, [$className])) {
			while ($row = mysqli_fetch_array($result)) {
				$dbId = $row['DbID'];
				if (!isset($ret[$dbId])) {
					$ret[$dbId] = array();
					$ret[$dbId]["DbId"] = $dbId;
					$ret[$dbId]["Time"] = 0;
					$ret[$dbId]["Status"] = 0;
					foreach ($ar as $c) {
						$ret[$dbId]["c_".$c] = false;
					}
				}
				
				$ret[$dbId]["Time"] += (int)$row['Time'];
				$status = (int)$row['Status'];
				if ($status > $ret[$dbId]["Status"]) {
					$ret[$dbId]["Status"] = $status;
				}
				$ret[$dbId]["c_".$row['TavId']] = true;
			}
			
			mysqli_free_result($result);
			
			//print_r($ret);
			
			/*set DNS on those missing any comp*/
			foreach ($ret as $key => $val) {
				$haveAll = true;
				foreach ($ar as $c) {
					if (!$val["c_".$c]) {
						$haveAll = false;
						break;
					}
				}
				if (!$haveAll) {
					$ret[$key]['Status'] = 1;
				}
			}
		} else {
			die(mysqli_error($this->m_Conn));
		}
		
		$sres = array();
		foreach ($ret as $key => $res) {
			$sres[$res["DbId"]]["DbId"] = $res["DbId"];
			$sres[$res["DbId"]]["Time"] = $res["Time"];
			$sres[$res["DbId"]]["Status"] = $res["Status"];
		}
		usort($sres, 'timeSorter');
		$pl = 0;
		$lastTime = -1;
		$bestTime = -1;
		
		foreach ($sres as $tr) {
			if ($tr['Status'] == 0) {
				if ($bestTime == -1)
					$bestTime = $tr['Time'];
				if ($tr['Time'] > $lastTime)
					$pl++;
				$ret[$tr['DbId']]["Place"] = $pl;
				$ret[$tr['DbId']]["TotalPlus"] = $tr['Time'] - $bestTime;
			} else {
				$ret[$tr['DbId']]["Place"] = "-";
				$ret[$tr['DbId']]["TotalPlus"] = 0;
			}
		}
		return $ret;
	}
	
	public static function DeleteRunner(int $compid, int $dbid): bool
	{
		$conn = self::openConnection();
		$conn->begin_transaction();
		try {
			$conn->execute_query("delete from results where tavid= ? and dbid = ?", [$compid, $dbid]);
			$conn->execute_query("delete from runners where tavid= ? and dbid = ?", [$compid, $dbid]);
			$conn->execute_query("delete from runneraliases where compid= ? and id = ?", [$compid, $dbid]);
			$conn->commit();
			return true;
		} catch (mysqli_sql_exception) {
			$conn->rollback();
			return false;
		}
	}
	
	public static function UpdateRunner(int $compid, string $name, string $club, string $classname, string $dbid, ?string $sourceid, ?string $bib): void
	{
		$conn = self::openConnection();
		$conn->begin_transaction();
		try {
			$q = "REPLACE INTO runners (tavid,name,club,class,brick,dbid,bib) VALUES (?,?,?,?,0,?,?)";
			$conn->execute_query($q, [$compid, $name, $club, $classname, $dbid, $bib == "" ? null : $bib]);
			if ($sourceid !== "") {
				$q = "REPLACE INTO runneraliases (compid,sourceid,id) VALUES (?,?,?)";
				$conn->execute_query($q, [$compid, $sourceid, $dbid]);
			}
			$conn->commit();
		} catch (mysqli_sql_exception $e) {
			$conn->rollback();
		}
	}
	
	public static function UpdateRunnerResults(int $compid, int $dbid, string $time, int $code, int $status): void
	{
		$conn = self::openConnection();
		$q = "REPLACE INTO results (tavid,dbid,control,time,status,changed) VALUES(?,?,?,?,?,Now())";
		$conn->execute_query($q, [$compid, $dbid, $code, $time, $status]);
	}
	
}
