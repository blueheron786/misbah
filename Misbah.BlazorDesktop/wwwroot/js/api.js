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
    
    // Register universal save function as fallback
    window.misbah.api.registerUniversalSave();
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
            
            // Try to help by retrying registration after a short delay
            console.log('🔄 Attempting to trigger delayed registration...');
            setTimeout(() => {
                if (typeof window.misbah.api._internal.currentSaveFunction !== 'function') {
                    console.log('⚠️ Save function still not available after delay');
                }
            }, 100);
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
    console.log('📝 [blazorHelpers] Registering save function:', methodName);
    console.log('📝 [blazorHelpers] DotNet reference:', dotNetRef);
    
    if (!dotNetRef || !methodName) {
        console.error('❌ [blazorHelpers] Invalid parameters for registerSaveFunction');
        return;
    }
    
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

// BlazorDesktop interop functions
window.misbah.api.desktopSaveInteropRef = null;

/**
 * Register BlazorDesktop save interop (C# object reference)
 * @param {object} dotNetRef - Reference to the .NET SaveInteropComponent
 */
window.misbah.api.registerDesktopSaveInterop = function(dotNetRef) {
    console.log('💻 [BlazorDesktop] Registering desktop save interop:', dotNetRef);
    
    if (!dotNetRef) {
        console.error('❌ [BlazorDesktop] Invalid dotNetRef for registerDesktopSaveInterop');
        return;
    }
    
    // Store the desktop interop reference
    window.misbah.api.desktopSaveInteropRef = dotNetRef;
    
    // Override the save function to use desktop interop
    window.misbah.api._internal.currentSaveFunction = async function() {
        console.log('💾 [BlazorDesktop] Invoking desktop save interop...');
        
        try {
            // Extract saveable content from current page
            const saveData = await window.misbah.api.extractSaveableContent();
            console.log('📄 [BlazorDesktop] Extracted save data:', saveData);
            
            // Call the C# SaveContent method
            const result = await dotNetRef.invokeMethodAsync('SaveContent', saveData);
            console.log('✅ [BlazorDesktop] Save result:', result);
            
            // Show success toast
            window.misbah.api.toast.success('💾 Saved successfully!');
            
            return result;
        } catch (error) {
            console.error('❌ [BlazorDesktop] Error in desktop save:', error);
            window.misbah.api.toast.error('❌ Save failed: ' + error.message);
            throw error;
        }
    };
    
    console.log('✅ [BlazorDesktop] Desktop save interop registered successfully');
};

/**
 * Unregister BlazorDesktop save interop
 */
window.misbah.api.unregisterDesktopSaveInterop = function() {
    console.log('🗑️ [BlazorDesktop] Unregistering desktop save interop');
    window.misbah.api.desktopSaveInteropRef = null;
    window.misbah.api._internal.currentSaveFunction = null;
    
    // Re-register universal save as fallback
    window.misbah.api.registerUniversalSave();
};

// Universal save functionality (fallback when no Blazor component is registered)
window.misbah.api.registerUniversalSave = function() {
    console.log('🌐 Registering universal save fallback...');
    
    // Only register if no save function is already registered
    if (!window.misbah.api._internal.currentSaveFunction) {
        window.misbah.api._internal.currentSaveFunction = async function() {
            console.log('💾 Universal save function called!');
            
            try {
                // Extract saveable content from current page
                const saveData = await window.misbah.api.extractSaveableContent();
                
                console.log('💾 Saving data:', saveData);
                
                // Call .NET backend to save to the same file
                const response = await fetch('/api/save', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(saveData)
                });
                
                if (response.ok) {
                    const result = await response.json();
                    window.misbah.api.toast.success(result.message || 'Content saved! ✓');
                    console.log('✅ Successfully saved to:', result.filePath);
                } else {
                    throw new Error(`Save failed: ${response.statusText}`);
                }
            } catch (error) {
                console.error('❌ Universal save error:', error);
                window.misbah.api.toast.error('Failed to save content');
            }
        };
    }
};

// Extract saveable content from current page
window.misbah.api.extractSaveableContent = async function() {
    const currentPath = window.location.pathname;
    
    // Try to find a specific file to save based on page context
    const textareas = document.querySelectorAll('textarea');
    const mainTextarea = textareas.length > 0 ? textareas[0] : null;
    
    if (mainTextarea && mainTextarea.value.trim()) {
        // If there's a main textarea (like a note editor), save its content
        
        // Try to get the actual file path from the page/component
        let actualFilePath = await window.misbah.api.getCurrentNoteFilePath();
        
        return {
            path: currentPath,
            timestamp: new Date().toISOString(),
            content: {
                filePath: actualFilePath,
                content: mainTextarea.value,
                type: 'note-content'
            }
        };
    } else {
        // Generic page content (form data, app state, etc.)
        const formData = {};
        
        // Extract all form inputs
        document.querySelectorAll('input, textarea, select').forEach((element, index) => {
            if (element.type !== 'password' && element.value) {
                const key = element.name || element.id || `element_${index}`;
                formData[key] = element.value;
            }
        });
        
        return {
            path: currentPath,
            timestamp: new Date().toISOString(),
            content: {
                type: 'page-content',
                formData: formData,
                url: window.location.href
            }
        };
    }
};

// Note file path management (for proper save-to-same-file functionality)
window.misbah.api.registerCurrentNoteFilePath = function(filePath) {
    console.log('📁 Registering current note file path:', filePath);
    window.misbah.api._internal.currentNoteFilePath = filePath;
};

window.misbah.api.unregisterCurrentNoteFilePath = function() {
    console.log('📁 Unregistering current note file path');
    window.misbah.api._internal.currentNoteFilePath = null;
};

window.misbah.api.getCurrentNoteFilePath = async function() {
    // Try to get the file path from a registered Blazor component
    if (window.misbah.api._internal.currentNoteFilePath) {
        console.log('📁 Using cached note file path:', window.misbah.api._internal.currentNoteFilePath);
        return window.misbah.api._internal.currentNoteFilePath;
    }
    
    // If no cached path, try to infer from URL
    const currentPath = window.location.pathname;
    console.log('🔍 Trying to infer file path from URL:', currentPath);
    
    // Check if we're on a note edit page
    const noteIdMatch = currentPath.match(/\/notes\/(.+)$/);
    if (noteIdMatch) {
        const noteId = decodeURIComponent(noteIdMatch[1]);
        console.log('📝 Inferred note ID from URL:', noteId);
        
        // If the noteId looks like a file path, use it directly
        if (noteId.includes('.md') || noteId.includes('/') || noteId.includes('\\')) {
            return noteId;
        } else {
            // Otherwise, assume it needs .md extension
            return `${noteId}.md`;
        }
    }
    
    console.log('⚠️ Could not determine note file path');
    return null;
};

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        window.misbah.api.init();
    });
} else {
    window.misbah.api.init();
}
