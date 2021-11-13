// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function changingOptions() {
    var x = document.getElementById("PhoneNumber")
    var y = document.getElementById("AccountNumber")
    if (y.style.display = "none") {
        y.style.display = "block";
        if (x.style.display = "block") {
            x.style.display = "none";
        }
    }
}

function changingOptions2() {
    var x = document.getElementById("PhoneNumber")
    var y = document.getElementById("AccountNumber")
    if (x.style.display = "none") {
        x.style.display = "block";
        if (y.style.display = "block") {
            y.style.display = "none";
        }
    }
    
}
