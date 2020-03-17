/// <reference path="../lib/jquery/dist/jquery.js" />

//HTML for the loading message to be used globaly
const loadingHTML = $("#loading")[0];

//Content div, everything will be placed inside it
const content = $("#content");

//Formatter used for numbers to be displayed in currency format
const currencyFormatter = new Intl.NumberFormat('en', {
	style: 'currency',
	currency: 'USD'
})

//Formatter used for dates
const dateFormatter = Intl.DateTimeFormat('en', {
	year: 'numeric',
	month: '2-digit',
	day: '2-digit'
});

//Updates content dynamically, with the option to override
function navigate(targetLocation = "") {
	//Default hash value
	let hash = "#Home";

	//If a target location is given, set it as the new hash value
	if (targetLocation)
		hash = targetLocation;

	//else, set location.hash value
	else if (location.hash)
		hash = location.hash;

	//Remove the '#' from the hash value
	hash = hash.substr(1);

	//Get the content
	getContent(hash);
}

//Gets the content based on the hash value and roleName
function getContent(hash) {
	//Set loading message while getting the HTML
	setContent(loadingHTML);

	hash = hash.toLowerCase().trim();

	//Set the End Point based on the hash value
	let endpoint =
		hash == "about" ? "/Home/GetAboutPage" :
			hash == "entries" ? "/Entries" :
				hash == "manage" ? "/Manage" :
					hash == "users" ? "/Users" :
						hash == "home" ? "/Home/GetHomePage" : "/Home/GetNotFoundPage";

	//Redirect the User based on whether or not they are logged in and where they attemp to go
	let role = sessionStorage.getItem("roleName");

	if (role == null) {
		switch (hash) {
			case "entries":
			case "manage":
			case "users": {
				location.hash = "#Home";
				return;
			}
		}
	}
	else {
		if (hash == "home") {
			location.hash = role == "Regular User" ? "Entries" : "Users";
			return;
		}
		else if (hash == "users" && role == "Regular User") {
			location.hash = "Entries";
			return;
		}
	}

	//Get the HTML and after loading, set it as the content and set the active link
	getHTML(endpoint, function (result) {
		setContent(result);

		//Depending on the hash value, call the corresponding functions
		if (hash == "Entries") {
			content.find("#From, #To").val(new Date().toISOString().substr(0, 10));
			entries.getReport();
		}
	});
	setActiveLink(hash);
}

//Sets the content
function setContent(html) {
	content.html(html);

	//Enable validation for any form within the content
	$("form").each(function () {
		$.validator.unobtrusive.parse(this);
	});
}

//Set link to active
function setActiveLink() {
	$("#navbarContent .nav-link").each(function () {
		let link = $(this);
		let hash = location.hash == "" ? "#Home" : location.hash;

		if (link.attr('href') == hash)
			link.addClass("active");
		else
			link.removeClass("active");

		document.title = hash.substr(1) + " - MoneyTrackr";
	});
}

//Get the HTML from a PartialView based on the EndPoint
function getHTML(endPoint, callback) {
	let headers = {};
	let jwt = sessionStorage.getItem("jwt");

	if (jwt != null)
		headers.Authorization = "Bearer " + jwt;

	$.ajax({
		headers: headers,
		datatype: "text/html",
		type: "GET",
		url: endPoint,
		cache: true,
		success: function (result) {
			callback(result);
		}
	}).done(function (data, textStatus, request) {
		//Look for response header 'X-Responded-JSON'
		//If null, everything goes as expected, else...
		//If got a response 401, token timeout has passed, log off
		let header = request.getResponseHeader("X-Responded-JSON");
		if (header != null && jQuery.parseJSON(header).status == 401)
			users.logOff();
	});
}

//Hides the content and saves it to show the loading message
function hideContent() {
	content.children().hide();
	content.prepend($(loadingHTML));
}

//Removes the loading message and shows the hidden content
function showContent() {
	content.find("#loading").remove();
	//Make sure not to select a modal, since it will cause a bug
	content.children().not(".modal").show();
}

//Returns a default template for AJAX requests
function request(url, type = 'GET', data = null, contentType = "application/json", additionalHeaders = [], cache = false) {
	let headers = {};
	let jwt = sessionStorage.getItem("jwt");

	if (jwt != null)
		headers.Authorization = "Bearer " + jwt;

	//If Additional Headers are given, merge them into the headers variable
	if (additionalHeaders.length > 0) {
		additionalHeaders.map((item) => {
			headers[item.key] = item.value;
		});
	}

	//Wire up the request and return the requestObject
	return $.ajax(url, {
		headers: headers,
		type: type,
		data: data,
		cache: cache,
		contentType: contentType
	});
}

//Shows and/or Hides the links based on the user logged in (if there is)
function setNavigationLinks() {
	//Get the RoleName and Links to work with
	let roleName = sessionStorage.getItem("roleName");
	let links = $(".navbar-nav");

	//Show or Hide the links based if user is authenticated and authorization
	if (roleName == null) {
		links.find("#lnkHome").show();

		links.find("#lnkUsers").hide();
		links.find("#lnkEntries").hide();
		links.find("#lnkManage").hide();
		links.find("#lnkLogOff").hide();
	}
	else {
		links.find("#lnkHome").hide();

		links.find("#lnkEntries").show();
		links.find("#lnkLogOff").show();

		if (roleName == "Regular User")
			links.find("#lnkUsers").hide();
		else
			links.find("#lnkUsers").show();

		//Finally, shows the username
		links.find("#lnkManage a").html("Hello " + sessionStorage.getItem("username") + "!");
		links.find("#lnkManage").show();
	}
}

//Disables every input field and button on the content
function disableInputs() {
	content.find("input button").addClass("disabled");
}

//Enables every input field and button on the content
function enableInputs() {
	content.find("input button").removeClass("disabled");
}

//Navigate on load, set navigation links
navigate();
setNavigationLinks();

//Listen to hashchange event to navigate
$(window).on("hashchange", function () {
	navigate();
});

//Helper function to replace all occurances
function replaceAll(text, searchValue, valueToReplace) {
	return text.split(searchValue).join(valueToReplace);
}

//Helper function to format string to number
function formatCurrenty(str) {
	return currencyFormatter.format(str).replace('$', '');
}

//Setting up toastr
toastr.options.progressBar = true;
toastr.options.hideDuration = 300;
toastr.options.showMethod = "slideDown";
toastr.options.hideMethod = "slideUp";