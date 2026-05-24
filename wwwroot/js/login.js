document.addEventListener('DOMContentLoaded', () => {
    const page = document.getElementById('login-page');
    const roleInput = document.getElementById('role-input');
    const roleTabs = document.querySelectorAll('.role-tab');
    const hint = document.getElementById('login-hint');
    const formFields = document.getElementById('form-fields');

    roleTabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const role = tab.dataset.role;
            roleInput.value = role;

            roleTabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');

            page.classList.remove('theme-customer', 'theme-admin');
            page.classList.add(role === 'Admin' ? 'theme-admin' : 'theme-customer');

            if (hint) {
                hint.textContent = role === 'Admin'
                    ? 'Demo admin: admin@byteme.com / Admin@123'
                    : 'Demo customer: user@byteme.com / User@123';
            }

            if (formFields) {
                formFields.style.animation = 'none';
                formFields.offsetHeight;
                formFields.style.animation = '';
            }
        });
    });
});
