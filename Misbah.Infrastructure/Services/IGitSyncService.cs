using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Misbah.Infrastructure.Services
{
    public interface IGitSyncService
    {
        /// <summary>
        /// Initializes the Git sync service with the repository path
        /// </summary>
        Task InitializeAsync(string repositoryPath);

        /// <summary>
        /// Starts automatic Git synchronization
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops automatic Git synchronization
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Manually triggers a Git sync operation
        /// </summary>
        Task SyncNowAsync();

        /// <summary>
        /// Adds a file to the staging area
        /// </summary>
        Task AddFileAsync(string filePath);

        /// <summary>
        /// Adds multiple files to the staging area
        /// </summary>
        Task AddFilesAsync(IEnumerable<string> filePaths);

        /// <summary>
        /// Commits staged changes with an auto-generated message
        /// </summary>
        Task CommitStagedChangesAsync();

        /// <summary>
        /// Runs Git garbage collection (desktop only)
        /// </summary>
        Task RunGarbageCollectionAsync();

        /// <summary>
        /// Event raised when sync operation completes
        /// </summary>
        event EventHandler<GitSyncEventArgs> SyncCompleted;

        /// <summary>
        /// Event raised when an error occurs during sync
        /// </summary>
        event EventHandler<GitSyncErrorEventArgs> SyncError;

        /// <summary>
        /// Gets the current sync status
        /// </summary>
        GitSyncStatus Status { get; }

        /// <summary>
        /// Gets whether the service is currently running
        /// </summary>
        bool IsRunning { get; }
    }

    public class GitSyncEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public int FilesChanged { get; set; }
        public IEnumerable<string> ChangedFiles { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class GitSyncErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum GitSyncStatus
    {
        Stopped,
        Starting,
        Running,
        Syncing,
        Error
    }
}
