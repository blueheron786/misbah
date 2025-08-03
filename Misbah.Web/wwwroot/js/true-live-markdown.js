// True Live Markdown Editor - Provides Obsidian-style inline editing
window.trueLiveMarkdownEditor = {
    editors: new Map(),
    
    initialize: function(containerId, initialContent, dotNetRef) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Container not found:', containerId);
            return;
        }

        const editor = {
            container: container,
            dotNetRef: dotNetRef,
            content: initialContent,
            currentEditingElement: null,
            isEditing: false
        };

        this.editors.set(containerId, editor);
        this.renderContent(editor);
        this.setupEventListeners(editor);
    },

    renderContent: function(editor) {
        if (!editor.content || editor.content.trim() === '') {
            editor.container.innerHTML = '<div class="empty-placeholder" onclick="trueLiveMarkdownEditor.createNewParagraph(this)">Click here to start writing...</div>';
            return;
        }

        // Split content into blocks and render each one
        const blocks = this.parseMarkdownBlocks(editor.content);
        editor.container.innerHTML = '';

        blocks.forEach((block, index) => {
            const blockElement = this.createBlockElement(block, index, editor);
            editor.container.appendChild(blockElement);
        });

        // Add a final empty block for easy content addition
        const addBlock = document.createElement('div');
        addBlock.className = 'add-new-block';
        addBlock.innerHTML = '<i class="fas fa-plus"></i> Add content';
        addBlock.onclick = () => this.addNewBlock(editor);
        editor.container.appendChild(addBlock);
    },

    parseMarkdownBlocks: function(content) {
        const lines = content.split('\n');
        const blocks = [];
        let currentBlock = [];
        let currentType = 'paragraph';
        let inCodeBlock = false;

        for (let i = 0; i < lines.length; i++) {
            const line = lines[i];
            
            if (line.trim().startsWith('```')) {
                if (currentBlock.length > 0) {
                    blocks.push({
                        type: currentType,
                        content: currentBlock.join('\n'),
                        startLine: i - currentBlock.length,
                        endLine: i - 1
                    });
                    currentBlock = [];
                }
                inCodeBlock = !inCodeBlock;
                currentType = inCodeBlock ? 'code' : 'paragraph';
                currentBlock.push(line);
                continue;
            }

            if (inCodeBlock) {
                currentBlock.push(line);
                continue;
            }

            const blockType = this.getBlockType(line);
            
            if (blockType !== currentType && currentBlock.length > 0) {
                blocks.push({
                    type: currentType,
                    content: currentBlock.join('\n'),
                    startLine: i - currentBlock.length,
                    endLine: i - 1
                });
                currentBlock = [];
            }

            if (line.trim() === '' && currentBlock.length > 0) {
                blocks.push({
                    type: currentType,
                    content: currentBlock.join('\n'),
                    startLine: i - currentBlock.length,
                    endLine: i - 1
                });
                currentBlock = [];
                currentType = 'paragraph';
            } else if (line.trim() !== '') {
                currentType = blockType;
                currentBlock.push(line);
            }
        }

        if (currentBlock.length > 0) {
            blocks.push({
                type: currentType,
                content: currentBlock.join('\n'),
                startLine: lines.length - currentBlock.length,
                endLine: lines.length - 1
            });
        }

        return blocks;
    },

    getBlockType: function(line) {
        const trimmed = line.trim();
        if (trimmed.startsWith('# ')) return 'h1';
        if (trimmed.startsWith('## ')) return 'h2';
        if (trimmed.startsWith('### ')) return 'h3';
        if (trimmed.startsWith('#### ')) return 'h4';
        if (trimmed.startsWith('##### ')) return 'h5';
        if (trimmed.startsWith('###### ')) return 'h6';
        if (trimmed.startsWith('> ')) return 'quote';
        if (trimmed.startsWith('- [ ]') || trimmed.startsWith('- [x]')) return 'task';
        if (trimmed.startsWith('- ') || trimmed.startsWith('* ') || trimmed.startsWith('+ ')) return 'list';
        if (/^\d+\.\s/.test(trimmed)) return 'ordered-list';
        if (trimmed.startsWith('```')) return 'code';
        if (trimmed === '---' || trimmed === '***') return 'hr';
        return 'paragraph';
    },

    createBlockElement: function(block, index, editor) {
        const wrapper = document.createElement('div');
        wrapper.className = 'live-block';
        wrapper.dataset.blockIndex = index;
        wrapper.dataset.blockType = block.type;

        // Render the markdown content to HTML
        const renderedHtml = this.renderMarkdownToHtml(block.content, block.type);
        
        // Create the display element
        const displayElement = document.createElement('div');
        displayElement.className = 'block-display';
        displayElement.innerHTML = renderedHtml;
        
        // Create the edit element (hidden by default)
        const editElement = document.createElement('div');
        editElement.className = 'block-edit';
        editElement.style.display = 'none';
        
        const textarea = document.createElement('textarea');
        textarea.className = 'block-textarea';
        textarea.value = block.content;
        textarea.style.cssText = this.getTextareaStyle(block.type);
        
        const controls = document.createElement('div');
        controls.className = 'edit-controls';
        controls.innerHTML = `
            <button class="btn-save" onclick="trueLiveMarkdownEditor.saveBlock('${editor.container.id}', ${index})">
                <i class="fas fa-check"></i>
            </button>
            <button class="btn-cancel" onclick="trueLiveMarkdownEditor.cancelEdit('${editor.container.id}', ${index})">
                <i class="fas fa-times"></i>
            </button>
        `;
        
        editElement.appendChild(textarea);
        editElement.appendChild(controls);
        
        wrapper.appendChild(displayElement);
        wrapper.appendChild(editElement);
        
        // Add click handler for editing
        displayElement.onclick = () => this.startEditing(editor, index);
        
        // Add keyboard handlers for the textarea
        textarea.onkeydown = (e) => this.handleTextareaKeyDown(e, editor, index);
        textarea.oninput = () => this.autoResizeTextarea(textarea);
        
        return wrapper;
    },

    renderMarkdownToHtml: function(content, type) {
        // Simple markdown to HTML conversion
        let html = content;
        
        // Handle different block types
        switch (type) {
            case 'h1':
                html = `<h1>${this.escapeHtml(content.replace(/^#\s*/, ''))}</h1>`;
                break;
            case 'h2':
                html = `<h2>${this.escapeHtml(content.replace(/^##\s*/, ''))}</h2>`;
                break;
            case 'h3':
                html = `<h3>${this.escapeHtml(content.replace(/^###\s*/, ''))}</h3>`;
                break;
            case 'h4':
                html = `<h4>${this.escapeHtml(content.replace(/^####\s*/, ''))}</h4>`;
                break;
            case 'h5':
                html = `<h5>${this.escapeHtml(content.replace(/^#####\s*/, ''))}</h5>`;
                break;
            case 'h6':
                html = `<h6>${this.escapeHtml(content.replace(/^######\s*/, ''))}</h6>`;
                break;
            case 'quote':
                html = `<blockquote>${this.escapeHtml(content.replace(/^>\s*/, ''))}</blockquote>`;
                break;
            case 'code':
                html = `<pre><code>${this.escapeHtml(content.replace(/^```[\w]*\n?/, '').replace(/\n?```$/, ''))}</code></pre>`;
                break;
            case 'hr':
                html = '<hr>';
                break;
            case 'task':
                html = this.renderTaskList(content);
                break;
            case 'list':
                html = this.renderList(content);
                break;
            case 'ordered-list':
                html = this.renderOrderedList(content);
                break;
            default:
                // Handle inline markdown in paragraphs
                html = `<p>${this.renderInlineMarkdown(content)}</p>`;
        }
        
        return html;
    },

    renderInlineMarkdown: function(text) {
        let html = this.escapeHtml(text);
        
        // Bold
        html = html.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        html = html.replace(/__(.*?)__/g, '<strong>$1</strong>');
        
        // Italic
        html = html.replace(/\*(.*?)\*/g, '<em>$1</em>');
        html = html.replace(/_(.*?)_/g, '<em>$1</em>');
        
        // Code
        html = html.replace(/`(.*?)`/g, '<code>$1</code>');
        
        // Links
        html = html.replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2">$1</a>');
        
        return html;
    },

    renderTaskList: function(content) {
        const lines = content.split('\n');
        const items = lines.map(line => {
            const match = line.match(/^- \[([ x])\] (.*)$/);
            if (match) {
                const checked = match[1] === 'x' ? 'checked' : '';
                const text = this.escapeHtml(match[2]);
                return `<li><input type="checkbox" ${checked} disabled> ${text}</li>`;
            }
            return '';
        }).filter(item => item);
        
        return `<ul class="task-list">${items.join('')}</ul>`;
    },

    renderList: function(content) {
        const lines = content.split('\n');
        const items = lines.map(line => {
            const match = line.match(/^[-*+]\s(.*)$/);
            if (match) {
                return `<li>${this.escapeHtml(match[1])}</li>`;
            }
            return '';
        }).filter(item => item);
        
        return `<ul>${items.join('')}</ul>`;
    },

    renderOrderedList: function(content) {
        const lines = content.split('\n');
        const items = lines.map(line => {
            const match = line.match(/^\d+\.\s(.*)$/);
            if (match) {
                return `<li>${this.escapeHtml(match[1])}</li>`;
            }
            return '';
        }).filter(item => item);
        
        return `<ol>${items.join('')}</ol>`;
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    getTextareaStyle: function(type) {
        const baseStyle = `
            resize: none; 
            border: none; 
            outline: none; 
            background: transparent; 
            font-family: inherit; 
            width: 100%; 
            min-height: 40px;
            padding: 0;
            margin: 0;
            color: inherit;
            line-height: inherit;
        `;
        
        switch (type) {
            case 'h1': return baseStyle + 'font-size: 2rem; font-weight: bold;';
            case 'h2': return baseStyle + 'font-size: 1.5rem; font-weight: bold;';
            case 'h3': return baseStyle + 'font-size: 1.25rem; font-weight: bold;';
            case 'h4': return baseStyle + 'font-size: 1.1rem; font-weight: bold;';
            case 'h5': return baseStyle + 'font-size: 1rem; font-weight: bold;';
            case 'h6': return baseStyle + 'font-size: 0.9rem; font-weight: bold;';
            case 'code': return baseStyle + 'font-family: "Consolas", "Monaco", monospace; background: #f8f9fa; padding: 12px; border-radius: 4px;';
            default: return baseStyle;
        }
    },

    startEditing: function(editor, blockIndex) {
        if (editor.isEditing) return;
        
        editor.isEditing = true;
        editor.currentEditingElement = blockIndex;
        
        const wrapper = editor.container.querySelector(`[data-block-index="${blockIndex}"]`);
        const display = wrapper.querySelector('.block-display');
        const edit = wrapper.querySelector('.block-edit');
        const textarea = edit.querySelector('.block-textarea');
        
        display.style.display = 'none';
        edit.style.display = 'block';
        
        // Focus and position cursor
        setTimeout(() => {
            textarea.focus();
            textarea.setSelectionRange(textarea.value.length, textarea.value.length);
            this.autoResizeTextarea(textarea);
        }, 10);
    },

    saveBlock: function(containerId, blockIndex) {
        const editor = this.editors.get(containerId);
        if (!editor) return;
        
        const wrapper = editor.container.querySelector(`[data-block-index="${blockIndex}"]`);
        const textarea = wrapper.querySelector('.block-textarea');
        const newContent = textarea.value;
        
        // Update the block content and re-render
        this.updateBlockAndRefresh(editor, blockIndex, newContent);
    },

    cancelEdit: function(containerId, blockIndex) {
        const editor = this.editors.get(containerId);
        if (!editor) return;
        
        const wrapper = editor.container.querySelector(`[data-block-index="${blockIndex}"]`);
        const display = wrapper.querySelector('.block-display');
        const edit = wrapper.querySelector('.block-edit');
        
        display.style.display = 'block';
        edit.style.display = 'none';
        
        editor.isEditing = false;
        editor.currentEditingElement = null;
    },

    updateBlockAndRefresh: function(editor, blockIndex, newContent) {
        // Parse current content into blocks
        const blocks = this.parseMarkdownBlocks(editor.content);
        
        if (blockIndex < blocks.length) {
            blocks[blockIndex].content = newContent;
        }
        
        // Rebuild full content
        const newFullContent = blocks.map(block => block.content).join('\n\n');
        editor.content = newFullContent;
        
        // Notify Blazor component
        if (editor.dotNetRef) {
            editor.dotNetRef.invokeMethodAsync('OnContentChanged', newFullContent);
        }
        
        // Re-render the content
        this.renderContent(editor);
        
        editor.isEditing = false;
        editor.currentEditingElement = null;
    },

    handleTextareaKeyDown: function(e, editor, blockIndex) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            e.preventDefault();
            this.saveBlock(editor.container.id, blockIndex);
        } else if (e.key === 'Escape') {
            e.preventDefault();
            this.cancelEdit(editor.container.id, blockIndex);
        }
    },

    autoResizeTextarea: function(textarea) {
        textarea.style.height = 'auto';
        textarea.style.height = Math.max(textarea.scrollHeight, 40) + 'px';
    },

    addNewBlock: function(editor) {
        const newContent = editor.content + (editor.content ? '\n\n' : '') + 'New paragraph';
        editor.content = newContent;
        
        if (editor.dotNetRef) {
            editor.dotNetRef.invokeMethodAsync('OnContentChanged', newContent);
        }
        
        this.renderContent(editor);
        
        // Start editing the new block
        const blocks = this.parseMarkdownBlocks(newContent);
        setTimeout(() => {
            this.startEditing(editor, blocks.length - 1);
        }, 100);
    },

    createNewParagraph: function(element) {
        const container = element.parentElement;
        const containerId = container.id;
        const editor = this.editors.get(containerId);
        
        if (editor) {
            this.addNewBlock(editor);
        }
    },

    updateContent: function(containerId, newContent) {
        const editor = this.editors.get(containerId);
        if (editor && !editor.isEditing) {
            editor.content = newContent;
            this.renderContent(editor);
        }
    },

    dispose: function(containerId) {
        this.editors.delete(containerId);
    },

    setupEventListeners: function(editor) {
        // Add any global event listeners here if needed
    }
};
