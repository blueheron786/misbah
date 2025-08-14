// Misbah Note-Taking Application - BlazorDesktop Entry Point
// This file loads the main API with cache busting

// Cache busting timestamp
const apiVersion = new Date().getTime();

// Create a promise to track when the API is loaded
window.misbahApiReady = new Promise((resolve) => {
    const apiScript = document.createElement('script');
    apiScript.src = `js/api.js?v=${apiVersion}`;
    apiScript.async = false;
    
    // Ensure API is ready before resolving
    apiScript.onload = function() {
        // Wait a bit to ensure the API has fully initialized
        setTimeout(() => {
            if (window.misbah && window.misbah.api && window.blazorHelpers) {
                console.log(`Misbah BlazorDesktop API fully ready: v=${apiVersion}`);
                resolve();
            } else {
                console.error('API loaded but objects not found');
                resolve(); // Still resolve to prevent hanging
            }
        }, 10);
    };
    
    apiScript.onerror = function() {
        console.error('Failed to load Misbah API');
        resolve(); // Resolve even on error to prevent hanging
    };
    
    document.head.appendChild(apiScript);
});

console.log(`Misbah BlazorDesktop API loading with cache busting: v=${apiVersion}`);
