# Git Auto-Sync Documentation

## Overview

The Git Auto-Sync system provides automatic Git synchronization for your Misbah notes with the following features:

- **Auto-save**: Saves the currently open file every 13 seconds
- **Change detection**: Monitors file changes using `.misbah/last-sync.json` tracking
- **Smart commits**: Creates commits with descriptive messages including changed filenames
- **Commit throttling**: Prevents commits within 15 seconds of each other
- **Garbage collection**: Runs `git gc --aggressive` periodically on desktop platforms
- **File staging**: Automatically adds files to Git staging when they're saved

## Architecture

### Core Services

1. **`IGitSyncService`** - Handles all Git operations
2. **`IAutoSaveService`** - Manages auto-saving of the current file
3. **`IAutoSyncCoordinator`** - Coordinates auto-save and Git sync operations
4. **Updated `INoteService`** - Integrates with Git sync when saving notes

### Key Components

- **GitSyncService**: Core Git operations using LibGit2Sharp
- **AutoSaveService**: File auto-saving with 13-second intervals
- **AutoSyncCoordinator**: High-level coordination between services

## Setup

### 1. Add Package References

Already added to `Misbah.Core.csproj`:
```xml
<PackageReference Include="LibGit2Sharp" Version="0.30.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

### 2. Register Services

In your `Program.cs`:

```csharp
// Add logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register Git sync services
builder.Services.AddSingleton<IGitSyncService, GitSyncService>();
builder.Services.AddSingleton<IAutoSaveService, AutoSaveService>();
builder.Services.AddSingleton<IAutoSyncCoordinator, AutoSyncCoordinator>();

// Update NoteService to include Git sync
builder.Services.AddSingleton<INoteService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<NoteService>>();
    var gitSyncService = sp.GetRequiredService<IGitSyncService>();
    return new NoteService(logger, gitSyncService);
});
```

### 3. Initialize and Start

```csharp
var app = builder.Build();
var coordinator = app.Services.GetRequiredService<IAutoSyncCoordinator>();

try
{
    await coordinator.InitializeAsync("path/to/your/notes");
    await coordinator.StartAsync();
}
catch (Exception ex)
{
    // Handle initialization errors
}

await app.RunAsync();
```

## Usage

### In Blazor Components

```csharp
@inject IAutoSyncCoordinator AutoSyncCoordinator

// Start auto-save for a file
await AutoSyncCoordinator.StartAutoSaveForFileAsync(filePath, SaveAction);

// Change to a different file
await AutoSyncCoordinator.ChangeFileAsync(newFilePath, NewSaveAction);

// Stop auto-save
await AutoSyncCoordinator.StopAutoSaveAsync();

// Manual save and sync
await AutoSyncCoordinator.SaveAndSyncNowAsync();
```

### Using the NoteService

```csharp
// Async methods automatically integrate with Git
await noteService.SaveNoteAsync(note);
await noteService.CreateNoteAsync(folderPath, title);
await noteService.DeleteNoteAsync(filePath);
```

## How It Works

### File Change Detection

1. The system maintains a `.misbah/last-sync.json` file that tracks the last write time of each markdown file
2. Every 13 seconds (when auto-save runs), it compares current file timestamps with stored timestamps
3. Changed files are automatically added to Git staging
4. If there are staged changes and 15+ seconds have passed since the last commit, a new commit is created

### Auto-Save Process

1. When a file is opened for editing, call `StartAutoSaveForFileAsync()` with a save action
2. Every 13 seconds, the save action is executed
3. The saved file is automatically detected as changed and staged for Git commit
4. When the user switches files, call `ChangeFileAsync()` with the new file and save action

### Commit Messages

The system generates descriptive commit messages:

- Single file: `"Updated filename.md"`
- Multiple files: `"Updated 5 files\n\nfile1.md, file2.md, file3.md, file4.md, file5.md"`
- Many files: `"Updated 15 files\n\nfile1.md, file2.md, ... (first 10 shown)"`

### Git Garbage Collection

On desktop platforms, `git gc --aggressive` runs every 30 minutes to keep the repository size manageable.

## Configuration

### Timing Configuration

Currently hardcoded but can be made configurable:

- Auto-save interval: 13 seconds
- Minimum time between commits: 15 seconds
- Garbage collection interval: 30 minutes (desktop only)

### File Patterns

- Only `.md` files are tracked
- Files in `.misbah` directory are ignored
- Hidden files (starting with `.`) are ignored

## Error Handling

- All operations are wrapped in try-catch blocks with proper logging
- Git sync failures don't prevent the application from functioning
- Auto-save errors are logged but don't stop the auto-save process
- The system gracefully handles Git repository initialization

## Platform Differences

### Desktop
- Full Git operations including garbage collection
- File system watchers for change detection
- No restrictions on Git operations

### Android
- Git operations work but garbage collection is skipped
- May have different file system behavior
- Should work with Termux or similar Git implementations

## Troubleshooting

### Common Issues

1. **Git not initialized**: The system automatically runs `git init` if no repository exists
2. **Permission errors**: Ensure the application has write access to the notes directory
3. **LibGit2Sharp native libraries**: May need platform-specific native binaries

### Logging

Enable detailed logging to see what's happening:

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

Look for log messages like:
- `"Auto-save completed for: filename.md"`
- `"Added file to staging: filename.md"`
- `"Committed 3 files: 1a2b3c4d"`
- `"No files changed since last sync"`

### Manual Operations

You can always trigger manual operations:

```csharp
await coordinator.SaveAndSyncNowAsync(); // Save current file and sync
await gitSyncService.RunGarbageCollectionAsync(); // Manual GC
await gitSyncService.SyncNowAsync(); // Manual sync without save
```

## Future Enhancements

Possible improvements:

1. **Remote sync**: Push/pull to remote repositories
2. **Conflict resolution**: Handle merge conflicts
3. **Selective sync**: Choose which files to include/exclude
4. **Sync status UI**: Visual indicators of sync status
5. **Configuration options**: Make timing and behavior configurable
6. **Backup before sync**: Create backups before potentially destructive operations

## Files Created

The system creates the following files:

- `.git/`: Standard Git repository
- `.misbah/`: Configuration directory
- `.misbah/last-sync.json`: File change tracking data

## Performance Considerations

- File scanning is limited to `.md` files only
- Change detection uses file timestamps, not content hashing
- Commits are throttled to prevent excessive Git history
- Garbage collection helps keep repository size reasonable

This system provides a robust, automatic Git synchronization solution that works seamlessly in the background while you edit your notes.
