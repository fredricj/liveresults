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

try {
    if ($_SERVER['REQUEST_METHOD'] === "GET") {
        $method = $_GET["method"];
        if (!is_string($method)) {
            http_response_code(400);
            exit();
        }
        if ($method === 'getcompetitionresultdata') {
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
        }
    } elseif ($_SERVER['REQUEST_METHOD'] === "POST") {
        $method = $_POST["method"];
        if (!is_string($method)) {
            http_response_code(400);
            exit();
        }
        if ($method === 'updateradiocontrol') {
            Emma::UpdateRadioControl($_POST["comp"], $_POST["classname"], $_POST["cname"], $_POST["code"], $_POST["corder"]);
            echo json_encode("{\"status\": \"OK\"}");
        } elseif ($method === 'deleteradiocontrol') {
            Emma::DeleteRadioControl($_POST["comp"], $_POST["classname"], $_POST["corder"], $_POST["code"], $_POST["cname"]);
            echo json_encode("{\"status\": \"OK\"}");
        } elseif ($method === 'deleterunner') {
            Emma::DeleteRunner($_POST["comp"], $_POST["dbid"]);
        } elseif ($method === 'updaterunner') {
            Emma::UpdateRunner($_POST["comp"], $_POST["name"], $_POST["club"], $_POST["classname"], $_POST["dbid"], $_POST["sourceid"], $_POST["bib"] ?? null);
        } elseif ($method === 'updaterunnerresults') {
            if ($_POST["finishTime"] === "") {
                $_POST["finishTime"] = null;
            }
            Emma::UpdateRunnerResults($_POST["comp"], $_POST["dbid"], $_POST["time"], 1000, $_POST["status"], $_POST["finishTime"]);
        } elseif ($method === 'updaterunnerstarttime') {
            Emma::UpdateRunnerResults($_POST["comp"], $_POST["dbid"], $_POST["starttime"], 100, $_POST["status"], null);
        } elseif ($method === 'updaterunnersplittimes') {
            Emma::UpdateRunnerResults($_POST["comp"], $_POST["dbid"], $_POST["time"], $_POST["code"], 0, $_POST["passingTime"]);
        } else {
            http_response_code(400);
            echo("{ \"status\": \"ERR\", \"message\": \"No method given\"}");
        }
    }
} catch (\Exception $e) {
    http_response_code(500);
    echo("{ \"status\": \"ERR\", \"message\": \"Internal server error\"}");
}
