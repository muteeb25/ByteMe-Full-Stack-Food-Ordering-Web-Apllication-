document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.getElementById('lumina-sidebar');
    const toggle = document.getElementById('sidebar-toggle');
    if (toggle && sidebar) {
        toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
    }

    const search = document.getElementById('dashboard-search');
    if (search) {
        search.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                const q = search.value.trim();
                if (q) window.location.href = `/Admin/FoodItems?search=${encodeURIComponent(q)}`;
            }
        });
    }

    // Food items page
    const addBtn = document.getElementById('btn-add-item');
    const addModal = document.getElementById('add-modal');
    if (addBtn && addModal) {
        addBtn.addEventListener('click', () => addModal.classList.add('open'));
        addModal.querySelectorAll('[data-close]').forEach(b => {
            b.addEventListener('click', () => addModal.classList.remove('open'));
        });
        addModal.addEventListener('click', (e) => {
            if (e.target === addModal) addModal.classList.remove('open');
        });
    }

    const foodSearch = document.getElementById('food-search');
    const foodTable = document.getElementById('food-table');
    if (foodSearch && foodTable) {
        foodSearch.addEventListener('input', () => {
            const q = foodSearch.value.toLowerCase();
            foodTable.querySelectorAll('tbody tr[data-name]').forEach(row => {
                const name = row.dataset.name || '';
                const cat = row.dataset.category || '';
                row.style.display = (name.includes(q) || cat.includes(q)) ? '' : 'none';
            });
        });
    }

    document.querySelectorAll('.avail-toggle').forEach(toggle => {
        toggle.addEventListener('change', async () => {
            const id = toggle.dataset.id;
            const res = await fetch(`/Admin/ToggleFoodAvailability?id=${id}`, { method: 'POST' });
            const data = await res.json();
            if (!data.success) toggle.checked = !toggle.checked;
        });
    });

    const editPanel = document.getElementById('edit-panel');
    const editOverlay = document.getElementById('edit-overlay');
    const closeEdit = document.getElementById('close-edit');
    const editForm = document.getElementById('edit-form');

    function openEdit() {
        editPanel?.classList.add('open');
        editOverlay?.classList.add('open');
    }

    function closeEditPanel() {
        editPanel?.classList.remove('open');
        editOverlay?.classList.remove('open');
    }

    closeEdit?.addEventListener('click', closeEditPanel);
    editOverlay?.addEventListener('click', closeEditPanel);

    document.querySelectorAll('.btn-edit').forEach(btn => {
        btn.addEventListener('click', async () => {
            const id = btn.dataset.id;
            const res = await fetch(`/Admin/GetFoodItem?id=${id}`);
            const item = await res.json();
            document.getElementById('edit-id').value = item.id;
            document.getElementById('edit-name').value = item.name;
            document.getElementById('edit-desc').value = item.description || '';
            document.getElementById('edit-price').value = item.price;
            document.getElementById('edit-image').value = item.imageUrl || '';
            document.getElementById('edit-category').value = item.categoryId;
            document.getElementById('edit-available').checked = item.isAvailable;
            openEdit();
        });
    });

    editForm?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const fd = new FormData(editForm);
        if (token) fd.append('__RequestVerificationToken', token);
        const res = await fetch('/Admin/EditFoodItemAjax', { method: 'POST', body: fd });
        const data = await res.json();
        if (data.success) location.reload();
    });
});
