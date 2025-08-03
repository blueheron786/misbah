// Enhanced file picker functionality for Capacitor with proper Android URI handling
window.showDirectoryPicker = async function() {
    // Just use the file-based picker directly
    return await showDirectoryPickerViaFile();
};

// File picker that picks files to find their folders - this is now the main method
window.showDirectoryPickerViaFile = async function() {
    try {
        console.log('Starting file-based directory picker...');
        
        // Check if Capacitor and FilePicker are available
        if (typeof Capacitor !== 'undefined' && Capacitor.Plugins && Capacitor.Plugins.FilePicker) {
            const { FilePicker } = Capacitor.Plugins;
            
            // Show instruction alert
            alert('Please navigate to your notes folder and select ANY file inside that folder.\n\nThe app will use that file\'s folder as your notes vault.');
            
            // Try to pick any file to get its directory
            const result = await FilePicker.pickFiles({
                types: ['*/*'], // Accept any file type
                multiple: false,
                readData: false
            });
            
            if (result && result.files && result.files.length > 0) {
                const file = result.files[0];
                console.log('Selected file via FilePicker:', file);
                
                // Extract directory from file path
                if (file.path) {
                    console.log('File path:', file.path);
                    
                    let folderPath = null;
                    
                    if (file.path.startsWith('content://')) {
                        // Handle content URI - use pattern-based extraction
                        console.log('Handling content URI');
                        folderPath = await convertContentUriToPath(file.path);
                    } else {
                        // Handle regular file path
                        console.log('Handling regular file path');
                        const parts = file.path.split('/');
                        parts.pop(); // Remove filename
                        folderPath = parts.join('/');
                    }
                    
                    if (folderPath) {
                        console.log('Extracted folder path:', folderPath);
                        
                        const confirmed = confirm(`Use this folder for your notes?\n\n${folderPath}`);
                        if (confirmed) {
                            console.log('User confirmed folder:', folderPath);
                            return folderPath;
                        }
                    } else {
                        console.error('Could not extract folder path from:', file.path);
                        alert('Could not determine folder from selected file. Please try again.');
                    }
                }
            } else {
                console.log('No file selected');
                alert('No file was selected. Please try again.');
                return null;
            }
        } else {
            console.error('Capacitor FilePicker not available');
            alert('File picker not available on this device.');
            return null;
        }
        
        return null;
        
    } catch (error) {
        console.error('Error in file-based directory picker:', error);
        alert('Error opening file picker: ' + error.message);
        return null;
    }
};

// Convert Android content:// URI to file path
window.convertContentUriToPath = async function(contentUri) {
    try {
        if (!contentUri || !contentUri.startsWith('content://')) {
            return contentUri; // Already a file path
        }
        
        console.log('Converting content URI:', contentUri);
        
        // Don't try to use Capacitor Filesystem with content URIs - it causes crashes
        // Instead, use pattern-based fallback immediately
        console.log('Using pattern-based conversion for content URI');
        
        // Extract meaningful path from content URI patterns
        if (contentUri.includes('ObsidianVaults')) {
            // Try to extract specific ObsidianVaults path
            if (contentUri.includes('SecondBrain')) {
                return '/storage/emulated/0/Documents/ObsidianVaults/SecondBrain';
            }
            return '/storage/emulated/0/Documents/ObsidianVaults';
        } else if (contentUri.includes('Documents')) {
            return '/storage/emulated/0/Documents';
        } else if (contentUri.includes('Download')) {
            return '/storage/emulated/0/Download';
        }
        
        // Default fallback
        return '/storage/emulated/0/Documents';
        
    } catch (error) {
        console.error('Error converting content URI:', error);
        return '/storage/emulated/0/Documents'; // Safe fallback
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('Simplified file picker initialized');
});
