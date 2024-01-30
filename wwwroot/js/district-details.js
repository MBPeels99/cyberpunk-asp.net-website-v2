document.addEventListener("DOMContentLoaded", function () {
    // Now using the global variable 'imageUrls' defined in the .cshtml file
    let currentImageIndex = 0;

    const districtBackgroundDiv = document.getElementById('districtBackground');

    function changeBackgroundImage() {
        if (imageUrls.length === 0) {
            return; // No images to display
        }

        currentImageIndex = (currentImageIndex + 1) % imageUrls.length; // Cycle through image indices
        const imageUrl = imageUrls[currentImageIndex];
        districtBackgroundDiv.style.backgroundImage = `url('${imageUrl}')`;
    }

    // Change background image every 5 seconds
    setInterval(changeBackgroundImage, 5000);
});
