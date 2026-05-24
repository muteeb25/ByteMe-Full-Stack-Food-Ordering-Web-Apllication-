document.addEventListener('DOMContentLoaded', () => {
    const navbar = document.getElementById('navbar');
    if (navbar) {
        const onScroll = () => {
            navbar.classList.toggle('navbar-scrolled', window.scrollY > 40);
        };
        onScroll();
        window.addEventListener('scroll', onScroll, { passive: true });
    }

    const floatBadge = document.getElementById('float-cart-badge');
    const navBadge = document.getElementById('cart-badge');
    const count = navBadge ? parseInt(navBadge.textContent, 10) || 0 : 0;
    if (floatBadge) floatBadge.textContent = count;
    if (floatBadge) floatBadge.style.display = count > 0 ? 'flex' : 'none';
});
