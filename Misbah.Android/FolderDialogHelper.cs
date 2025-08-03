using Microsoft.JSInterop;

namespace Misbah.Web
{
    public static class FolderDialogHelper
    {
        private static IJSRuntime? _jsRuntime;

        public static void Initialize(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public static async Task<string?> ShowFolderDialog()
        {
            try
            {
                if (_jsRuntime == null)
                {
                    // Fallback if JSRuntime not initialized
                    Console.WriteLine("JSRuntime not initialized, using default path");
                    return "/storage/emulated/0/Documents/Notes";
                }

                // Try to show the directory picker with native Android support
                var result = await _jsRuntime.InvokeAsync<string?>("showDirectoryPicker");
                
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine($"User selected path: {result}");
                    
                    // Convert content:// URIs to file paths if needed
                    var finalPath = result;
                    if (result.StartsWith("content://"))
                    {
                        Console.WriteLine($"Converting content URI: {result}");
                        try
                        {
                            finalPath = await _jsRuntime.InvokeAsync<string>("convertContentUriToPath", result);
                            Console.WriteLine($"Converted to path: {finalPath}");
                        }
                        catch (Exception convEx)
                        {
                            Console.WriteLine($"URI conversion failed: {convEx.Message}");
                            // Use a reasonable default based on the content URI
                            if (result.Contains("Documents"))
                            {
                                finalPath = "/storage/emulated/0/Documents";
                            }
                            else if (result.Contains("Download"))
                            {
                                finalPath = "/storage/emulated/0/Download";
                            }
                            else
                            {
                                finalPath = "/storage/emulated/0/Documents";
                            }
                            Console.WriteLine($"Using fallback path: {finalPath}");
                        }
                    }
                    
                    // Test path access
                    try
                    {
                        var canAccess = await _jsRuntime.InvokeAsync<bool>("testPathAccess", finalPath);
                        Console.WriteLine($"Path access test for '{finalPath}': {canAccess}");
                    }
                    catch (Exception testEx)
                    {
                        Console.WriteLine($"Path access test failed: {testEx.Message}");
                    }
                    
                    return finalPath;
                }
                
                Console.WriteLine("User cancelled directory selection");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing directory picker: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Fallback to default path
                return "/storage/emulated/0/Documents/Notes";
            }
        }
    }
}
