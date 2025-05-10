<?php
include_once("../templates/classEmma.class.php");
session_start();

if (isset($_POST["compid"], $_POST["email"], $_POST["password"])) {
	$is_valid = Emma::validLoginForComp($_POST["compid"], $_POST["email"], $_POST["password"]);
    if ($is_valid) {
        $_SESSION["competitions"][$_POST["compid"]] = 1;
        if (isset($_POST["returnto"])) {
			header("Location: ".$_POST["returnto"]);
		} else {
			header("Location: editComp.php?compid=".$_POST["compid"]);
        }
    } else {
		header("Location: loginComp.php?compid=".$_POST["compid"]);
	}
    exit();
}

include_once("../templates/emmalang_sv.php");
$lang = "en";
if (isset($_GET['lang']) && $_GET['lang'] != "") {
	$lang = $_GET['lang'];
}

include_once("../templates/emmalang_$lang.php");


header('Content-Type: text/html; charset='.$CHARSET);

?>
<!DOCTYPE html>
<html lang="<?=$lang?>">

<head><title><?=$_TITLE?></title>
	
	<link rel="stylesheet" type="text/css" href="../css/style.css">
	<meta name="robots" content="noindex">
	<meta http-equiv="Content-Type" content="text/html;charset=<?= $CHARSET ?>">
</head>

<body topmargin="0" leftmargin="0">

<!-- MAIN DIV -->

<div class="maindiv">
	<table width="759" cellpadding="0" cellspacing="0" border="0" ID="Table6">
		<tr>
		<tr>
			<td>
			</td>
		</tr>
	</table>
	<table border="0" cellpadding="0" cellspacing="0" width="759">
		<tr>
			<td valign="bottom">
				<!-- MAIN MENU FLAPS - Two rows, note that left and right styles differs from middle ones -->
				<table border="0" cellpadding="0" cellspacing="0">
					<!-- Top row with rounded corners -->
					<tr>
						<td colspan="4"><span class="mttop"></td>
					</tr>
				</table>
			</td>
			<td align="right" valign="bottom">
			</td>
		</tr>
		<tr>
			<td class="submenu" colspan="2">
				<table border="0" cellpadding="0" cellspacing="0">
					<tr>
						<td><a href="admincompetitions.php">Adminpage Competitionindex</a> |</td>
						<td><a href="../index.php"><?= $_CHOOSECMP ?> to view</a></td>
					</tr>
				</table>
			</td>
		</tr>
		<!-- End SUB MENU -->
		<tr>
			<td class="searchmenu" colspan="2" style="padding: 5px;">
				<table border="0" cellpadding="0" cellspacing="0" width="400">
					<tr>
						<td>
							<form name="form1" action="loginComp.php?compid=<?= $_GET['compid'] ?>"
								  method="post">
                                <input type="hidden" name="returnto" value="<?=htmlspecialchars($_GET["returnto"])?>">
								<b><label for="emailfield">Email</label></b><br>
								<input type="text" id="emailfield" name="email" size="30"><br>
								<b><label for="passwordfield">Password</label></b><br>
								<input type="password" id="passwordfield" name="password" size="30"><br>
								<input type="submit" name="btnLogin" value="Login">
							</form>
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</table>
</div>
<br>
<br>
</body>
</html>
