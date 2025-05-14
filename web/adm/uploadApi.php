<?php
if (PHP_SAPI === "cli") {
	parse_str(getenv('QUERY_STRING'), $_GET);
}
date_default_timezone_set("Europe/Stockholm");

include_once(__DIR__."/../templates/classEmma.class.php");

header('content-type: application/json; charset='.$CHARSET);
header('Access-Control-Allow-Origin: *');
header('cache-control: max-age=15');
header('pragma: public');
header('Expires: '.gmdate('D, d M Y H:i:s \G\M\T', time() + 15));

if (!isset($_GET['method'])) {
	$_GET['method'] = null;
}
if ($_GET['method'] === 'getcompetitionresultdata') {
	$currentComp = new Emma($_GET['comp']);
	$splitcontrols = $currentComp->getAllSplitControls();
	// Since an empty array becomes an array instead of dict
	$splits = new ArrayObject();
	foreach ($splitcontrols as $sc) {
		if (!$splits->offsetExists($sc["classname"])) {
			$splits[$sc["classname"]] = [];
		}
		$splits[$sc["classname"]][] = $sc;
	}
	$aliases = $currentComp->GetCompetitionRunnerAliases();
	$results = $currentComp->GetCompetitionResults();
	echo json_encode(["splitcontrols" => $splits, "runneraliases" => $aliases, "results" => $results], JSON_THROW_ON_ERROR);
} elseif ($_POST['method'] === 'updateradiocontrol') {
	Emma::UpdateRadioControl($_POST["comp"], $_POST["classname"], $_POST["cname"], $_POST["code"], $_POST["corder"]);
	echo json_encode("{\"status\": \"OK\"}");
} elseif ($_POST['method'] === 'deleteradiocontrol') {
	Emma::DeleteRadioControl($_POST["comp"], $_POST["classname"], $_POST["corder"], $_POST["code"], $_POST["cname"]);
	echo json_encode("{\"status\": \"OK\"}");
} elseif ($_POST['method'] === 'deleterunner') {
	Emma::DeleteRunner($_POST["comp"], $_POST["dbid"]);
} elseif ($_POST['method'] === 'updaterunner') {
	Emma::UpdateRunner($_POST["comp"], $_POST["name"], $_POST["club"], $_POST["classname"], $_POST["dbid"], $_POST["sourceid"], $_POST["bib"] ?? null);
} elseif ($_POST['method'] === 'updaterunnerresults') {
	Emma::UpdateRunnerResults($_POST["comp"], $_POST["dbid"], $_POST["time"], 1000, $_POST["status"]);
} elseif ($_POST['method'] === 'updaterunnerstarttime') {
	Emma::UpdateRunnerResults($_POST["comp"], $_POST["dbid"], $_POST["starttime"], 100, $_POST["status"]);
} elseif ($_POST['method'] === 'updaterunnersplittimes') {
	Emma::UpdateRunnerResults($_POST["comp"], $_POST["dbid"], $_POST["time"], $_POST["code"], 0);
} else {
	http_response_code(400);
	echo("{ \"status\": \"ERR\", \"message\": \"No method given\"}");
}
