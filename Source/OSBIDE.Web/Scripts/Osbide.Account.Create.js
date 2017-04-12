$(document).ready(accountCreateDocumentReady);

function accountCreateDocumentReady() {
    $("#create-account-button").click(function (evt) {
        $("#User_Email").val($("#Email").val());
    });
}