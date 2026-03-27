<?php
// index.php
date_default_timezone_set('Asia/Manila');

require_once 'app/init.php'; // Bootstrap file to load necessary files

// Get the request URL path (excluding the query string)
$url = isset($_GET['url']) ? $_GET['url'] : '';

// Load the router and dispatch the request
$router = new Router($url);
$router->dispatch();




