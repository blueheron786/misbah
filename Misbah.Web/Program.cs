using Misbah.Core.Services;
using Misbah.Core.Utils;
using Misbah.Core.Models;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Repositories;
using Misbah.Infrastructure.Configuration;
using Misbah.Web.Components;

// Record for the save API
public record SaveRequest(string Path, string Timestamp, object Content);

namespace Misbah.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Configure shared services
            ConfigureServices(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();
            
            // Add minimal save API for universal Ctrl+S functionality
            app.MapPost("/api/save", async (HttpContext context, INoteService noteService) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var jsonContent = await reader.ReadToEndAsync();
                
                try
                {
                    var saveData = System.Text.Json.JsonSerializer.Deserialize<SaveRequest>(jsonContent);
                    if (saveData == null)
                    {
                        return Results.BadRequest(new { success = false, error = "Invalid save data" });
                    }
                    
                    // Determine the actual file to save based on the request
                    string targetFilePath;
                    string contentToSave;
                    
                    if (saveData.Content is System.Text.Json.JsonElement contentElement)
                    {
                        // If we have a specific file path in the content, use it
                        if (contentElement.TryGetProperty("filePath", out var filePathElement) && 
                            filePathElement.ValueKind == System.Text.Json.JsonValueKind.String &&
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
                                contentToSave = jsonContent; // Fallback to full JSON
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
                            contentToSave = jsonContent;
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
                        contentToSave = jsonContent;
                    }
                    
                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
                    
                    // Create a Note object and save using NoteService (includes Git integration)
                    var note = new Note
                    {
                        Id = targetFilePath,
                        FilePath = targetFilePath,
                        Content = contentToSave,
                        Title = Path.GetFileNameWithoutExtension(targetFilePath),
                        Modified = DateTime.Now
                    };
                    
                    // Use NoteService for saving (this triggers Git sync automatically)
                    await noteService.SaveNoteAsync(note);
                    
                    Console.WriteLine($"💾 Saved content via NoteService to: {targetFilePath}");
                    return Results.Ok(new { 
                        success = true, 
                        filePath = targetFilePath,
                        message = $"Saved to {Path.GetFileName(targetFilePath)}"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Save error: {ex.Message}");
                    return Results.BadRequest(new { success = false, error = ex.Message });
                }
            });
            
            app.MapRazorComponents<Routes>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            // Add localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Legacy services (maintained for backward compatibility)
            services.AddSingleton<SearchService>();
            services.AddSingleton<MarkdownRenderer>();

            // Git services - register GitSyncService if available
            services.AddSingleton<IGitSyncService, GitSyncService>();
            services.AddSingleton<Misbah.Web.Services.GitCommandService>();

            // Advanced Clean Architecture with CQRS and Domain Events
            services.AddAdvancedCleanArchitecture("Notes");

            // Keep legacy clean architecture services for existing components
            services.AddScoped<IFolderRepository, FolderRepositoryAdapter>();
            services.AddScoped<IFolderApplicationService, FolderApplicationService>();
        }
    }
}
