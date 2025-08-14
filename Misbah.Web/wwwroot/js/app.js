// Misbah Note-Taking Application - Web Entry Point
// Simple cache busting approach

// Include the API directly with cache busting
document.write(`<script src="js/api.js?v=${Date.now()}"><\/script>`);

console.log(`Misbah Web API loaded with cache busting: v=${Date.now()}`);
