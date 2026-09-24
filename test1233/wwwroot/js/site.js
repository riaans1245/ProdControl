    $(document).ready(function () {
    // 1. Automatically show the loader whenever ANY form in the app is submitted
    $('form').on('submit', function () {
        // If the form inputs are valid, show the full-screen spinner overlay
        if ($(this).valid()) {
            $('#global-loader').removeClass('d-none');
        }
    });

    // 2. Automatically show the loader if they click direct links that might take time
    $('.show-loader-link').on('click', function () {
        $('#global-loader').removeClass('d-none');
    });
});