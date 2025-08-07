// Resizable splitter functionality
window.initializeSplitter = function() {
    const splitter = document.querySelector('.splitter');
    const leftPanel = document.querySelector('.sidebar-panel');
    const container = document.querySelector('.layout-container');
    
    if (!splitter || !leftPanel || !container) {
        return;
    }

    let isResizing = false;
    let startX = 0;
    let startWidth = 0;

    // Load saved width from localStorage
    const savedWidth = localStorage.getItem('sidebarWidth');
    if (savedWidth) {
        const width = parseInt(savedWidth, 10);
        const containerWidth = container.offsetWidth;
        const minWidth = 180;
        const maxWidth = containerWidth * 0.5;
        
        if (width >= minWidth && width <= maxWidth) {
            leftPanel.style.width = width + 'px';
        }
    }

    splitter.addEventListener('mousedown', function(e) {
        isResizing = true;
        startX = e.clientX;
        startWidth = parseInt(document.defaultView.getComputedStyle(leftPanel).width, 10);
        
        splitter.classList.add('dragging');
        document.body.style.cursor = 'ew-resize';
        document.body.style.userSelect = 'none';
        
        e.preventDefault();
    });

    document.addEventListener('mousemove', function(e) {
        if (!isResizing) return;

        const containerWidth = container.offsetWidth;
        const newWidth = startWidth + (e.clientX - startX);
        const minWidth = 180;
        const maxWidth = containerWidth * 0.5;

        if (newWidth >= minWidth && newWidth <= maxWidth) {
            leftPanel.style.width = newWidth + 'px';
        }
    });

    document.addEventListener('mouseup', function() {
        if (isResizing) {
            isResizing = false;
            splitter.classList.remove('dragging');
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
            
            // Save the current width to localStorage
            const currentWidth = parseInt(document.defaultView.getComputedStyle(leftPanel).width, 10);
            localStorage.setItem('sidebarWidth', currentWidth.toString());
        }
    });

    // Handle window resize to enforce constraints
    window.addEventListener('resize', function() {
        const containerWidth = container.offsetWidth;
        const currentWidth = parseInt(document.defaultView.getComputedStyle(leftPanel).width, 10);
        const maxWidth = containerWidth * 0.5;
        
        if (currentWidth > maxWidth) {
            leftPanel.style.width = maxWidth + 'px';
            localStorage.setItem('sidebarWidth', maxWidth.toString());
        }
    });
};

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.initializeSplitter);
} else {
    window.initializeSplitter();
}
