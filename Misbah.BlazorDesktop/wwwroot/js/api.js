// Misbah Note-Taking Application - BlazorDesktop JavaScript API
// Organized under misbah.api namespace for clean code organization

// Initialize the Misbah API namespace
window.misbah = window.misbah || {};
window.misbah.api = {
    // Internal state management
    _internal: {
        currentSaveFunction: null,
        isInitialized: false
    },

    // Core keyboard functionality
    keyboard: {
        /**
         * Register a save function to be called on Ctrl+S
         * @param {Object} dotNetRef - Blazor component reference
         * @param {string} methodName - Method name to invoke
         */
        registerSaveFunction: function(dotNetRef, methodName) {
            window.misbah.api._internal.currentSaveFunction = function() {
                dotNetRef.invokeMethodAsync(methodName);
            };
            console.log('Save function registered for Ctrl+S');
        },
        
        /**
         * Unregister the current save function
         */
        unregisterSaveFunction: function() {
            window.misbah.api._internal.currentSaveFunction = null;
            console.log('Save function unregistered');
        },
        
        /**
         * Handle Ctrl+S keyboard shortcut
         * @param {KeyboardEvent} event - The keyboard event
         */
        handleCtrlS: function(event) {
            event.preventDefault();
            event.stopPropagation();
            
            if (typeof window.misbah.api._internal.currentSaveFunction === 'function') {
                window.misbah.api._internal.currentSaveFunction();
            } else if (typeof console !== 'undefined') {
                console.log('Ctrl+S pressed but no save function is registered.');
            }
            
            return false;
        },
        
        /**
         * Handle refresh prevention (Ctrl+R, F5)
         * @param {KeyboardEvent} event - The keyboard event
         */
        handleRefreshPrevention: function(event) {
            // Disable Ctrl+R (refresh)
            if (event.ctrlKey && event.key === 'r') {
                event.preventDefault();
                event.stopPropagation();
                
                if (typeof console !== 'undefined') {
                    console.log('Page refresh disabled to prevent losing content. Use the browser refresh button if needed.');
                }
                
                return false;
            }
            
            // Disable F5 (refresh)
            if (event.key === 'F5') {
                event.preventDefault();
                event.stopPropagation();
                
                if (typeof console !== 'undefined') {
                    console.log('Page refresh disabled to prevent losing content. Use cache busting: Use the browser refresh button if needed.');
                }
                
                return false;
            }
            
            return true;
        },

        /**
         * Main keyboard event handler that coordinates all keyboard shortcuts
         * @param {KeyboardEvent} event - The keyboard event
         */
        handleKeydown: function(event) {
            // Handle Ctrl+S (save)
            if (event.ctrlKey && event.key === 's') {
                return window.misbah.api.keyboard.handleCtrlS(event);
            }
            
            // Handle refresh prevention (Ctrl+R, F5)
            return window.misbah.api.keyboard.handleRefreshPrevention(event);
        }
    },

    // Content protection utilities
    protection: {
        /**
         * Shows a warning before page unload if there might be unsaved content
         * @param {Event} event - The beforeunload event
         */
        handleBeforeUnload: function(event) {
            // Only show warning if we detect potential unsaved content
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
        }
    },

    /**
     * Toast notification utilities
     */
    toast: {
        /**
         * Show a toast notification
         * @param {string} message - The message to display
         * @param {string} type - The type of toast: 'success', 'error', 'info'
         * @param {number} duration - Duration in milliseconds (default: 3000)
         */
        show: function(message, type = 'success', duration = 3000) {
            const container = document.getElementById('toast-container') || this.createContainer();
            
            const toast = document.createElement('div');
            toast.className = `toast toast-${type}`;
            
            const icon = type === 'success' ? '✓' : type === 'error' ? '✗' : 'ℹ';
            toast.innerHTML = `
                <span class="toast-icon">${icon}</span>
                <span class="toast-message">${message}</span>
            `;
            
            container.appendChild(toast);
            
            // Trigger animation
            setTimeout(() => {
                toast.classList.add('show');
            }, 10);
            
            // Auto-remove
            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => {
                    if (container.contains(toast)) {
                        container.removeChild(toast);
                    }
                }, 300);
            }, duration);
        },

        /**
         * Create the toast container if it doesn't exist
         */
        createContainer: function() {
            const container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            document.body.appendChild(container);
            return container;
        },

        /**
         * Show a success toast
         * @param {string} message - The success message
         */
        success: function(message) {
            this.show(message, 'success');
        },

        /**
         * Show an error toast
         * @param {string} message - The error message
         */
        error: function(message) {
            this.show(message, 'error');
        },

        /**
         * Show an info toast
         * @param {string} message - The info message
         */
        info: function(message) {
            this.show(message, 'info');
        }
    },

    /**
     * Initialize the misbah API with all event listeners and protections
     */
    init: function() {
        // Set up keyboard shortcuts
        document.addEventListener('keydown', window.misbah.api.keyboard.handleKeydown);
        
        // Set up page unload protection - with safety check
        if (window.misbah.api.protection && typeof window.misbah.api.protection.handleBeforeUnload === 'function') {
            window.addEventListener('beforeunload', window.misbah.api.protection.handleBeforeUnload);
        } else {
            console.warn('Protection module not properly initialized');
        }
        
        console.log('Misbah API initialized - refresh shortcuts disabled, Ctrl+S save enabled');
    }
};

// Legacy compatibility layer - maintain backward compatibility with existing code
window.blazorHelpers = {
    /**
     * Register a save function to be called on Ctrl+S
     * @param {Object} dotNetRef - .NET object reference
     * @param {string} methodName - Method name to invoke
     */
    registerSaveFunction: function(dotNetRef, methodName) {
        window.currentSaveFunction = function() {
            dotNetRef.invokeMethodAsync(methodName);
        };
    },
    
    /**
     * Unregister the save function (call when component is destroyed)
     */
    unregisterSaveFunction: function() {
        window.currentSaveFunction = null;
    }
};

// Auto-initialize when DOM is ready with race condition protection
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        if (window.misbah && window.misbah.api && typeof window.misbah.api.init === 'function') {
            window.misbah.api.init();
        } else {
            console.error('Misbah API not properly initialized - init function not found');
        }
    });
} else {
    if (window.misbah && window.misbah.api && typeof window.misbah.api.init === 'function') {
        window.misbah.api.init();
    } else {
        console.error('Misbah API not properly initialized - init function not found');
    }
}
