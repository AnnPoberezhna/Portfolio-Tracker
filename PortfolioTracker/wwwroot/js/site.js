// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    const darkModeToggle = document.getElementById('darkModeToggle');
    if (darkModeToggle) {
        // Load saved preference
        const isDarkMode = localStorage.getItem('darkMode') === 'true';
        document.body.classList.toggle('dark-mode', isDarkMode);
        darkModeToggle.checked = isDarkMode;

        // Toggle event
        darkModeToggle.addEventListener('change', function() {
            const isChecked = this.checked;
            document.body.classList.toggle('dark-mode', isChecked);
            localStorage.setItem('darkMode', isChecked);
        });
    }
});
