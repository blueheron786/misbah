// WYSIWYG Markdown Editor - True WYSIWYG editing with HTML to Markdown conversion
window.wysiwygMarkdownEditor = {
    editors: new Map(),
    
    initialize: function(containerId, dotNetRef) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('WYSIWYG container not found:', containerId);
            return;
        }

        const editorContent = container.querySelector('.editor-content');
        if (!editorContent) {
            console.error('Editor content element not found in:', containerId);
            return;
        }

        const editor = {
            container: container,
            content: editorContent,
            dotNetRef: dotNetRef,
            lastContent: editorContent.innerHTML
        };

        this.editors.set(containerId, editor);
        this.setupEventListeners(editor);
        this.enhanceEditing(editor);
    },

    setupEventListeners: function(editor) {
        const content = editor.content;
        
        // Handle input changes
        content.addEventListener('input', () => {
            this.debounce(() => {
                if (editor.dotNetRef) {
                    const html = content.innerHTML;
                    if (html !== editor.lastContent) {
                        editor.lastContent = html;
                        editor.dotNetRef.invokeMethodAsync('OnEditorContentChanged', html);
                    }
                }
            }, 300)();
        });

        // Handle paste events to clean up pasted content
        content.addEventListener('paste', (e) => {
            e.preventDefault();
            const text = e.clipboardData.getData('text/plain');
            this.insertText(text);
        });

        // Handle keyboard shortcuts
        content.addEventListener('keydown', (e) => {
            this.handleKeyboardShortcuts(e, editor);
        });

        // Handle placeholder
        content.addEventListener('focus', () => {
            if (content.textContent.trim() === 'Click here to start writing...') {
                content.innerHTML = '<p><br></p>';
                this.setCursorAtStart(content);
            }
        });
    },

    enhanceEditing: function(editor) {
        const content = editor.content;
        
        // Ensure we always have a paragraph for empty content
        if (!content.innerHTML.trim() || content.innerHTML === '<p class="placeholder">Click here to start writing...</p>') {
            content.innerHTML = '<p><br></p>';
        }
    },

    handleKeyboardShortcuts: function(e, editor) {
        // Bold: Ctrl+B
        if (e.ctrlKey && e.key === 'b') {
            e.preventDefault();
            document.execCommand('bold', false, null);
            return;
        }

        // Italic: Ctrl+I
        if (e.ctrlKey && e.key === 'i') {
            e.preventDefault();
            document.execCommand('italic', false, null);
            return;
        }

        // Code: Ctrl+`
        if (e.ctrlKey && e.key === '`') {
            e.preventDefault();
            this.toggleInlineCode();
            return;
        }

        // Headers: Ctrl+1-6
        if (e.ctrlKey && e.key >= '1' && e.key <= '6') {
            e.preventDefault();
            const level = parseInt(e.key);
            this.toggleHeading(level);
            return;
        }

        // Handle Enter key to maintain proper paragraph structure
        if (e.key === 'Enter' && !e.shiftKey) {
            const selection = window.getSelection();
            const range = selection.getRangeAt(0);
            const currentElement = range.commonAncestorContainer;
            
            // If we're in a heading, create a new paragraph
            if (currentElement.nodeType === Node.ELEMENT_NODE && 
                currentElement.tagName && 
                currentElement.tagName.match(/^H[1-6]$/)) {
                e.preventDefault();
                const p = document.createElement('p');
                p.innerHTML = '<br>';
                currentElement.parentNode.insertBefore(p, currentElement.nextSibling);
                range.setStart(p, 0);
                range.setEnd(p, 0);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        }
    },

    toggleInlineCode: function() {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        const selectedText = range.toString();
        
        if (selectedText) {
            const code = document.createElement('code');
            code.textContent = selectedText;
            code.className = 'inline-code';
            range.deleteContents();
            range.insertNode(code);
            
            // Move cursor after the code element
            range.setStartAfter(code);
            range.setEndAfter(code);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    },

    toggleHeading: function(level) {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        const element = range.commonAncestorContainer.nodeType === Node.TEXT_NODE 
            ? range.commonAncestorContainer.parentElement 
            : range.commonAncestorContainer;

        let targetElement = element;
        while (targetElement && !targetElement.tagName?.match(/^(P|H[1-6]|DIV)$/)) {
            targetElement = targetElement.parentElement;
        }

        if (targetElement) {
            const currentTag = targetElement.tagName;
            const newTag = `H${level}`;
            
            if (currentTag === newTag) {
                // Convert back to paragraph
                const p = document.createElement('p');
                p.innerHTML = targetElement.innerHTML;
                targetElement.parentNode.replaceChild(p, targetElement);
            } else {
                // Convert to heading
                const heading = document.createElement(newTag);
                heading.innerHTML = targetElement.innerHTML;
                targetElement.parentNode.replaceChild(heading, targetElement);
            }
        }
    },

    insertText: function(text) {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        range.deleteContents();
        range.insertNode(document.createTextNode(text));
        range.collapse(false);
        selection.removeAllRanges();
        selection.addRange(range);
    },

    setCursorAtStart: function(element) {
        const range = document.createRange();
        const selection = window.getSelection();
        range.setStart(element.firstChild || element, 0);
        range.setEnd(element.firstChild || element, 0);
        selection.removeAllRanges();
        selection.addRange(range);
    },

    getContent: function(containerId) {
        const editor = this.editors.get(containerId);
        if (!editor) return '';
        return editor.content.innerHTML;
    },

    htmlToMarkdown: function(html) {
        // Simple HTML to Markdown conversion
        let markdown = html;
        
        // Remove placeholder
        if (markdown.includes('Click here to start writing...')) {
            return '';
        }
        
        // Convert headings
        markdown = markdown.replace(/<h1[^>]*>(.*?)<\/h1>/gi, '# $1\n\n');
        markdown = markdown.replace(/<h2[^>]*>(.*?)<\/h2>/gi, '## $1\n\n');
        markdown = markdown.replace(/<h3[^>]*>(.*?)<\/h3>/gi, '### $1\n\n');
        markdown = markdown.replace(/<h4[^>]*>(.*?)<\/h4>/gi, '#### $1\n\n');
        markdown = markdown.replace(/<h5[^>]*>(.*?)<\/h5>/gi, '##### $1\n\n');
        markdown = markdown.replace(/<h6[^>]*>(.*?)<\/h6>/gi, '###### $1\n\n');
        
        // Convert paragraphs
        markdown = markdown.replace(/<p[^>]*>(.*?)<\/p>/gi, '$1\n\n');
        
        // Convert bold
        markdown = markdown.replace(/<(strong|b)[^>]*>(.*?)<\/(strong|b)>/gi, '**$2**');
        
        // Convert italic
        markdown = markdown.replace(/<(em|i)[^>]*>(.*?)<\/(em|i)>/gi, '*$2*');
        
        // Convert inline code
        markdown = markdown.replace(/<code[^>]*>(.*?)<\/code>/gi, '`$1`');
        
        // Convert code blocks
        markdown = markdown.replace(/<pre[^>]*><code[^>]*>(.*?)<\/code><\/pre>/gi, '```\n$1\n```\n\n');
        
        // Convert blockquotes
        markdown = markdown.replace(/<blockquote[^>]*>(.*?)<\/blockquote>/gi, (match, content) => {
            return content.split('\n').map(line => '> ' + line.trim()).join('\n') + '\n\n';
        });
        
        // Convert lists
        markdown = markdown.replace(/<ul[^>]*>(.*?)<\/ul>/gi, (match, content) => {
            return content.replace(/<li[^>]*>(.*?)<\/li>/gi, '- $1\n') + '\n';
        });
        
        markdown = markdown.replace(/<ol[^>]*>(.*?)<\/ol>/gi, (match, content) => {
            let counter = 1;
            return content.replace(/<li[^>]*>(.*?)<\/li>/gi, () => `${counter++}. $1\n`) + '\n';
        });
        
        // Convert links
        markdown = markdown.replace(/<a[^>]*href="([^"]*)"[^>]*>(.*?)<\/a>/gi, '[$2]($1)');
        
        // Convert horizontal rules
        markdown = markdown.replace(/<hr[^>]*>/gi, '---\n\n');
        
        // Clean up HTML tags
        markdown = markdown.replace(/<[^>]*>/g, '');
        
        // Clean up extra whitespace
        markdown = markdown.replace(/\n{3,}/g, '\n\n');
        markdown = markdown.replace(/^\n+|\n+$/g, '');
        
        // Decode HTML entities
        const textarea = document.createElement('textarea');
        textarea.innerHTML = markdown;
        markdown = textarea.value;
        
        return markdown;
    },

    debounce: function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    dispose: function(containerId) {
        this.editors.delete(containerId);
    }
};
