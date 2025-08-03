# Service Registration Example

Add this code to your Program.cs file to register the Git sync services:

```csharp
// Add logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register Git sync services
builder.Services.AddSingleton<IGitSyncService, GitSyncService>();
builder.Services.AddSingleton<IAutoSaveService, AutoSaveService>();
builder.Services.AddSingleton<IAutoSyncCoordinator, AutoSyncCoordinator>();

// Update NoteService registration to include Git sync
builder.Services.AddSingleton<INoteService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<NoteService>>();
    var gitSyncService = sp.GetRequiredService<IGitSyncService>();
    return new NoteService(logger, gitSyncService);
});

// Initialize the coordinator after building the app
var app = builder.Build();

// Get the coordinator and initialize it
var coordinator = app.Services.GetRequiredService<IAutoSyncCoordinator>();
var notesPath = "Notes"; // TODO: Get from configuration

try
{
    await coordinator.InitializeAsync(notesPath);
    await coordinator.StartAsync();
    
    // The coordinator is now ready to handle auto-save and Git sync
    Console.WriteLine("Auto-sync coordinator started successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to start auto-sync coordinator: {ex.Message}");
    // Continue without Git sync functionality
}

await app.RunAsync();
```
