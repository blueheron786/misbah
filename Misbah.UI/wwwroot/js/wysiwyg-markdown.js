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
            this.toggleBold();
            return;
        }

        // Italic: Ctrl+I
        if (e.ctrlKey && e.key === 'i') {
            e.preventDefault();
            this.toggleItalic();
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

        // Task list: Ctrl+Shift+X
        if (e.ctrlKey && e.shiftKey && e.key === 'X') {
            e.preventDefault();
            this.toggleTaskList();
            return;
        }

        // Unordered list: Ctrl+L
        if (e.ctrlKey && e.key === 'l') {
            e.preventDefault();
            this.toggleUnorderedList();
            return;
        }

        // Handle Enter key in task lists
        if (e.key === 'Enter') {
            const selection = window.getSelection();
            if (selection.rangeCount > 0) {
                const range = selection.getRangeAt(0);
                const container = range.startContainer;
                const listItem = container.nodeType === Node.TEXT_NODE ? 
                    container.parentElement.closest('li') : 
                    container.closest('li');
                
                if (listItem && listItem.querySelector('input[type="checkbox"]')) {
                    e.preventDefault();
                    this.createNewTaskItem(listItem);
                    return;
                }
                
                // Handle Enter in regular lists
                if (listItem && listItem.parentElement && listItem.parentElement.tagName === 'UL') {
                    e.preventDefault();
                    this.createNewListItem(listItem);
                    return;
                }
            }
        }

        // Handle Enter key to maintain proper paragraph structure
        if (e.key === 'Enter' && !e.shiftKey) {
            const selection = window.getSelection();
            if (selection.rangeCount === 0) return;
            
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
                return;
            }
            
            // Default behavior - create proper line breaks
            if (!range.commonAncestorContainer.closest('li')) {
                e.preventDefault();
                const br = document.createElement('br');
                range.deleteContents();
                range.insertNode(br);
                range.setStartAfter(br);
                range.collapse(true);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        }
    },

    toggleBold: function() {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        const selectedText = range.toString();
        
        if (selectedText) {
            // Check if the selection is already bold
            const parentElement = range.commonAncestorContainer.parentElement;
            if (parentElement && (parentElement.tagName === 'STRONG' || parentElement.tagName === 'B')) {
                // Remove bold
                const textNode = document.createTextNode(selectedText);
                parentElement.parentNode.replaceChild(textNode, parentElement);
            } else {
                // Add bold
                const strong = document.createElement('strong');
                strong.textContent = selectedText;
                range.deleteContents();
                range.insertNode(strong);
                
                // Move cursor after the strong element
                range.setStartAfter(strong);
                range.setEndAfter(strong);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        } else {
            // No selection, toggle bold at cursor position
            document.execCommand('bold', false, null);
        }
    },

    toggleItalic: function() {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        const selectedText = range.toString();
        
        if (selectedText) {
            // Check if the selection is already italic
            const parentElement = range.commonAncestorContainer.parentElement;
            if (parentElement && (parentElement.tagName === 'EM' || parentElement.tagName === 'I')) {
                // Remove italic
                const textNode = document.createTextNode(selectedText);
                parentElement.parentNode.replaceChild(textNode, parentElement);
            } else {
                // Add italic
                const em = document.createElement('em');
                em.textContent = selectedText;
                range.deleteContents();
                range.insertNode(em);
                
                // Move cursor after the em element
                range.setStartAfter(em);
                range.setEndAfter(em);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        } else {
            // No selection, toggle italic at cursor position
            document.execCommand('italic', false, null);
        }
    },

    toggleTaskList: function() {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        const element = range.commonAncestorContainer.nodeType === Node.TEXT_NODE 
            ? range.commonAncestorContainer.parentElement 
            : range.commonAncestorContainer;

        let targetElement = element;
        while (targetElement && !targetElement.tagName?.match(/^(P|LI|DIV)$/)) {
            targetElement = targetElement.parentElement;
        }

        if (targetElement) {
            if (targetElement.tagName === 'LI') {
                // Check if it's already a task list item
                const checkbox = targetElement.querySelector('input[type="checkbox"]');
                if (checkbox) {
                    // Remove checkbox, convert to regular list item
                    checkbox.remove();
                    targetElement.innerHTML = targetElement.innerHTML.trim();
                } else {
                    // Add checkbox to make it a task list item
                    const checkbox = document.createElement('input');
                    checkbox.type = 'checkbox';
                    checkbox.className = 'md-task';
                    targetElement.insertBefore(checkbox, targetElement.firstChild);
                    targetElement.insertBefore(document.createTextNode(' '), checkbox.nextSibling);
                }
            } else {
                // Convert paragraph to task list
                const ul = document.createElement('ul');
                const li = document.createElement('li');
                const checkbox = document.createElement('input');
                checkbox.type = 'checkbox';
                checkbox.className = 'md-task';
                
                li.appendChild(checkbox);
                li.appendChild(document.createTextNode(' ' + targetElement.textContent));
                ul.appendChild(li);
                
                targetElement.parentNode.replaceChild(ul, targetElement);
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
        
        // Handle line breaks first - convert <br> and <div> to newlines
        markdown = markdown.replace(/<br[^>]*>/gi, '\n');
        markdown = markdown.replace(/<div[^>]*>/gi, '\n');
        markdown = markdown.replace(/<\/div>/gi, '\n');
        
        // Handle task lists FIRST (before regular lists)
        markdown = markdown.replace(/<li[^>]*>\s*<input[^>]*type="checkbox"[^>]*checked[^>]*>\s*(.*?)<\/li>/gi, '- [x] $1');
        markdown = markdown.replace(/<li[^>]*>\s*<input[^>]*type="checkbox"[^>]*>\s*(.*?)<\/li>/gi, '- [ ] $1');
        
        // Convert headings
        markdown = markdown.replace(/<h1[^>]*>(.*?)<\/h1>/gi, '# $1\n\n');
        markdown = markdown.replace(/<h2[^>]*>(.*?)<\/h2>/gi, '## $1\n\n');
        markdown = markdown.replace(/<h3[^>]*>(.*?)<\/h3>/gi, '### $1\n\n');
        markdown = markdown.replace(/<h4[^>]*>(.*?)<\/h4>/gi, '#### $1\n\n');
        markdown = markdown.replace(/<h5[^>]*>(.*?)<\/h5>/gi, '##### $1\n\n');
        markdown = markdown.replace(/<h6[^>]*>(.*?)<\/h6>/gi, '###### $1\n\n');
        
        // Convert paragraphs
        markdown = markdown.replace(/<p[^>]*>(.*?)<\/p>/gi, '$1\n\n');
        
        // Convert bold (handle both <strong> and <b>)
        markdown = markdown.replace(/<(strong|b)[^>]*>(.*?)<\/(strong|b)>/gi, '**$2**');
        
        // Convert italic (handle both <em> and <i>)
        markdown = markdown.replace(/<(em|i)[^>]*>(.*?)<\/(em|i)>/gi, '*$2*');
        
        // Convert inline code
        markdown = markdown.replace(/<code[^>]*>(.*?)<\/code>/gi, '`$1`');
        
        // Convert code blocks
        markdown = markdown.replace(/<pre[^>]*><code[^>]*>(.*?)<\/code><\/pre>/gi, '```\n$1\n```\n\n');
        
        // Convert blockquotes
        markdown = markdown.replace(/<blockquote[^>]*>(.*?)<\/blockquote>/gi, (match, content) => {
            // Clean up the content and split into lines
            const cleanContent = content.replace(/<[^>]*>/g, '').trim();
            return cleanContent.split('\n').map(line => '> ' + line.trim()).filter(line => line.trim() !== '>').join('\n') + '\n\n';
        });
        
        // Convert unordered lists (but not task lists which we handled above)
        markdown = markdown.replace(/<ul[^>]*>(.*?)<\/ul>/gi, (match, content) => {
            // Only convert if it doesn't contain checkboxes (task lists)
            if (content.includes('type="checkbox"')) {
                // For task lists, just clean up the content
                return content.replace(/<li[^>]*>(.*?)<\/li>/gi, '$1\n') + '\n';
            } else {
                // For regular lists, add bullet points
                const listItems = content.replace(/<li[^>]*>(.*?)<\/li>/gi, '- $1\n');
                return '\n' + listItems + '\n';
            }
        });
        
        // Convert ordered lists
        markdown = markdown.replace(/<ol[^>]*>(.*?)<\/ol>/gi, (match, content) => {
            let counter = 1;
            const listItems = content.replace(/<li[^>]*>(.*?)<\/li>/gi, () => `${counter++}. $1\n`);
            return '\n' + listItems + '\n';
        });
        
        // Convert links
        markdown = markdown.replace(/<a[^>]*href="([^"]*)"[^>]*>(.*?)<\/a>/gi, '[$2]($1)');
        
        // Convert horizontal rules
        markdown = markdown.replace(/<hr[^>]*>/gi, '---\n\n');
        
        // Clean up remaining HTML tags
        markdown = markdown.replace(/<[^>]*>/g, '');
        
        // Fix multiple consecutive newlines but preserve intentional spacing
        markdown = markdown.replace(/\n{4,}/g, '\n\n\n');
        
        // Clean up leading/trailing whitespace on lines
        markdown = markdown.split('\n').map(line => line.trim()).join('\n');
        
        // Remove leading and trailing newlines from the entire content
        markdown = markdown.replace(/^\n+|\n+$/g, '');
        
        // Decode HTML entities
        const textarea = document.createElement('textarea');
        textarea.innerHTML = markdown;
        markdown = textarea.value;
        
        return markdown;
    },

    createNewTaskItem: function(currentItem) {
        const newItem = document.createElement('li');
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.className = 'md-task';
        
        newItem.appendChild(checkbox);
        newItem.appendChild(document.createTextNode(' '));
        
        currentItem.parentNode.insertBefore(newItem, currentItem.nextSibling);
        
        // Set cursor after the checkbox
        const range = document.createRange();
        const selection = window.getSelection();
        range.setStartAfter(checkbox);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    },

    createNewListItem: function(currentItem) {
        const newItem = document.createElement('li');
        newItem.textContent = '';
        
        currentItem.parentNode.insertBefore(newItem, currentItem.nextSibling);
        
        // Set cursor in the new list item
        const range = document.createRange();
        const selection = window.getSelection();
        range.setStart(newItem, 0);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    },

    toggleUnorderedList: function() {
        const selection = window.getSelection();
        if (selection.rangeCount === 0) return;

        const range = selection.getRangeAt(0);
        const container = range.commonAncestorContainer;
        const listItem = container.nodeType === Node.TEXT_NODE ? 
            container.parentElement.closest('li') : 
            container.closest('li');

        if (listItem && listItem.parentElement.tagName === 'UL') {
            // Convert list item to paragraph
            const p = document.createElement('p');
            p.textContent = listItem.textContent;
            listItem.parentElement.insertBefore(p, listItem);
            listItem.remove();

            // If the list is now empty, remove it
            if (listItem.parentElement.children.length === 0) {
                listItem.parentElement.remove();
            }
        } else {
            // Create new list
            const ul = document.createElement('ul');
            const li = document.createElement('li');
            
            if (range.collapsed) {
                li.textContent = 'List item';
            } else {
                li.textContent = range.toString();
                range.deleteContents();
            }
            
            ul.appendChild(li);
            range.insertNode(ul);
            
            // Set cursor in the list item
            const newRange = document.createRange();
            newRange.setStart(li.firstChild, li.textContent.length);
            newRange.collapse(true);
            selection.removeAllRanges();
            selection.addRange(newRange);
        }
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
