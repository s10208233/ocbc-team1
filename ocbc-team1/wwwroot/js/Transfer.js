// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function changingOptions() {
    clearInputs();
    var x = document.getElementById("PhoneNumberDiv")
    var y = document.getElementById("AccountNumberDiv")
    if (y.style.display = "none") {
        y.style.display = "block";
        if (x.style.display = "block") {
            x.style.display = "none";
        }
    }
}

function changingOptions2() {
    clearInputs();
    var x = document.getElementById("PhoneNumberDiv")
    var y = document.getElementById("AccountNumberDiv")
    if (x.style.display = "none") {
        x.style.display = "block";
        if (y.style.display = "block") {
            y.style.display = "none";
        }
    }
    
}

function clearInputs() {
    document.getElementById('PHinput').value = ''
    document.getElementById('ACinput').value = ''

}

$(document).ready(function () {
    // Number
    $('#ScheduledDate').datepicker({
        minDate: -3
    });
});