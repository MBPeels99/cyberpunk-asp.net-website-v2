document.addEventListener('DOMContentLoaded', function () {
    const password = document.querySelector('input[name="Password"]');
    const confirmPassword = document.querySelector('input[name="confirmPassword"]');
    const confirmPasswordError = document.querySelector('#confirmPasswordError');

    confirmPassword.addEventListener('input', validatePasswords);

    function validatePasswords() {
        if (password.value !== confirmPassword.value) {
            confirmPasswordError.textContent = "Passwords do not match.";
        } else {
            confirmPasswordError.textContent = '';
        }
    }
});