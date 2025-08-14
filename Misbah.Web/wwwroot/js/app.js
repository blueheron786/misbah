// App-level JavaScript functionality

// Disable page refresh shortcuts to prevent losing content
document.addEventListener('keydown', function(event) {
    // Disable Ctrl+R and Ctrl+Shift+R (refresh)
    if ((event.ctrlKey && event.key === 'r') || 
        (event.ctrlKey && event.shiftKey && event.key === 'R')) {
        event.preventDefault();
        event.stopPropagation();
        
        // Show a notification instead
        if (typeof console !== 'undefined') {
            console.log('Page refresh disabled to prevent losing content. Use the browser refresh button if needed.');
        }
        
        // You could also show a toast notification here if you have a notification system
        return false;
    }
    
    // Disable F5 (refresh)
    if (event.key === 'F5') {
        event.preventDefault();
        event.stopPropagation();
        
        if (typeof console !== 'undefined') {
            console.log('Page refresh disabled to prevent losing content. Use the browser refresh button if needed.');
        }
        
        return false;
    }
});

// Additional protection: warn before page unload if there might be unsaved content
window.addEventListener('beforeunload', function(event) {
    // Only show warning if we detect potential unsaved content
    // You might want to customize this logic based on your app's state
    const textareas = document.querySelectorAll('textarea');
    const inputs = document.querySelectorAll('input[type="text"]');
    
    let hasContent = false;
    textareas.forEach(textarea => {
        if (textarea.value && textarea.value.trim().length > 0) {
            hasContent = true;
        }
    });
    
    inputs.forEach(input => {
        if (input.value && input.value.trim().length > 0) {
            hasContent = true;
        }
    });
    
    if (hasContent) {
        event.preventDefault();
        event.returnValue = 'You may have unsaved changes. Are you sure you want to leave?';
        return 'You may have unsaved changes. Are you sure you want to leave?';
    }
});

console.log('App.js loaded - refresh shortcuts disabled');
