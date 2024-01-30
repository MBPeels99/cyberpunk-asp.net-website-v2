document.getElementById('bookingButton').addEventListener('click', function () {
    @if (!isLoggedIn) {
        // Show custom dialog
        if (confirm("You need to be logged in to make a booking. Do you want to log in now?")) {
            window.location.href = '@Url.Action("Login", "User")'; // Redirect to login page
        }
    }
    else {
        window.location.href = '@Url.Action("Create", "Booking")'; // Redirect to booking creation page
    }
});