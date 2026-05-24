(function () {
    const card = document.getElementById('track-card');
    if (!card) return;

    const orderId = card.dataset.orderId;
    const statusText = document.getElementById('track-status-text');
    const steps = ['Placed', 'Preparing', 'Ready', 'Delivered'];

    function updateUI(status) {
        if (statusText) statusText.textContent = status;
        const idx = steps.indexOf(status);
        document.querySelectorAll('.progress-step').forEach((el, i) => {
            el.classList.toggle('active', i <= idx);
        });
        document.querySelectorAll('.progress-line').forEach((el, i) => {
            el.classList.toggle('active', i < idx);
        });
    }

    async function refresh() {
        try {
            const res = await fetch(`/Order/GetOrderStatus?orderId=${orderId}`);
            const data = await res.json();
            if (data.success) updateUI(data.status);
        } catch { /* ignore */ }
    }

    setInterval(refresh, 30000);
})();
