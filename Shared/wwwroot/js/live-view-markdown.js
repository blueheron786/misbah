// Live View Markdown Editor JavaScript helpers
window.liveViewMarkdownHelpers = {
    // Focus management for inline editing
    focusInlineEditor: function(selector) {
        setTimeout(() => {
            const element = document.querySelector(selector);
            if (element) {
                element.focus();
                // Position cursor at end
                element.setSelectionRange(element.value.length, element.value.length);
            }
        }, 50);
    },

    // Auto-resize textarea based on content
    autoResizeTextarea: function(element) {
        element.style.height = 'auto';
        element.style.height = Math.max(element.scrollHeight, 40) + 'px';
    },

    // Set up auto-resize listener
    setupAutoResize: function(selector) {
        const element = document.querySelector(selector);
        if (element) {
            element.addEventListener('input', function() {
                window.liveViewMarkdownHelpers.autoResizeTextarea(this);
            });
            // Initial resize
            window.liveViewMarkdownHelpers.autoResizeTextarea(element);
        }
    },

    // Handle special key combinations for better editing experience
    setupKeyboardShortcuts: function(selector, dotNetRef) {
        const element = document.querySelector(selector);
        if (element) {
            element.addEventListener('keydown', function(e) {
                // Ctrl+Enter or Cmd+Enter to finish editing
                if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('FinishEditing');
                }
                // Escape to cancel
                else if (e.key === 'Escape') {
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('CancelEditing');
                }
                // Tab for indentation in code blocks and lists
                else if (e.key === 'Tab') {
                    const selectionStart = this.selectionStart;
                    const selectionEnd = this.selectionEnd;
                    const value = this.value;
                    
                    if (e.shiftKey) {
                        // Shift+Tab: Remove indentation
                        const lineStart = value.lastIndexOf('\n', selectionStart - 1) + 1;
                        const lineText = value.substring(lineStart, selectionStart);
                        if (lineText.startsWith('    ')) {
                            e.preventDefault();
                            this.value = value.substring(0, lineStart) + lineText.substring(4) + value.substring(selectionStart);
                            this.setSelectionRange(selectionStart - 4, selectionEnd - 4);
                        } else if (lineText.startsWith('\t')) {
                            e.preventDefault();
                            this.value = value.substring(0, lineStart) + lineText.substring(1) + value.substring(selectionStart);
                            this.setSelectionRange(selectionStart - 1, selectionEnd - 1);
                        }
                    } else {
                        // Tab: Add indentation
                        e.preventDefault();
                        this.value = value.substring(0, selectionStart) + '    ' + value.substring(selectionEnd);
                        this.setSelectionRange(selectionStart + 4, selectionStart + 4);
                    }
                }
            });
        }
    },

    // Smart enter behavior for lists and tasks
    handleSmartEnter: function(element) {
        element.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' && !e.ctrlKey && !e.metaKey) {
                const selectionStart = this.selectionStart;
                const value = this.value;
                const lineStart = value.lastIndexOf('\n', selectionStart - 1) + 1;
                const currentLine = value.substring(lineStart, selectionStart);
                
                // Check for list patterns
                const listMatch = currentLine.match(/^(\s*)([-*+])\s/);
                const taskMatch = currentLine.match(/^(\s*)-\s\[([ x])\]\s/);
                const numberedMatch = currentLine.match(/^(\s*)(\d+)\.\s/);
                
                if (listMatch || taskMatch || numberedMatch) {
                    e.preventDefault();
                    let newLinePrefix = '';
                    
                    if (taskMatch) {
                        newLinePrefix = `${taskMatch[1]}- [ ] `;
                    } else if (listMatch) {
                        newLinePrefix = `${listMatch[1]}${listMatch[2]} `;
                    } else if (numberedMatch) {
                        const nextNum = parseInt(numberedMatch[2]) + 1;
                        newLinePrefix = `${numberedMatch[1]}${nextNum}. `;
                    }
                    
                    this.value = value.substring(0, selectionStart) + '\n' + newLinePrefix + value.substring(selectionStart);
                    this.setSelectionRange(selectionStart + 1 + newLinePrefix.length, selectionStart + 1 + newLinePrefix.length);
                    
                    // Trigger auto-resize
                    window.liveViewMarkdownHelpers.autoResizeTextarea(this);
                }
            }
        });
    },

    // Initialize all helpers for an editor
    initializeEditor: function(selector, dotNetRef) {
        const element = document.querySelector(selector);
        if (element) {
            this.setupAutoResize(selector);
            this.setupKeyboardShortcuts(selector, dotNetRef);
            this.handleSmartEnter(element);
            this.focusInlineEditor(selector);
        }
    },

    // Clean up any event listeners (for disposal)
    cleanup: function(selector) {
        const element = document.querySelector(selector);
        if (element) {
            // Clone and replace to remove all event listeners
            const newElement = element.cloneNode(true);
            element.parentNode.replaceChild(newElement, element);
        }
    }
};

// Export for use in modules if needed
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.liveViewMarkdownHelpers;
}
