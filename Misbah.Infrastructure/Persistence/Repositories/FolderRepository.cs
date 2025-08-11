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

            // Get notes directly from this specific directory only (no reading file contents)
            var notesInThisDir = new List<Note>();
            
            if (Directory.Exists(fullPath))
            {
                var mdFiles = Directory.GetFiles(fullPath, "*.md", SearchOption.TopDirectoryOnly);
                
                foreach (var filePath in mdFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileInfo = new FileInfo(filePath);
                    
                    notesInThisDir.Add(new Note
                    {
                        Id = fileName,
                        Title = fileName,
                        Content = string.Empty, // Don't read content for tree view
                        Tags = new List<string>(),
                        Created = fileInfo.CreationTimeUtc,
                        Modified = fileInfo.LastWriteTimeUtc,
                        FilePath = filePath
                    });
                }
            }
            
            folder.Notes = notesInThisDir;

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
