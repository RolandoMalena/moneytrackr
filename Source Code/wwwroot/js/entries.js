/// <reference path="../lib/jquery/dist/jquery.js" />

var entries = (function () {
	//Get Users
	let getUsers = function () {
		//Do the request
		request("/api/" + apiVersion + "/Users", "GET")
			.done(function (users) {
				//Iterate all users
				$.each(users, function (index, u) {
					//If this User is the logged in User, ignore it since a User can't edit itself
					if (u.userName.toUpperCase() == sessionStorage.getItem("username").toUpperCase())
						return;

					content.find("#Users").append('"<option value="' + u.userName + '">' + u.userName + '</option>');
				});
			}).fail(function (response) {
				toastr.error(response);
			});
	}

	//Get and Filter Entries
	let getEntries = function (form) {
		let frm = $(form);
		let errorMsg = $("#filterValidation");

		//Build model
		let model = {
			from: frm.find("#From").val(),
			to: frm.find("#To").val()
		};

		//If 'To' is lower than 'From, display error and return
		if (Date.parse(model.to) < Date.parse(model.from)) {
			errorMsg.text("The field 'To' can't be lower than 'From'.");
			return false;
		}

		let user = frm.find("#Users").val();
		let tableBody = content.find("#entries tbody");
		let details = content.find("#details");

		//At this point, clear the error message and hide content
		errorMsg.text('');
		tableBody.empty();
		content.append($(loadingHTML).clone().addClass("animate-bottom"));

		//Get the HTML for each Row
		getHTML("/Entries/GetRow", function (result) {
			let rowTemplate = result;

			//Do the request
			request("/api/" + apiVersion + "/Users/" + user + "/Entries/Report?from=" + model.from + "&to=" + model.to, "GET")
				.done(async function (result) {
					details.find("#balance").text(formatCurrenty(result.currentBalance));
					details.find("#totalEntries").text(result.entryCount);
					details.find("#totalDeposits").text(formatCurrenty(result.totalDeposits));
					details.find("#countDeposits").text(result.depositCount);
					details.find("#totalWithdrawals").text(formatCurrenty(result.totalWithdraws).replace('-', ''));
					details.find("#countWithdrawals").text(result.withdrawCount);
					content.find("#currentUser").val(user);

					//If there are not entries, nothing else needs to be done
					if (result.entries.length == 0) {
						toastr.warning("No entries where to be found.");
						return;
					}

					//Iterate through all the entries
					result.entries.forEach(function (e) {
						//Grab the rowTemplate and replace the placeholders with the User values
						let row = rowTemplate;
						row = replaceAll(row, '{Id}', e.id);
						row = replaceAll(row, '{Date}', dateFormatter.format(new Date(e.date)));
						row = replaceAll(row, '{Description}', e.description);
						row = replaceAll(row, '{Amount}', formatCurrenty(e.amount));
						row = replaceAll(row, '{Balance}', formatCurrenty(e.balance));
						
						//Append the row into the table body
						tableBody.append(row);
					});

				}).fail(function (response) {
					toastr.error(response.responseText);
				})
				.always(function () {
					details.show();
					content.find("#loading.animate-bottom").remove();
				});
		});

		return false;
	}

	//Open Modal Form to add a new Entry or save an existing Entry
	let loadForm = function (id = null) {
		//Declare ModalHandler and shows it
		let handler = new ModalHandler("modal");
		handler.show(id == null ? "New Entry" : "Edit Entry #" + id, $(loadingHTML), false);

		//Get the Form 
		getHTML('/Entries/GetForm' + (id == null ? "" : "/" + id), function (html) {
			//If we are attempting to create a user...
			if (id == undefined && id == null) {
				//Set modal content and return
				handler.setForm(html);
				return;
			}

			let username = content.find("#currentUser").val();

			//Otherwise attempt to get the entry and fill the input fields
			request("/api/" + apiVersion + "/Users/" + username + "/Entries/" + id, "GET")
				.done(function (entry) {
					let formContent = $(html);

					formContent.find("#Id").val(entry.id);
					formContent.find("#Date").val(entry.date.substr(0,10));
					formContent.find("#Description").val(entry.description);
					formContent.find("#Amount").val(formatCurrenty(entry.amount));

					//Set modal content
					handler.setForm(formContent);
				}).fail(function (response) {
					toastr.error(response);
					return;
				});
		});
	}

	//Creates or Updates an Entry
	let saveEntry = function (form) {
		let frm = $(form);

		//If form is not valid, return
		if (!frm.valid())
			return false;

		//Hide the content and show a loading message
		let handler = new ModalHandler("modal");
		handler.toggleContentVisibility(false);

		//Prepare the model object to be sent
		let model = {
			Date: frm.find("#Date").val(),
			Amount: parseFloat(replaceAll(frm.find("#Amount").val(), ',', '')),
			Description: frm.find("#Description").val()
		}

		let id = frm.find("#Id").val();
		let user = content.find("#currentUser").val();

		let isNewEntry = parseInt(id) == NaN;

		//Do the request
		request("/api/" + apiVersion + "/Users/" + user + "/Entries/" + (isNewEntry ? "" : id), id == "" ? "POST" : "PUT", JSON.stringify(model))
			.done(function (result) {
				//Hide modal
				handler.hide();

				//Show message accordingly
				if (isNewEntry) {
					toastr.success("Entry created successfully!");
				}
				else {
					toastr.success("Entry updated successfully!");
				}

				//Reload the main table
				content.find("#searchForm").submit();
			})
			.fail(function (jqXHR) {
				//Display error in case Username is already taken
				handler.toggleContentVisibility(true);
				frm.find("#summaryErrors").text(jqXHR.responseText);
				return;
			});

		return false;
	}

	//Delete Entry
	let deleteEntry = function (id) {
		//Shows the modal with the warning
		let handler = new ModalHandler("modal");
		handler.show("Delete Entry #" + id, "Are you sure you want to delete this entry?");

		//On Submit...
		handler.onSubmit(function () {
			//Hide buttons and show loading message
			handler.content.html($(loadingHTML));
			handler.toggleButtonsVisibility(false);

			let user = content.find("#currentUser").val();

			request("/api/" + apiVersion + "/Users/" + user + "/Entries/" + id, "DELETE")
				.done(function () {
					//Remove row from table
					let table = content.find("#entries");
					table.find("tbody tr[name='" + id + "']").remove();

					//Show success message
					toastr.success("Entry deleted successfully!");
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
		getUsers: getUsers,
		getEntries: getEntries,
		loadForm: loadForm,
		saveEntry: saveEntry,
		deleteEntry: deleteEntry
	}
})();