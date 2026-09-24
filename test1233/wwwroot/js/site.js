   
    $(document).ready(function () {
    
    // Intercept form submissions
    $('form').on('submit', function (e) {
        
        // Only run if the form passes validation checks
        if ($(this).valid()) {
            
            // 1. Prevent the form from submitting instantly
            e.preventDefault();
            var currentForm = this;

            // 2. Un-hide the full-screen spinner overlay
            $('#global-loader').removeClass('d-none');

            // 3. Track two independent conditions: 
            //    A) Has 2 seconds passed? 
            //    B) Is the browser ready to submit?
            var timerFinished = false;

            // Start the 2-second countdown clock
            setTimeout(function () {
                timerFinished = true;
                submitWhenReady();
            }, 2000); // 2000 milliseconds = 2 seconds

            function submitWhenReady() {
                if (timerFinished) {
                    // 4. Finally release the form to submit to the database.
                    // If the server takes *longer* than 2 seconds to load, 
                    // the spinner will naturally stay on screen until the page changes.
                    currentForm.submit();
                }
            }
        }
    });
});