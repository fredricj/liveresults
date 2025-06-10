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


// Login to competition
if (isset($_POST['method']) && $_POST['method'] === 'authenticate') {
    if (!isset($_POST['compid']) || !isset($_POST['user']) || !isset($_POST['password'])) {
        http_response_code(400);
        exit();
    }
    $is_valid = Emma::validLoginForComp($_POST["compid"], $_POST["user"], $_POST["password"]);
    if ($is_valid) {
        session_start();
        $session_id = session_id();
        $_SESSION["competition"] = $_POST["compid"];
        echo json_encode(["status" => "OK", "session_id" => $session_id]);
    } else {
        http_response_code(401);
        echo("{ \"status\": \"ERR\", \"message\": \"Invalid credentials for competition {$_POST["compid"]}\"}");
    }
    exit();
}

if (!array_key_exists('HTTP_APISESSIONID', $_SERVER)) {
    http_response_code(401);
    echo("{ \"status\": \"ERR\", \"message\": \"missing auth header\"}");
    exit();
} else {
    $session_id = $_SERVER['HTTP_APISESSIONID'];
    if (!is_string($session_id)) {
        http_response_code(401);
        exit();
    }
    session_id($session_id);
    session_start();
    $comp = null;
    if (isset($_POST['comp'])) {
        $comp = $_POST["comp"];
    } else if (isset($_GET['comp'])) {
        $comp = $_GET['comp'];
    } else {
        http_response_code(400);
        exit();
    }
    if (!isset($_SESSION["competition"]) || $_SESSION["competition"] !== $comp) {
        http_response_code(401);
        exit();
    }
}

try {
    if ($_SERVER['REQUEST_METHOD'] === "GET") {
        $method = $_GET["method"];
        if (!is_string($method)) {
            http_response_code(400);
            exit();
        }
        if ($method === 'validate') {
            // Already validated return 200
            http_response_code(200);
            exit();
        } elseif ($method === 'getcompetitionresultdata') {
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
        if ($method === 'validate') {
            // Already validated return 200
            http_response_code(200);
            exit();
        } elseif ($method === 'updateradiocontrol') {
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
