document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.btn-expand').forEach(btn => {
        btn.addEventListener('click', () => {
            const id = btn.dataset.expand;
            const detail = document.getElementById('detail-' + id);
            if (!detail) return;
            const hidden = detail.hasAttribute('hidden');
            if (hidden) {
                detail.removeAttribute('hidden');
                btn.classList.add('expanded');
            } else {
                detail.setAttribute('hidden', '');
                btn.classList.remove('expanded');
            }
        });
    });

    document.querySelectorAll('.status-select').forEach(select => {
        select.addEventListener('change', async () => {
            const orderId = select.dataset.orderId;
            const status = select.value;
            const fd = new FormData();
            fd.append('orderId', orderId);
            fd.append('status', status);
            const res = await fetch('/Admin/UpdateOrderStatus', { method: 'POST', body: fd });
            const data = await res.json();
            if (data.success) {
                select.closest('tr').querySelectorAll('.badge').forEach(() => {});
                const row = select.closest('.order-row');
                if (row) {
                    let badge = row.querySelector('.badge');
                    if (!badge) {
                        badge = document.createElement('span');
                        select.parentElement.appendChild(badge);
                    }
                }
            }
        });
    });
});
