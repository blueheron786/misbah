// Markdown Editor Helper Functions
window.markdownEditorHelpers = {
    // Initialize message listener for editor content changes
    initializeEditor: function (dotNetReference) {
        window.addEventListener('message', function (event) {
            if (event.data && event.data.type === 'contentChanged') {
                dotNetReference.invokeMethodAsync('OnContentChanged', event.data.content);
            }
        });
    },

    // Get editor content
    getEditorContent: function () {
        try {
            const iframe = document.querySelector('iframe[src*="codemirror-editor.html"]');
            if (iframe && iframe.contentWindow && iframe.contentWindow.getContent) {
                return iframe.contentWindow.getContent();
            }
        } catch (error) {
            console.warn('Could not get editor content:', error);
        }
        return '';
    },

    // Set editor content
    setEditorContent: function (content) {
        try {
            const iframe = document.querySelector('iframe[src*="codemirror-editor.html"]');
            if (iframe && iframe.contentWindow && iframe.contentWindow.setContent) {
                iframe.contentWindow.setContent(content || '');
                return true;
            }
        } catch (error) {
            console.warn('Could not set editor content:', error);
        }
        return false;
    },

    // Set editor theme
    setEditorTheme: function (theme) {
        try {
            const iframe = document.querySelector('iframe[src*="codemirror-editor.html"]');
            if (iframe && iframe.contentWindow && iframe.contentWindow.setTheme) {
                iframe.contentWindow.setTheme(theme);
                return true;
            }
        } catch (error) {
            console.warn('Could not set editor theme:', error);
        }
        return false;
    },

    // Focus editor
    focusEditor: function () {
        try {
            const iframe = document.querySelector('iframe[src*="codemirror-editor.html"]');
            if (iframe && iframe.contentWindow && iframe.contentWindow.focus) {
                iframe.contentWindow.focus();
                return true;
            }
        } catch (error) {
            console.warn('Could not focus editor:', error);
        }
        return false;
    },

    // Initialize editor with content and theme
    createEditor: function (content, theme) {
        try {
            const iframe = document.querySelector('iframe[src*="codemirror-editor.html"]');
            if (iframe && iframe.contentWindow && iframe.contentWindow.createEditor) {
                iframe.contentWindow.createEditor(content || '', theme || 'light');
                return true;
            }
        } catch (error) {
            console.warn('Could not create editor:', error);
        }
        return false;
    },

    // Wait for iframe to load and then execute callback
    waitForIframe: function (callback, maxAttempts = 50) {
        let attempts = 0;
        const checkIframe = () => {
            attempts++;
            const iframe = document.querySelector('iframe[src*="codemirror-editor.html"]');
            
            if (iframe && iframe.contentWindow && iframe.contentWindow.createEditor) {
                callback();
            } else if (attempts < maxAttempts) {
                setTimeout(checkIframe, 100);
            } else {
                console.warn('Iframe failed to load properly after maximum attempts');
            }
        };
        
        checkIframe();
    }
};

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.markdownEditorHelpers;
}