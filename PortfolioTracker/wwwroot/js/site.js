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

    const confirmModalElement = document.getElementById('confirmActionModal');
    if (confirmModalElement) {
        const confirmModal = new bootstrap.Modal(confirmModalElement);
        const titleElement = document.getElementById('confirmActionModalLabel');
        const messageElement = document.getElementById('confirmActionModalMessage');
        const yesButton = document.getElementById('confirmActionModalYesButton');
        let pendingForm = null;

        document.querySelectorAll('form[data-confirm-modal]').forEach(function(form) {
            form.addEventListener('submit', function(event) {
                event.preventDefault();
                pendingForm = form;

                titleElement.textContent = form.dataset.confirmTitle || 'Confirm action';
                messageElement.textContent = form.dataset.confirmMessage || 'Are you sure?';
                yesButton.textContent = form.dataset.confirmAction || 'Yes';
                yesButton.className = 'btn btn-' + (form.dataset.confirmVariant || 'primary');

                confirmModal.show();
            });
        });

        yesButton.addEventListener('click', function() {
            if (pendingForm) {
                confirmModal.hide();
                pendingForm.submit();
                pendingForm = null;
            }
        });

        confirmModalElement.addEventListener('hidden.bs.modal', function() {
            pendingForm = null;
        });
    }
});
