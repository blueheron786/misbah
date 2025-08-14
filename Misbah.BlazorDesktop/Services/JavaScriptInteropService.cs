using Microsoft.JSInterop;
using System.IO;
using System.Text.Json;

namespace Misbah.BlazorDesktop.Services
{
    public static class JavaScriptInteropService
    {
        [JSInvokable("SaveContent")]
        public static async Task<string> SaveContent(object saveDataObject)
        {
            try
            {
                var saveDataJson = saveDataObject.ToString() ?? "";
                var saveData = JsonSerializer.Deserialize<SaveRequest>(saveDataJson);
                
                if (saveData == null)
                {
                    return "Error: Invalid save data";
                }

                // Determine the actual file to save based on the request
                string targetFilePath;
                string contentToSave;

                if (saveData.Content is JsonElement contentElement)
                {
                    // If we have a specific file path in the content, use it
                    if (contentElement.TryGetProperty("filePath", out var filePathElement) && 
                        filePathElement.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrEmpty(filePathElement.GetString()))
                    {
                        var relativePath = filePathElement.GetString()!;

                        // Build full path in Documents/Misbah
                        targetFilePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                            "Misbah", 
                            relativePath
                        );

                        // Extract the actual content to save
                        if (contentElement.TryGetProperty("content", out var actualContentElement))
                        {
                            contentToSave = actualContentElement.GetString() ?? "";
                        }
                        else
                        {
                            contentToSave = saveDataJson; // Fallback to full JSON
                        }
                    }
                    else
                    {
                        // No specific file path, create a default save location based on page
                        var pageBasedName = saveData.Path.Replace("/", "_").Replace("\\", "_").Trim('_');
                        if (string.IsNullOrEmpty(pageBasedName)) 
                            pageBasedName = "home";

                        targetFilePath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                            "Misbah", 
                            "PageContent",
                            $"{pageBasedName}.json"
                        );
                        contentToSave = saveDataJson;
                    }
                }
                else
                {
                    // Fallback for non-JSON content
                    var pageBasedName = saveData.Path.Replace("/", "_").Replace("\\", "_").Trim('_');
                    if (string.IsNullOrEmpty(pageBasedName)) 
                        pageBasedName = "home";

                    targetFilePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                        "Misbah", 
                        "PageContent",
                        $"{pageBasedName}.json"
                    );
                    contentToSave = saveDataJson;
                }

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);

                // Save to the target file (overwrites existing)
                await File.WriteAllTextAsync(targetFilePath, contentToSave);

                Console.WriteLine($"💾 [BlazorDesktop] Saved content to: {targetFilePath}");
                return $"Saved to {Path.GetFileName(targetFilePath)}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [BlazorDesktop] Save error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }

    // Record for the save request (same as Web project)
    public record SaveRequest(string Path, string Timestamp, object Content);
}
