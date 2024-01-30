function toggleDescription(button) {
    // Find the <p> element containing the description
    var description = button.previousElementSibling;

    // Toggle the 'collapsed-text' class
    description.classList.toggle('collapsed-text');

    // Change the button text accordingly
    if (description.classList.contains('collapsed-text')) {
        button.textContent = "Read More";
    } else {
        button.textContent = "Read Less";
    }
}

