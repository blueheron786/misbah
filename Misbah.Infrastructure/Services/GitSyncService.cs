using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Misbah.Infrastructure.Services
{
    public class GitSyncService : IGitSyncService, IDisposable
    {
        private readonly ILogger<GitSyncService> _logger;
        private readonly Timer _autoSaveTimer;
        private readonly Timer _gcTimer;
        private readonly object _lockObject = new object();
        private readonly string _misbahConfigDir = ".misbah";
        private readonly string _lastSyncFile = "last-sync.json";
        
        private string _repositoryPath = string.Empty;
        private Repository? _repository;
        private DateTime _lastCommitTime = DateTime.MinValue;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isDisposed;
        
        public GitSyncStatus Status { get; private set; } = GitSyncStatus.Stopped;
        public bool IsRunning => Status == GitSyncStatus.Running || Status == GitSyncStatus.Syncing;

        public event EventHandler<GitSyncEventArgs>? SyncCompleted;
        public event EventHandler<GitSyncErrorEventArgs>? SyncError;

        public GitSyncService(ILogger<GitSyncService> logger)
        {
            _logger = logger;
            _autoSaveTimer = new Timer(AutoSaveCallback, null, Timeout.Infinite, Timeout.Infinite);
            _gcTimer = new Timer(GcCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task InitializeAsync(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath))
                throw new ArgumentException("Repository path cannot be null or empty", nameof(repositoryPath));

            _repositoryPath = repositoryPath;
            
            // Create .misbah directory if it doesn't exist
            var configDir = Path.Combine(_repositoryPath, _misbahConfigDir);
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            // Initialize git repository if it doesn't exist
            if (!Directory.Exists(Path.Combine(_repositoryPath, ".git")))
            {
                _logger.LogInformation("Initializing Git repository at {Path}", _repositoryPath);
                Repository.Init(_repositoryPath);
            }

            _repository = new Repository(_repositoryPath);
            
            // Load last sync time
            await LoadLastSyncTimeAsync();
            
            _logger.LogInformation("GitSyncService initialized for repository: {Path}", _repositoryPath);
        }

        public async Task StartAsync()
        {
            if (_repository == null)
                throw new InvalidOperationException("Service must be initialized before starting");

            Status = GitSyncStatus.Starting;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Start auto-save timer (every 13 seconds)
                _autoSaveTimer.Change(TimeSpan.FromSeconds(13), TimeSpan.FromSeconds(13));
                
                // Start GC timer (every 30 minutes on desktop)
                if (IsDesktopPlatform())
                {
                    _gcTimer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
                }

                Status = GitSyncStatus.Running;
                _logger.LogInformation("GitSyncService started");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Status = GitSyncStatus.Error;
                _logger.LogError(ex, "Failed to start GitSyncService");
                OnSyncError("Failed to start GitSyncService", ex);
                throw;
            }
        }

        public async Task StopAsync()
        {
            Status = GitSyncStatus.Stopped;
            _cancellationTokenSource?.Cancel();
            
            // Stop timers
            _autoSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _gcTimer.Change(Timeout.Infinite, Timeout.Infinite);
            
            _logger.LogInformation("GitSyncService stopped");
            await Task.CompletedTask;
        }

        public async Task SyncNowAsync()
        {
            if (_repository == null)
                throw new InvalidOperationException("Service must be initialized before syncing");

            await PerformSyncAsync();
        }

        public async Task AddFileAsync(string filePath)
        {
            if (_repository == null)
                throw new InvalidOperationException("Service must be initialized");

            try
            {
                var relativePath = Path.GetRelativePath(_repositoryPath, filePath);
                Commands.Stage(_repository, relativePath);
                _logger.LogDebug("Added file to staging: {Path}", relativePath);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add file to staging: {Path}", filePath);
                OnSyncError($"Failed to add file to staging: {filePath}", ex);
                throw;
            }
        }

        public async Task AddFilesAsync(IEnumerable<string> filePaths)
        {
            if (_repository == null)
                throw new InvalidOperationException("Service must be initialized");

            try
            {
                var relativePaths = filePaths.Select(fp => Path.GetRelativePath(_repositoryPath, fp)).ToArray();
                Commands.Stage(_repository, relativePaths);
                _logger.LogDebug("Added {Count} files to staging", relativePaths.Length);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add files to staging");
                OnSyncError("Failed to add files to staging", ex);
                throw;
            }
        }

        public async Task CommitStagedChangesAsync()
        {
            if (_repository == null)
                throw new InvalidOperationException("Service must be initialized");

            // Don't commit within 15 seconds of previous commit
            if (DateTime.Now - _lastCommitTime < TimeSpan.FromSeconds(15))
            {
                _logger.LogDebug("Skipping commit - too soon after last commit");
                return;
            }

            try
            {
                var status = _repository.RetrieveStatus();
                var stagedFiles = status.Staged.Select(s => s.FilePath).ToList();
                
                if (!stagedFiles.Any())
                {
                    _logger.LogDebug("No staged files to commit");
                    return;
                }

                // Generate commit message
                var message = GenerateCommitMessage(stagedFiles);
                
                // Create signature
                var signature = new Signature("Misbah Auto-Sync", "misbah@auto.sync", DateTimeOffset.Now);
                
                // Commit
                var commit = _repository.Commit(message, signature, signature);
                _lastCommitTime = DateTime.Now;
                
                // Save sync time
                await SaveLastSyncTimeAsync();
                
                _logger.LogInformation("Committed {Count} files: {CommitId}", stagedFiles.Count, commit.Id.ToString()[..8]);
                
                OnSyncCompleted(message, stagedFiles.Count, stagedFiles);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit staged changes");
                OnSyncError("Failed to commit staged changes", ex);
                throw;
            }
        }

        public async Task RunGarbageCollectionAsync()
        {
            if (_repository == null)
                throw new InvalidOperationException("Service must be initialized");

            if (!IsDesktopPlatform())
            {
                _logger.LogDebug("Skipping GC - not on desktop platform");
                return;
            }

            try
            {
                _logger.LogInformation("Running Git garbage collection");
                
                // Run git gc --aggressive in the repository directory
                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "gc --aggressive",
                    WorkingDirectory = _repositoryPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processStartInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("Git garbage collection completed successfully");
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.LogWarning("Git garbage collection completed with warnings: {Error}", error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run Git garbage collection");
                OnSyncError("Failed to run Git garbage collection", ex);
            }
        }

        private async void AutoSaveCallback(object? state)
        {
            if (_cancellationTokenSource?.Token.IsCancellationRequested == true || Status != GitSyncStatus.Running)
                return;

            try
            {
                await PerformSyncAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during auto-save sync");
                OnSyncError("Error during auto-save sync", ex);
            }
        }

        private async void GcCallback(object? state)
        {
            if (_cancellationTokenSource?.Token.IsCancellationRequested == true || Status != GitSyncStatus.Running)
                return;

            try
            {
                await RunGarbageCollectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during garbage collection");
                OnSyncError("Error during garbage collection", ex);
            }
        }

        private async Task PerformSyncAsync()
        {
            if (_repository == null || Status == GitSyncStatus.Syncing)
                return;

            lock (_lockObject)
            {
                if (Status == GitSyncStatus.Syncing)
                    return;
                Status = GitSyncStatus.Syncing;
            }

            try
            {
                // Check if files have changed since last sync
                var changedFiles = await GetChangedFilesAsync();
                
                if (changedFiles.Any())
                {
                    await AddFilesAsync(changedFiles);
                    await CommitStagedChangesAsync();
                }
                else
                {
                    _logger.LogDebug("No files changed since last sync");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync operation");
                OnSyncError("Error during sync operation", ex);
            }
            finally
            {
                Status = GitSyncStatus.Running;
            }
        }

        private async Task<List<string>> GetChangedFilesAsync()
        {
            var changedFiles = new List<string>();
            var lastSyncData = await LoadLastSyncDataAsync();
            
            // Find all markdown files
            var mdFiles = Directory.EnumerateFiles(_repositoryPath, "*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains(_misbahConfigDir) && !Path.GetFileName(f).StartsWith("."))
                .ToList();

            foreach (var filePath in mdFiles)
            {
                var fileInfo = new FileInfo(filePath);
                var relativePath = Path.GetRelativePath(_repositoryPath, filePath);
                
                // Check if file has been modified since last sync
                if (!lastSyncData.ContainsKey(relativePath) ||
                    lastSyncData[relativePath] != fileInfo.LastWriteTime)
                {
                    changedFiles.Add(filePath);
                    lastSyncData[relativePath] = fileInfo.LastWriteTime;
                }
            }

            // Remove deleted files from tracking
            var currentFiles = mdFiles.Select(f => Path.GetRelativePath(_repositoryPath, f)).ToHashSet();
            var keysToRemove = lastSyncData.Keys.Where(k => !currentFiles.Contains(k)).ToList();
            foreach (var key in keysToRemove)
            {
                lastSyncData.Remove(key);
            }

            // Save updated sync data
            await SaveLastSyncDataAsync(lastSyncData);
            
            return changedFiles;
        }

        private async Task<Dictionary<string, DateTime>> LoadLastSyncDataAsync()
        {
            var syncFilePath = Path.Combine(_repositoryPath, _misbahConfigDir, _lastSyncFile);
            
            if (!File.Exists(syncFilePath))
                return new Dictionary<string, DateTime>();

            try
            {
                var json = await File.ReadAllTextAsync(syncFilePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);
                return data ?? new Dictionary<string, DateTime>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load last sync data, starting fresh");
                return new Dictionary<string, DateTime>();
            }
        }

        private async Task SaveLastSyncDataAsync(Dictionary<string, DateTime> data)
        {
            var syncFilePath = Path.Combine(_repositoryPath, _misbahConfigDir, _lastSyncFile);
            
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(syncFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save last sync data");
            }
        }

        private async Task LoadLastSyncTimeAsync()
        {
            try
            {
                var lastCommit = _repository?.Head?.Tip;
                if (lastCommit != null)
                {
                    _lastCommitTime = lastCommit.Author.When.DateTime;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load last commit time");
            }
            
            await Task.CompletedTask;
        }

        private async Task SaveLastSyncTimeAsync()
        {
            // The commit time is automatically tracked in git, so we don't need to save it separately
            await Task.CompletedTask;
        }

        private static string GenerateCommitMessage(List<string> stagedFiles)
        {
            var fileCount = stagedFiles.Count;
            var message = fileCount == 1 
                ? $"Updated {stagedFiles.First()}" 
                : $"Updated {fileCount} files";

            if (fileCount > 1 && fileCount <= 10)
            {
                message += "\n\n" + string.Join(", ", stagedFiles.Select(Path.GetFileName));
            }
            else if (fileCount > 10)
            {
                message += "\n\n" + string.Join(", ", stagedFiles.Take(10).Select(Path.GetFileName)) + "...";
            }

            return message;
        }

        private static bool IsDesktopPlatform()
        {
            // Simple check for desktop vs mobile platform
            return Environment.OSVersion.Platform == PlatformID.Win32NT ||
                   Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX;
        }

        private void OnSyncCompleted(string message, int filesChanged, IEnumerable<string> changedFiles)
        {
            SyncCompleted?.Invoke(this, new GitSyncEventArgs
            {
                Message = message,
                FilesChanged = filesChanged,
                ChangedFiles = changedFiles,
                Timestamp = DateTime.Now
            });
        }

        private void OnSyncError(string errorMessage, Exception? exception = null)
        {
            SyncError?.Invoke(this, new GitSyncErrorEventArgs
            {
                ErrorMessage = errorMessage,
                Exception = exception,
                Timestamp = DateTime.Now
            });
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _autoSaveTimer?.Dispose();
            _gcTimer?.Dispose();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _repository?.Dispose();
            
            _isDisposed = true;
        }
    }
}
