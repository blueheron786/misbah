// Misbah Note-Taking Application - Main Application JavaScript
// This file loads the main API with cache busting and provides any app-specific functionality

(function() {
    // Cache busting - append current timestamp to force reload of API file
    var script = document.createElement('script');
    script.src = '/js/api.js?v=' + Date.now();
    script.async = false; // Ensure synchronous loading for proper initialization order
    document.head.appendChild(script);
    
    console.log('Misbah Web App - API loaded with cache busting');
})();
