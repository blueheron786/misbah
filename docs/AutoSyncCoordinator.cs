using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Misbah.Core.Services
{
    public interface IAutoSyncCoordinator
    {
        /// <summary>
        /// Initializes the coordinator with the repository path
        /// </summary>
        Task InitializeAsync(string repositoryPath);

        /// <summary>
        /// Starts the auto-sync coordination
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the auto-sync coordination
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Begins auto-saving for a specific file
        /// </summary>
        Task StartAutoSaveForFileAsync(string filePath, Func<Task> saveAction);

        /// <summary>
        /// Stops auto-saving for the current file
        /// </summary>
        Task StopAutoSaveAsync();

        /// <summary>
        /// Changes the file being auto-saved
        /// </summary>
        Task ChangeFileAsync(string filePath, Func<Task> saveAction);

        /// <summary>
        /// Manually triggers a save and sync
        /// </summary>
        Task SaveAndSyncNowAsync();

        /// <summary>
        /// Gets whether the coordinator is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the Git sync service status
        /// </summary>
        GitSyncStatus GitSyncStatus { get; }

        /// <summary>
        /// Gets the currently auto-saving file
        /// </summary>
        string? CurrentFilePath { get; }
    }

    public class AutoSyncCoordinator : IAutoSyncCoordinator, IDisposable
    {
        private readonly IAutoSaveService _autoSaveService;
        private readonly IGitSyncService _gitSyncService;
        private readonly ILogger<AutoSyncCoordinator> _logger;
        private bool _isDisposed;

        public bool IsRunning { get; private set; }
        public GitSyncStatus GitSyncStatus => _gitSyncService.Status;
        public string? CurrentFilePath => _autoSaveService.CurrentFilePath;

        public AutoSyncCoordinator(
            IAutoSaveService autoSaveService,
            IGitSyncService gitSyncService,
            ILogger<AutoSyncCoordinator> logger)
        {
            _autoSaveService = autoSaveService ?? throw new ArgumentNullException(nameof(autoSaveService));
            _gitSyncService = gitSyncService ?? throw new ArgumentNullException(nameof(gitSyncService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to auto-save events
            _autoSaveService.AutoSaved += OnAutoSaved;
            _autoSaveService.AutoSaveError += OnAutoSaveError;
        }

        public async Task InitializeAsync(string repositoryPath)
        {
            _logger.LogInformation("Initializing AutoSyncCoordinator for repository: {Path}", repositoryPath);
            await _gitSyncService.InitializeAsync(repositoryPath);
        }

        public async Task StartAsync()
        {
            if (IsRunning)
                return;

            _logger.LogInformation("Starting AutoSyncCoordinator");
            
            await _gitSyncService.StartAsync();
            IsRunning = true;
            
            _logger.LogInformation("AutoSyncCoordinator started successfully");
        }

        public async Task StopAsync()
        {
            if (!IsRunning)
                return;

            _logger.LogInformation("Stopping AutoSyncCoordinator");
            
            await _autoSaveService.StopAutoSaveAsync();
            await _gitSyncService.StopAsync();
            IsRunning = false;
            
            _logger.LogInformation("AutoSyncCoordinator stopped");
        }

        public async Task StartAutoSaveForFileAsync(string filePath, Func<Task> saveAction)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Coordinator must be started before auto-saving files");

            _logger.LogInformation("Starting auto-save for file: {FilePath}", filePath);
            await _autoSaveService.StartAutoSaveAsync(filePath, saveAction);
        }

        public async Task StopAutoSaveAsync()
        {
            _logger.LogInformation("Stopping auto-save");
            await _autoSaveService.StopAutoSaveAsync();
        }

        public async Task ChangeFileAsync(string filePath, Func<Task> saveAction)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Coordinator must be started before changing files");

            _logger.LogInformation("Changing auto-save to file: {FilePath}", filePath);
            await _autoSaveService.ChangeFileAsync(filePath, saveAction);
        }

        public async Task SaveAndSyncNowAsync()
        {
            _logger.LogInformation("Manual save and sync requested");
            
            try
            {
                // Save current file if auto-save is running
                if (_autoSaveService.IsRunning)
                {
                    await _autoSaveService.SaveNowAsync();
                }
                
                // Trigger Git sync
                await _gitSyncService.SyncNowAsync();
                
                _logger.LogInformation("Manual save and sync completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform manual save and sync");
                throw;
            }
        }

        private async void OnAutoSaved(object? sender, AutoSaveEventArgs e)
        {
            _logger.LogDebug("Auto-save completed for: {FilePath}", e.FilePath);
            
            // The GitSyncService will automatically detect and sync the file
            // based on its periodic check, so we don't need to do anything here
            await Task.CompletedTask;
        }

        private void OnAutoSaveError(object? sender, AutoSaveErrorEventArgs e)
        {
            _logger.LogError(e.Exception, "Auto-save failed for {FilePath}: {ErrorMessage}", 
                e.FilePath, e.ErrorMessage);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _autoSaveService.AutoSaved -= OnAutoSaved;
            _autoSaveService.AutoSaveError -= OnAutoSaveError;
            
            if (_autoSaveService is IDisposable autoSaveDisposable)
                autoSaveDisposable.Dispose();
            
            if (_gitSyncService is IDisposable gitSyncDisposable)
                gitSyncDisposable.Dispose();
            
            _isDisposed = true;
        }
    }
}
