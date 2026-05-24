function getAntiForgeryToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
}

function updateCartBadge(count) {
    const badge = document.getElementById('cart-badge');
    const floatBadge = document.getElementById('float-cart-badge');
    const n = count ?? 0;
    if (badge) badge.textContent = n;
    if (floatBadge) {
        floatBadge.textContent = n;
        floatBadge.style.display = n > 0 ? 'flex' : 'none';
    }
}

function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = 'toast' + (type === 'error' ? ' error' : '');
    toast.textContent = message;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.3s';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

async function addToCart(foodItemId, quantity = 1) {
    const formData = new FormData();
    formData.append('foodItemId', foodItemId);
    formData.append('quantity', quantity);

    try {
        const res = await fetch('/Cart/AddToCart', { method: 'POST', body: formData });
        const data = await res.json();

        if (data.success) {
            updateCartBadge(data.cartCount);
            showToast('Added to cart!');
        } else {
            showToast(data.message || 'Could not add to cart.', 'error');
            if (data.message && data.message.includes('log in')) {
                setTimeout(() => { window.location.href = '/Account/Login'; }, 1500);
            }
        }
    } catch {
        showToast('Something went wrong.', 'error');
    }
}

async function removeFromCart(cartItemId, rowElement) {
    const formData = new FormData();
    formData.append('cartItemId', cartItemId);

    try {
        const res = await fetch('/Cart/RemoveFromCart', { method: 'POST', body: formData });
        const data = await res.json();

        if (data.success) {
            if (rowElement) rowElement.remove();
            updateCartBadge(data.cartCount);
            showToast('Item removed.');
            recalculateCartTotal();
            if (document.querySelectorAll('.cart-table tbody tr').length === 0) {
                location.reload();
            }
        }
    } catch {
        showToast('Could not remove item.', 'error');
    }
}

async function updateQuantity(cartItemId, qty, pricePerItem) {
    if (qty < 0) return;

    const formData = new FormData();
    formData.append('cartItemId', cartItemId);
    formData.append('quantity', qty);

    try {
        const res = await fetch('/Cart/UpdateQuantity', { method: 'POST', body: formData });
        const data = await res.json();

        if (data.success) {
            if (data.removed) {
                const row = document.getElementById('cart-row-' + cartItemId);
                if (row) row.remove();
                location.reload();
                return;
            }

            const qtyEl = document.getElementById('qty-' + cartItemId);
            const totalEl = document.getElementById('total-' + cartItemId);
            if (qtyEl) qtyEl.textContent = qty;
            if (totalEl) totalEl.textContent = 'Rs. ' + Math.round(data.newTotal).toLocaleString();

            recalculateCartTotal();
        }
    } catch {
        showToast('Could not update quantity.', 'error');
    }
}

function recalculateCartTotal() {
    let subtotal = 0;
    document.querySelectorAll('.cart-table tbody tr').forEach(row => {
        const price = parseFloat(row.dataset.price) || 0;
        const qtyEl = row.querySelector('.qty-value');
        const qty = qtyEl ? parseInt(qtyEl.textContent, 10) : 0;
        subtotal += price * qty;
    });

    const delivery = typeof deliveryFee !== 'undefined' ? deliveryFee : 150;
    const subtotalEl = document.getElementById('subtotal');
    const grandEl = document.getElementById('cart-grand-total');

    if (subtotalEl) subtotalEl.textContent = 'Rs. ' + Math.round(subtotal).toLocaleString();
    if (grandEl) grandEl.textContent = 'Rs. ' + Math.round(subtotal + delivery).toLocaleString();
}
