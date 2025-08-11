using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Services;

namespace Misbah.Infrastructure.Persistence.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly IHubPathProvider _hubPathProvider;
        private readonly INoteRepository _noteRepo;

        public FolderRepository(IHubPathProvider hubPathProvider, INoteRepository noteRepo)
        {
            _hubPathProvider = hubPathProvider;
            _noteRepo = noteRepo;
        }

        public async Task<FolderNode> GetByPathAsync(string path)
        {
            var basePath = _hubPathProvider.GetCurrentHubPath();
            var fullPath = string.IsNullOrEmpty(path) || path == "/" ? basePath : Path.Combine(basePath, path.TrimStart(Path.DirectorySeparatorChar));

            if (!Directory.Exists(fullPath))
                return null;

            var folder = new FolderNode
            {
                Path = path ?? "/",
                Name = string.IsNullOrEmpty(path) || path == "/" ? Path.GetFileName(basePath) : Path.GetFileName(fullPath),
            };

            // Get subfolders
            var subDirs = Directory.GetDirectories(fullPath)
                .Where(dir => !Path.GetFileName(dir).StartsWith(".")) // Skip hidden folders
                .ToList();

            foreach (var subDir in subDirs)
            {
                var relativePath = Path.GetRelativePath(basePath, subDir).Replace(Path.DirectorySeparatorChar, '/');
                var subFolder = await GetByPathAsync(relativePath);
                if (subFolder != null)
                {
                    folder.Folders.Add(subFolder);
                }
            }

            // Get notes from the NoteRepository
            var allNotes = await _noteRepo.GetAllAsync();
            Console.WriteLine($"DEBUG: FolderRepository - Got {allNotes.Count()} total notes from repository");
            Console.WriteLine($"DEBUG: FolderRepository - Looking for notes in directory: '{fullPath}'");
            
            // Filter notes that are in this specific directory
            var notesInThisDir = allNotes.Where(note => 
            {
                // Get the directory of the note file
                var noteDir = Path.GetDirectoryName(note.FilePath);
                Console.WriteLine($"DEBUG: FolderRepository - Note '{note.Id}' is in directory: '{noteDir}'");
                
                // Compare the directories (normalize path separators and case)
                var normalizedNoteDir = noteDir?.Replace('\\', '/').TrimEnd('/');
                var normalizedFullPath = fullPath.Replace('\\', '/').TrimEnd('/');
                
                var matches = string.Equals(normalizedNoteDir, normalizedFullPath, StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"DEBUG: FolderRepository - Note '{note.Id}' matches directory: {matches}");
                
                return matches;
            }).ToList();

            Console.WriteLine($"DEBUG: FolderRepository - Found {notesInThisDir.Count} notes in this directory");
            
            folder.Notes = notesInThisDir.ToList();

            return folder;
        }

        public Task<IEnumerable<FolderNode>> GetAllAsync()
        {
            throw new NotImplementedException("GetAllAsync not implemented yet");
        }

        public Task AddAsync(FolderNode folder)
        {
            throw new NotImplementedException("AddAsync not implemented yet");
        }

        public Task UpdateAsync(FolderNode folder)
        {
            throw new NotImplementedException("UpdateAsync not implemented yet");
        }

        public Task DeleteAsync(string path)
        {
            throw new NotImplementedException("DeleteAsync not implemented yet");
        }
    }
}
