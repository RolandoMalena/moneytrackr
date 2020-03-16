/// <reference path="../lib/jquery/dist/jquery.js" />

var users = (function () {
	//Logs in the user
	let logIn = function (form) {
		let frm = $(form);

		//If form is not valid, return
		if (!frm.valid())
			return false;

		//Hide the content and show a loading message
		hideContent();

		//Prepare the request body
		let model = {
			Username: frm.find('input[name="Username"]').val(),
			Password: frm.find('input[name="Password"]').val()
		}

		//Do the request
		request("/api/users/login", "POST", JSON.stringify(model))
			.done(function (result) {

				sessionStorage.setItem("jwt", result.auth_token);

				//Extract and Decode the payload of the JWT to extract Username and Role
				let payload = result.auth_token.split(".")[1];
				let decodedPayloadString = atob(payload);
				let payloadObject = JSON.parse(decodedPayloadString);

				sessionStorage.setItem("roleName", payloadObject.role);
				sessionStorage.setItem("username", payloadObject.nameid);

				//If User is Regular User, send to the Entries Page, otherwise, to the Users Page
				location.hash = payloadObject.role == "Regular User" ? "#Entries" : "#Users";
				setNavigationLinks();
			})
			.fail(function (result) {
				//Display error in case Username and/or Password do not match
				frm.find("#summaryErrors").text(result.responseText);
				return;
			})
			.always(showContent);

		return false;
	}

	//Clears the session storage, sets the navigation links and sends the user to the Home page
	function logOff() {
		sessionStorage.clear();
		setNavigationLinks();
		location.hash = "#Home";
	}

	//Register a new user
	let register = function (form) {
		let frm = $(form);

		//If form is not valid, return
		if (!frm.valid())
			return false;

		//Hide the content and show a loading message
		hideContent();

		//Prepare the model object to be sent
		let model = {
			Username: frm.find("#Register-Username").val(),
			Password: frm.find("#Register-Password").val()
		}

		//Do the request
		request("/api/Users/Register", "POST", JSON.stringify(model))
			.done(function () {
				//Message the user, empty every field and clear validation errors
				toastr.success("User created successfully. You can now log in.");
				frm.find("input[type!='submit']").val('');
				frm.find("#summaryErrors").text('');
			}).fail(function (jqXHR) {
				//Display error in case Username and/or Email is already taken
				frm.find("#summaryErrors").text(jqXHR.responseText);
			}).always(showContent);

		return false;
	}

	//Open Modal Form to add a new User or save an existing User
	let loadForm = function (id = null) {
		//TODO
	}

	//Creates or Updates a User
	let saveUser = function (form) {
		//TODO
	}

	//Delete User
	let deleteUser = function (id) {
		//TODO
	}

	return {
		logIn: logIn,
		logOff: logOff,
		register: register,
		loadForm: loadForm,
		saveUser: saveUser,
		deleteUser: deleteUser
	}
})();