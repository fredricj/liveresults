<?php

function get_available_languages(): array
{
	return [
		"sv" => [
			"icon" => "images/se.png",
			"name" => "Svenska"
		],
		"en" => [
			"icon" => 'images/en.png',
			"name" => "English"
		],
		"fi" => [
			"icon" => 'images/fi.png',
			"name" => 'Suomeksi'
		],
		"ru" => [
			"icon" => 'images/ru.png',
			"name" => 'Русский'
		],
		"cz" => [
			"icon" => 'images/cz.png',
			"name" => 'Česky'
		],
		"de" => [
			"icon" => 'images/de.png',
			"name" => 'Deutsch'
		],
		"bg" => [
			"icon" => 'images/bg.png',
			"name" => 'български'
		],
		"fr" => [
			"icon" => 'images/fr.png',
			"name" => 'Français'
		],
		"it" => [
			"icon" => 'images/it.png',
			"name" => 'Italiano'
		],
		"hu" => [
			"icon" => 'images/hu.png',
			"name" => 'Magyar'
		],
		"es" =>
			["icon" => 'images/es.png',
			"name" => 'Español'
		],
		"pl" => [
			"icon" => 'images/pl.png',
			"name" => 'Polska'
		],
		"pt" => [
			"icon" => 'images/pt.png',
			"name" => 'Português'
		],
	];
}

function get_language_chooser(string $url_prefix, $lang): string {
    if (str_contains($url_prefix, "?")) {
        if (str_ends_with($url_prefix, "?")) {
	        $url_prefix .= "lang=";
        } else {
	        $url_prefix .= "&amp;lang=";
        }
    } else {
	    $url_prefix .= "?lang=";
    }
    ob_start();
    foreach (get_available_languages() as $lang_key => $lang_val) {
        ?>| <?php
        if ($lang != $lang_key) { ?>
            <a href="<?=$url_prefix?><?=$lang_key?>" style='text-decoration: none'>
        <?php } ?>
        <img src="<?=$lang_val["icon"]?>" border='0' alt="<?=$lang_val["name"]?>"> <?=$lang_val["name"]?>
        <?= $lang != $lang_key ? "</a>" : ""?>
        <?php
    }
    ?> |
	<?php
    return ob_get_clean();
}
