/// <reference path="../lib/jquery/dist/jquery.js" />

//This is a class that helps with the handling of modals with forms.
class ModalHandler {
	constructor(modalId) {
		//Get some references and save it into properties for later use
		this.modal = content.find("#" + modalId);
		this.title = this.modal.find(".modal-title");
		this.content = this.modal.find(".modal-body");
		this.footer = this.modal.find(".modal-footer");
		this.submit = this.footer.find("button[type='submit']");

		//On hide, remove every event listener to avoid duplicated calls
		//A reference of "this" object is NEEDED, otherwise you can't access properties
		let reference = this;
		this.modal.on("hide.bs.modal", function () {
			reference.submit.off();
			reference.modal.off();
		});
	}

	//Set title and content of the modal and shows it
	show(title, content, enableSubmit = true) {
		this.title.html(title);
		this.content.html(content);
		this.submit.toggleClass("disabled", !enableSubmit);

		this.modal.modal('show');
	}

	//Hide modal
	hide() {
		//Hides the modal
		this.modal.modal('hide');

		//Shows the buttons to reuse modal
		this.toggleButtonsVisibility(true);

		//Disable the submit button and removes every listener to the click event
		this.submit.off("click");
	}

	setForm(formHTML) {
		//Convert Form into a jQuery object
		let frm = $(formHTML);

		//Set modal content, enable validation of the form and enable submit button
		this.content.html(frm);
		$.validator.unobtrusive.parse(frm);
		this.submit.removeClass("disabled");

		//Make form submit when the submit button is clicked
		this.onSubmit(function () { frm.submit() });
	}

	//Calls the callback function passed in when the submit button is clicked
	onSubmit(callback) {
		this.submit.click(callback);
	}

	//Shows or Hides every button and footer in the modal, depending on the value given
	toggleButtonsVisibility(value) {
		if (value) {
			this.modal.find("button").show();
			this.footer.show();
		}
		else {
			this.modal.find("button").hide();
			this.footer.hide();
		}
	}

	//Shows or Hide the content and buttons of the Modal, depending on the value given
	toggleContentVisibility(value) {
		this.toggleButtonsVisibility(value);

		if (value) {
			this.content.find("#loading").remove();
			this.content.children().show();
		}
		else {
			this.content.children().hide();
			this.content.prepend($(loadingHTML));
		}
	}
}
