using System;
using System.Collections.Generic;
using Misbah.Core.Services;
using Misbah.Core.Models;

namespace Misbah.Web.Services
{
    public class GitCommandEntry
    {
        public string Command { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class GitCommandService : IDisposable
    {
        private readonly LinkedList<GitCommandEntry> _commands = new();
        private readonly IGitSyncService? _gitSyncService;
        private const int MaxCommands = 50;

        public GitCommandService(IGitSyncService? gitSyncService = null)
        {
            _gitSyncService = gitSyncService;
            if (_gitSyncService != null)
            {
                _gitSyncService.SyncCompleted += OnSyncCompleted;
                _gitSyncService.SyncError += OnSyncError;
            }
            
            // Add some sample commands for testing
            AddCommand("git init");
            AddCommand("git add .");
            AddCommand("git commit -m \"Initial commit\"");
            AddCommand("git status");
        }

        private void OnSyncCompleted(object? sender, GitSyncEventArgs e)
        {
            AddCommand($"Git sync completed: {e.Message} ({e.FilesChanged} files)");
        }

        private void OnSyncError(object? sender, GitSyncErrorEventArgs e)
        {
            AddCommand($"Git sync error: {e.ErrorMessage}");
        }

        public void AddCommand(string command)
        {
            _commands.AddFirst(new GitCommandEntry { Command = command, Timestamp = DateTime.Now });
            if (_commands.Count > MaxCommands)
                _commands.RemoveLast();
        }

        public IEnumerable<GitCommandEntry> GetCommands()
        {
            return _commands;
        }

        public void Dispose()
        {
            if (_gitSyncService != null)
            {
                _gitSyncService.SyncCompleted -= OnSyncCompleted;
                _gitSyncService.SyncError -= OnSyncError;
            }
        }
    }
}
