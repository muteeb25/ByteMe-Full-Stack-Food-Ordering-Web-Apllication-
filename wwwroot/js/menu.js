document.addEventListener('DOMContentLoaded', () => {
    const tabs = document.querySelectorAll('#category-tabs .tab-btn');
    const searchParams = new URLSearchParams(window.location.search);
    const currentSearch = searchParams.get('search');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const categoryId = tab.dataset.categoryId;
            const url = new URL('/Menu', window.location.origin);
            if (categoryId) url.searchParams.set('categoryId', categoryId);
            if (currentSearch) url.searchParams.set('search', currentSearch);
            window.location.href = url.toString();
        });
    });

    const liveSearch = document.getElementById('menu-live-search');
    const cards = document.querySelectorAll('.food-searchable');
    const noResults = document.getElementById('menu-no-results');

    if (liveSearch && cards.length) {
        liveSearch.addEventListener('input', () => {
            const q = liveSearch.value.trim().toLowerCase();
            let visible = 0;
            cards.forEach(card => {
                const name = card.dataset.name || '';
                const desc = card.dataset.desc || '';
                const cat = card.dataset.category || '';
                const match = !q || name.includes(q) || desc.includes(q) || cat.includes(q);
                card.style.display = match ? '' : 'none';
                if (match) visible++;
            });
            if (noResults) noResults.style.display = visible === 0 ? 'block' : 'none';
        });
    }
});
