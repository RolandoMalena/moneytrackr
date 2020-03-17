/// <reference path="../lib/jquery/dist/jquery.js" />

var manage = (function () {
	//Changes the username of the current user
	let changeUsername = function (form) {
		let frm = $(form);

		//If form is not valid, return
		if (!frm.valid())
			return false;

		//Hide the content and show a loading message
		hideContent();

		//Prepare the model object to be sent
		let model = {
			Username: frm.find("#UserName").val(),
			CurrentPassword: frm.find("#CurrentPassword").val()
		}

		//Do the request
		request("/api/Manage/ChangeUsername", "PATCH", JSON.stringify(model))
			.done(function () {
				//Message the user, empty every field and clear validation errors
				toastr.success("Username changed successfully.");
				frm.find("input[type!='submit']").val('');
				frm.find("#summaryErrors").text('');

				sessionStorage.setItem("username", model.Username);
				setNavigationLinks();
			}).fail(function (jqXHR) {
				//Display error in case the current password is wrong
				frm.find("#summaryErrors").text(jqXHR.responseText);
			}).always(showContent);

		return false;
	}

	//Changes the password of the current user
	let changePassword = function (form) {
		let frm = $(form);

		//If form is not valid, return
		if (!frm.valid())
			return false;

		//Hide the content and show a loading message
		hideContent();

		//Prepare the model object to be sent
		let model = {
			CurrentPassword: frm.find("#CurrentPassword").val(),
			NewPassword: frm.find("#NewPassword").val()
		}

		//Do the request
		request("/api/Manage/ChangePassword", "PATCH", JSON.stringify(model))
			.done(function () {
				//Message the user, empty every field and clear validation errors
				toastr.success("Password changed successfully.");
				frm.find("input[type!='submit']").val('');
				frm.find("#summaryErrors").text('');
			}).fail(function (jqXHR) {
				//Display error in case the current password is wrong
				frm.find("#summaryErrors").text(jqXHR.responseText);
			}).always(showContent);

		return false;
	}

	return {
		changeUsername: changeUsername,
		changePassword: changePassword
	}
})();