// Listen for Ctrl+O and show/hide the search popup
(function() {
    window.addEventListener('keydown', function(e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'o' && !e.repeat) {
            e.preventDefault();
            window.dispatchEvent(new CustomEvent('show-search-popup'));
        }
    }, true);
})();
