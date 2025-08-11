using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Misbah.Infrastructure.Services
{
    public class AutoSaveService : IAutoSaveService, IDisposable
    {
        private readonly ILogger<AutoSaveService> _logger;
        private readonly Timer _autoSaveTimer;
        private readonly object _lockObject = new object();
        
        private string? _currentFilePath;
        private Func<Task>? _currentSaveAction;
        private bool _isDisposed;

        public string? CurrentFilePath => _currentFilePath;
        public bool IsRunning { get; private set; }

        public event EventHandler<AutoSaveEventArgs>? AutoSaved;
        public event EventHandler<AutoSaveErrorEventArgs>? AutoSaveError;

        public AutoSaveService(ILogger<AutoSaveService> logger)
        {
            _logger = logger;
            _autoSaveTimer = new Timer(AutoSaveCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task StartAutoSaveAsync(string filePath, Func<Task> saveAction)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            
            if (saveAction == null)
                throw new ArgumentNullException(nameof(saveAction));

            lock (_lockObject)
            {
                _currentFilePath = filePath;
                _currentSaveAction = saveAction;
                IsRunning = true;
                
                // Start timer for 13 seconds
                _autoSaveTimer.Change(TimeSpan.FromSeconds(13), TimeSpan.FromSeconds(13));
            }

            _logger.LogInformation("Auto-save started for file: {FilePath}", filePath);
            await Task.CompletedTask;
        }

        public async Task StopAutoSaveAsync()
        {
            lock (_lockObject)
            {
                _autoSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _currentFilePath = null;
                _currentSaveAction = null;
                IsRunning = false;
            }

            _logger.LogInformation("Auto-save stopped");
            await Task.CompletedTask;
        }

        public async Task ChangeFileAsync(string filePath, Func<Task> saveAction)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            
            if (saveAction == null)
                throw new ArgumentNullException(nameof(saveAction));

            lock (_lockObject)
            {
                _currentFilePath = filePath;
                _currentSaveAction = saveAction;
                
                if (IsRunning)
                {
                    // Reset timer
                    _autoSaveTimer.Change(TimeSpan.FromSeconds(13), TimeSpan.FromSeconds(13));
                }
            }

            _logger.LogInformation("Auto-save changed to file: {FilePath}", filePath);
            await Task.CompletedTask;
        }

        public async Task SaveNowAsync()
        {
            Func<Task>? saveAction;
            string? filePath;

            lock (_lockObject)
            {
                saveAction = _currentSaveAction;
                filePath = _currentFilePath;
            }

            if (saveAction != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    await saveAction();
                    OnAutoSaved(filePath);
                    _logger.LogDebug("Manual save completed for: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Manual save failed for: {FilePath}", filePath);
                    OnAutoSaveError(filePath, "Manual save failed", ex);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No file is currently being tracked for auto-save");
            }
        }

        private async void AutoSaveCallback(object? state)
        {
            Func<Task>? saveAction;
            string? filePath;

            lock (_lockObject)
            {
                if (!IsRunning)
                    return;

                saveAction = _currentSaveAction;
                filePath = _currentFilePath;
            }

            if (saveAction != null && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    await saveAction();
                    OnAutoSaved(filePath);
                    _logger.LogDebug("Auto-save completed for: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Auto-save failed for: {FilePath}", filePath);
                    OnAutoSaveError(filePath, "Auto-save failed", ex);
                }
            }
        }

        private void OnAutoSaved(string filePath)
        {
            AutoSaved?.Invoke(this, new AutoSaveEventArgs
            {
                FilePath = filePath,
                Timestamp = DateTime.Now
            });
        }

        private void OnAutoSaveError(string filePath, string errorMessage, Exception? exception = null)
        {
            AutoSaveError?.Invoke(this, new AutoSaveErrorEventArgs
            {
                FilePath = filePath,
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
            _isDisposed = true;
        }
    }
}
