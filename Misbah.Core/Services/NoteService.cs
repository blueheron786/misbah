using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Misbah.Core.Models;
using Microsoft.Extensions.Logging;

namespace Misbah.Core.Services
{
    public class NoteService : INoteService
    {
        private readonly ILogger<NoteService> _logger;
        private readonly IGitSyncService? _gitSyncService;
        private string _rootPath;

        public NoteService(ILogger<NoteService> logger, IGitSyncService gitSyncService)
        {
            _logger = logger;
            _gitSyncService = gitSyncService;
            _rootPath = string.Empty;
        }

        public NoteService(string rootPath, IGitSyncService gitSyncService)
            : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<NoteService>.Instance, gitSyncService)
        {
            _rootPath = rootPath;
        }

        public void SetRootPath(string rootPath)
        {
            _rootPath = rootPath;
        }

        public IEnumerable<Note> GetAllNotes()
        {
            var notes = new List<Note>();
            foreach (var file in Directory.EnumerateFiles(_rootPath, "*.md", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(_rootPath, file);
                var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (parts.Any(p => p.StartsWith(".", System.StringComparison.Ordinal)))
                    continue;
                notes.Add(LoadNote(file));
            }
            return notes;
        }

        public Note LoadNote(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var title = Path.GetFileNameWithoutExtension(filePath);
            var tags = ExtractTags(content);
            var info = new FileInfo(filePath);
            return new Note
            {
                Id = filePath,
                Title = title,
                Content = content,
                Tags = tags,
                Created = info.CreationTime,
                Modified = info.LastWriteTime,
                FilePath = filePath
            };
        }

        public void SaveNote(Note note)
        {
            File.WriteAllText(note.FilePath, note.Content);
        }

        public async Task SaveNoteAsync(Note note)
        {
            await File.WriteAllTextAsync(note.FilePath, note.Content);
            
            // Notify Git sync service if available
            _logger.LogInformation("SaveNoteAsync: GitSyncService is {Status}, IsRunning: {IsRunning}", 
                _gitSyncService == null ? "NULL" : "Available", 
                _gitSyncService?.IsRunning ?? false);
                
            if (_gitSyncService != null && _gitSyncService.IsRunning)
            {
                try
                {
                    _logger.LogInformation("Calling GitSyncService.AddFileAsync for: {FilePath}", note.FilePath);
                    await _gitSyncService.AddFileAsync(note.FilePath);
                    _logger.LogInformation("Successfully added note to Git staging: {FilePath}", note.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to add note to Git staging: {FilePath}", note.FilePath);
                }
            }
            else
            {
                _logger.LogWarning("GitSyncService not available or not running - file not added to Git: {FilePath}", note.FilePath);
            }
        }

        public Note CreateNote(string folderPath, string title)
        {
            var filePath = Path.Combine(folderPath, title + ".md");
            var note = new Note
            {
                Id = filePath,
                Title = title,
                Content = "# " + title + "\n",
                Tags = new List<string>(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                FilePath = filePath
            };
            SaveNote(note);
            return note;
        }

        public async Task<Note> CreateNoteAsync(string folderPath, string title)
        {
            var filePath = Path.Combine(folderPath, title + ".md");
            var note = new Note
            {
                Id = filePath,
                Title = title,
                Content = "# " + title + "\n",
                Tags = new List<string>(),
                Created = DateTime.Now,
                Modified = DateTime.Now,
                FilePath = filePath
            };
            await SaveNoteAsync(note);
            return note;
        }

        public void DeleteNote(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public async Task DeleteNoteAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                
                // Notify Git sync service if available
                if (_gitSyncService != null && _gitSyncService.IsRunning)
                {
                    try
                    {
                        // Git will detect the deletion when we stage changes
                        _logger.LogDebug("Note deleted: {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error handling Git sync for deleted note: {FilePath}", filePath);
                    }
                }
            }
            
            await Task.CompletedTask;
        }

        public List<string> ExtractTags(string content)
        {
            // Simple: #tag or tags in YAML frontmatter
            var tags = new HashSet<string>();
            var regex = new Regex(@"#(\w+)");
            foreach (Match match in regex.Matches(content))
            {
                tags.Add(match.Groups[1].Value);
            }
            // TODO: Add YAML frontmatter parsing if needed
            return tags.ToList();
        }
    }
}
