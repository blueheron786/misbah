/**
 * Misbah Application JavaScript API
 * Shared between Web and BlazorDesktop applications
 * Handles keyboard shortcuts, Blazor interop, and UI utilities
 */

// Initialize the global Misbah namespace
window.misbah = window.misbah || {};
window.misbah.api = window.misbah.api || {};
window.misbah.api._internal = window.misbah.api._internal || {};

// Main API initialization
window.misbah.api.init = function() {
    console.log('Misbah API initialized - refresh shortcuts disabled, Ctrl+S save enabled');
    
    // Initialize keyboard shortcuts
    window.misbah.api.keyboard.init();
    
    // Initialize protection utilities
    window.misbah.api.protection.init();
    
    // Initialize toast system
    window.misbah.api.toast.init();
    
    // Register universal Ctrl+S functionality for any page
    console.log('🌐 Registering universal Ctrl+S save function...');
    window.misbah.api._internal.currentSaveFunction = function() {
        console.log('💾 Universal save function called from any page!');
        
        // Show success toast
        window.misbah.api.toast.success('Universal save function executed! ✓');
        
        // Add any universal save logic here
        // For example: save current state, trigger auto-save, etc.
        
        // You can customize this based on current page
        const currentPath = window.location.pathname;
        console.log('💾 Saving from page:', currentPath);
        
        if (currentPath === '/') {
            console.log('💾 Home page save - could save app state, preferences, etc.');
        } else if (currentPath.startsWith('/notes/')) {
            console.log('💾 Notes page save - could save current note');
        } else {
            console.log('💾 Generic page save - could save any form data, etc.');
        }
    };
    
    console.log('✅ Universal save function registered and ready!');
};

// Keyboard shortcuts namespace
window.misbah.api.keyboard = {
    init: function() {
        // Remove any existing listeners to prevent duplicates
        document.removeEventListener('keydown', this.handleKeyDown);
        
        // Add keyboard event listener
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
    },
    
    /**
     * Main keyboard event handler
     * @param {KeyboardEvent} event - The keyboard event
     */
    handleKeyDown: function(event) {
        // Handle Ctrl+S (Save)
        if (event.ctrlKey && event.key === 's') {
            this.handleCtrlS(event);
            return;
        }
        
        // Handle F5 and Ctrl+R (Refresh prevention)
        if (event.key === 'F5' || (event.ctrlKey && event.key === 'r')) {
            this.handleRefreshPrevention(event);
            return;
        }
    },
    
    /**
     * Handle Ctrl+S keyboard shortcut
     * @param {KeyboardEvent} event - The keyboard event
     */
    handleCtrlS: function(event) {
        event.preventDefault();
        event.stopPropagation();
        
        console.log('🔥 Ctrl+S pressed at', new Date().toISOString());
        console.log('🔍 Current save function type:', typeof window.misbah.api._internal.currentSaveFunction);
        console.log('🔍 Current save function value:', window.misbah.api._internal.currentSaveFunction);
        console.log('🔍 blazorHelpers available:', typeof window.blazorHelpers);
        console.log('🔍 blazorHelpers.registerSaveFunction available:', typeof window.blazorHelpers?.registerSaveFunction);
        
        if (typeof window.misbah.api._internal.currentSaveFunction === 'function') {
            console.log('✅ Calling save function...');
            window.misbah.api._internal.currentSaveFunction();
        } else if (typeof console !== 'undefined') {
            console.log('❌ Ctrl+S pressed but no save function is registered.');
            
            // DEBUG: Show current page info
            console.log('🌐 Current URL:', window.location.href);
            console.log('🌐 Current pathname:', window.location.pathname);
            console.log('💡 This should not happen with universal save function registered');
        }
        
        return false;
    },
    
    /**
     * Handle refresh prevention (Ctrl+R, F5)
     * @param {KeyboardEvent} event - The keyboard event
     */
    handleRefreshPrevention: function(event) {
        // Only prevent refresh in certain contexts (like when editing)
        if (document.querySelector('.markdown-editor, .note-editor, textarea, input[type="text"]')) {
            event.preventDefault();
            event.stopPropagation();
            console.log('Refresh prevented - unsaved changes may exist');
            return false;
        }
    }
};

// Protection utilities namespace
window.misbah.api.protection = {
    init: function() {
        console.log('🛡️ Protection utilities disabled for development');
        
        // DEVELOPMENT: All protection disabled to allow debugging
        // Uncomment below for production builds
        
        /*
        // Disable context menu in production
        if (window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') {
            document.addEventListener('contextmenu', function(e) {
                e.preventDefault();
                return false;
            });
        }
        
        // Disable F12, Ctrl+Shift+I, Ctrl+U
        document.addEventListener('keydown', function(e) {
            if (e.key === 'F12' || 
                (e.ctrlKey && e.shiftKey && e.key === 'I') || 
                (e.ctrlKey && e.key === 'u')) {
                if (window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') {
                    e.preventDefault();
                    return false;
                }
            }
        });
        */
    }
};

// Toast notification system
window.misbah.api.toast = {
    init: function() {
        this.ensureToastContainer();
    },
    
    ensureToastContainer: function() {
        if (!document.getElementById('toast-container')) {
            const container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            document.body.appendChild(container);
        }
    },
    
    show: function(message, type = 'info', duration = 3000) {
        this.ensureToastContainer();
        
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <span class="toast-message">${message}</span>
                <button class="toast-close" onclick="this.parentElement.parentElement.remove()">&times;</button>
            </div>
        `;
        
        const container = document.getElementById('toast-container');
        container.appendChild(toast);
        
        // Animate in
        setTimeout(() => toast.classList.add('toast-show'), 10);
        
        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                toast.classList.remove('toast-show');
                setTimeout(() => {
                    if (toast.parentNode) {
                        toast.parentNode.removeChild(toast);
                    }
                }, 300);
            }, duration);
        }
        
        return toast;
    },
    
    success: function(message, duration = 3000) {
        return this.show(message, 'success', duration);
    },
    
    error: function(message, duration = 5000) {
        return this.show(message, 'error', duration);
    },
    
    warning: function(message, duration = 4000) {
        return this.show(message, 'warning', duration);
    },
    
    info: function(message, duration = 3000) {
        return this.show(message, 'info', duration);
    }
};

// Blazor helpers namespace (backwards compatibility layer)
window.blazorHelpers = window.blazorHelpers || {};

/**
 * Register a save function to be called when Ctrl+S is pressed
 * @param {object} dotNetRef - Reference to the .NET object
 * @param {string} methodName - Name of the method to call
 */
window.blazorHelpers.registerSaveFunction = function(dotNetRef, methodName) {
    console.log("=============== INSIDE registerSaveFunction - this means Blazor called us!")
    console.log('📝 [blazorHelpers] Registering save function:', methodName);
    console.log('📝 [blazorHelpers] DotNet reference:', dotNetRef);
    
    if (!dotNetRef || !methodName) {
        console.error('❌ [blazorHelpers] Invalid parameters for registerSaveFunction');
        return;
    }
    
    console.log("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@")
    // Store the save function
    window.misbah.api._internal.currentSaveFunction = function() {
        console.log('🚀 [blazorHelpers] Invoking', methodName, 'on', dotNetRef);
        try {
            dotNetRef.invokeMethodAsync(methodName);
        } catch (error) {
            console.error('❌ [blazorHelpers] Error invoking save method:', error);
        }
    };
    
    console.log('✅ [blazorHelpers] Save function registered successfully');
};

/**
 * Unregister the current save function
 */
window.blazorHelpers.unregisterSaveFunction = function() {
    console.log('🗑️ [blazorHelpers] Unregistering save function');
    window.misbah.api._internal.currentSaveFunction = null;
};

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.misbah.api.init();
    });
} else {
    window.misbah.api.init();
}
