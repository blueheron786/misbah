window.setCultureFromLocalStorage = function() {
    var culture = localStorage.getItem('misbah-culture');
    if (culture) {
        window.__misbahCulture = culture;
    }
};
window.setCultureFromLocalStorage();
