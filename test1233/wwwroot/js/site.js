// 1. SHOW SPINNER: Trigger overlay when user submits any data or navigates away
window.addEventListener("beforeunload", function () {
    var loader = document.getElementById("page-loader");
    if (loader) {
        loader.classList.remove("d-none"); // Removes Bootstrap's 'display: none' utility class
    }
});

// 2. HIDE SPINNER: Handle page caching edge cases (e.g. clicking the browser Back button)
window.addEventListener("pageshow", function (event) {
    if (event.persisted) {
        var loader = document.getElementById("page-loader");
        if (loader) {
            loader.classList.add("d-none"); // Hides it if browser pulled layout memory out of cache history
        }
    }
});