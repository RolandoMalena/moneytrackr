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
				//Display error in case Username is already taken
				frm.find("#summaryErrors").text(jqXHR.responseText);
			}).always(showContent);

		return false;
	}

	//Loads all Users
	let loadUsers = function () {
		content.append($(loadingHTML).clone().addClass("animate-bottom"));
		
		//Get the HTML for each Row
		getHTML("/Users/GetRow", function (result) {
			let rowTemplate = result;
			let tableBody = $("#users tbody");

			//Do the request
			request("/api/Users", "GET")
				.done(function (users) {
					//Iterate all users
					$.each(users, function (index, u) {
						//If this User is the logged in User, ignore it since a User can't edit itself
						if (u.userName.toUpperCase() == sessionStorage.getItem("username").toUpperCase())
							return;

						//Grab the rowTemplate and replace the placeholders with the User values
						let row = replaceAll(replaceAll(rowTemplate, '{UserName}', u.userName), '{Role.Name}', u.role.name);

						//Append the row into the table body
						tableBody.append(row);
					});
				}).fail(function (response) {
					toastr.error(response);
				});
		});

		content.find("#loading").remove();
	}

	//Open Modal Form to add a new User or save an existing User
	let loadForm = function (username = null) {
		//Declare ModalHandler and shows it
		let handler = new ModalHandler("modal");
		let isNewUser = username == undefined || username == null || username == "";
		handler.show(isNewUser ? "New User" : "Edit User '" + username + "'", $(loadingHTML), false);

		//Get the Form
		getHTML('/Users/GetForm', function (html) {
			//If we are attempting to create a user...
			if (isNewUser) {
				//Set modal content, hide password warning and return
				html = $(html);
				html.find("#passwordWarning").hide();
				handler.setForm(html);
				return;
			}

			//Otherwise attempt to get the user and fill the input fields
			request("/api/Users/" + username, "GET")
				.done(function (user) {
					let formContent = $(html);

					formContent.find("#Id").val(username);
					formContent.find("#Username").val(username);
					formContent.find("#RoleId").val(user.role.id);

					//Set modal content
					handler.setForm(formContent);
				}).fail(function (response) {
					toastr.error(response);
					return;
				});
		});
	}

	//Creates or Updates a User
	let saveUser = function (form) {
		let frm = $(form);

		//If form is not valid, return
		if (!frm.valid())
			return false;

		//Hide the content and show a loading message
		let handler = new ModalHandler("modal");
		handler.toggleContentVisibility(false);

		//Prepare the model object to be sent and retrieve the original username
		let model = {
			Username: frm.find("#Username").val(),
			RoleId: frm.find("#RoleId").val()
		}

		let password = frm.find("#Password").val();
		if (password != "")
			model.Password = password;

		let originalUsername = frm.find("#Id").val();
		let roleName = frm.find("#RoleId option:selected").text();
		let isNewUser = originalUsername == "";

		//Do the request
		request("/api/Users/" + originalUsername, isNewUser ? "POST" : "PUT", JSON.stringify(model))
			.done(function (result) {
				//Get the HTML for each Row
				getHTML("/Users/GetRow", function (html) {
					//Hide modal
					handler.hide();

					//Set animation class to the row and add the User data
					html = replaceAll(replaceAll(html, '{UserName}', model.Username), '{Role.Name}', roleName);
					let row = $(html).addClass("animatehighlight");

					//If a User was created, add into the table and show message
					if (isNewUser) {
						content.find("#users tbody").prepend(row);
						toastr.success("User created successfully!");
					}
					//Else, User was updated, replace row and show message
					else {
						content.find("#users tbody tr[name='" + originalUsername + "']").replaceWith(row);
						toastr.success("User updated successfully!");
					}
				});
			})
			.fail(function (jqXHR) {
				//Display error in case Username is already taken
				handler.toggleContentVisibility(true);
				frm.find("#summaryErrors").text(jqXHR.responseText);
				return;
			});

		return false;
	}

	//Delete User
	let deleteUser = function (username) {
		//Shows the modal with the warning
		let handler = new ModalHandler("modal");
		handler.show("Delete User '" + username + "'", "Are you sure you want to delete this user?");

		//On Submit...
		handler.onSubmit(function () {
			//Hide buttons and show loading message
			handler.content.html($(loadingHTML));
			handler.toggleButtonsVisibility(false);

			request("api/Users/" + username, "DELETE")
				.done(function () {
					toastr.success("User deleted successfully!")
					content.find("#users tbody tr[name='" + username + "']").remove();
				})
				.always(function () {
					handler.hide();
				})
				.fail(function (jqXHR) {
					//Display error
					toastr.error(jqXHR.responseText);
				});
		});
	}

	return {
		logIn: logIn,
		logOff: logOff,
		register: register,
		loadUsers: loadUsers,
		loadForm: loadForm,
		saveUser: saveUser,
		deleteUser: deleteUser
	}
})();