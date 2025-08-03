using System;
using System.Threading.Tasks;

namespace Misbah.Core.Services
{
    public interface IAutoSaveService
    {
        /// <summary>
        /// Starts auto-save for the specified file
        /// </summary>
        Task StartAutoSaveAsync(string filePath, Func<Task> saveAction);

        /// <summary>
        /// Stops auto-save for the currently tracked file
        /// </summary>
        Task StopAutoSaveAsync();

        /// <summary>
        /// Changes the file being auto-saved
        /// </summary>
        Task ChangeFileAsync(string filePath, Func<Task> saveAction);

        /// <summary>
        /// Manually triggers a save
        /// </summary>
        Task SaveNowAsync();

        /// <summary>
        /// Gets the currently tracked file path
        /// </summary>
        string? CurrentFilePath { get; }

        /// <summary>
        /// Gets whether auto-save is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Event raised when auto-save occurs
        /// </summary>
        event EventHandler<AutoSaveEventArgs> AutoSaved;

        /// <summary>
        /// Event raised when auto-save fails
        /// </summary>
        event EventHandler<AutoSaveErrorEventArgs> AutoSaveError;
    }

    public class AutoSaveEventArgs : EventArgs
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class AutoSaveErrorEventArgs : EventArgs
    {
        public string FilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
