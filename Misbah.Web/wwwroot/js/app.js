/**
 * Misbah Application Loader
 * Shared between Web and BlazorDesktop applications
 * Handles cache busting and synchronous loading of the API
 */

(function() {
    // Cache busting with timestamp
    const timestamp = Date.now();
    const version = timestamp;
    
    console.log('Misbah API loading with cache busting: v=' + version);
    
    // Use document.write for synchronous loading
    // This ensures the API is available immediately when Blazor components initialize
    document.write('<script src="js/api.js?v=' + version + '"><\/script>');
})();
