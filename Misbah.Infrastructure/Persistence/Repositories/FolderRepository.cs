using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using Newtonsoft.Json;

namespace Misbah.Infrastructure.Persistence.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly string _basePath;
        private const string FoldersDirectory = "Folders";
        private const string FolderMetaFile = ".folder.meta";

        public FolderRepository(string basePath = null)
        {
            _basePath = basePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FoldersDirectory);
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<FolderNode> GetByPathAsync(string path)
        {
            var fullPath = Path.Combine(_basePath, path.TrimStart(Path.DirectorySeparatorChar));
            if (!Directory.Exists(fullPath))
            {
                return null;
            }

            var metaPath = Path.Combine(fullPath, FolderMetaFile);
            var folderNode = new FolderNode
            {
                Name = Path.GetFileName(fullPath),
                Path = path
            };

            // Load subfolders
            var subDirs = Directory.GetDirectories(fullPath)
                .Where(d => !Path.GetFileName(d).StartsWith("."));
            
            foreach (var dir in subDirs)
            {
                var subPath = Path.Combine(path, Path.GetFileName(dir));
                var subFolder = await GetByPathAsync(subPath);
                if (subFolder != null)
                {
                    folderNode.Folders.Add(subFolder);
                }
            }

            // Load notes (if any)
            var noteRepo = new NoteRepository(fullPath);
            var notes = await noteRepo.GetAllAsync();
            folderNode.Notes.AddRange(notes);

            return folderNode;
        }

        public async Task<IEnumerable<FolderNode>> GetAllAsync()
        {
            var rootDirs = Directory.GetDirectories(_basePath)
                .Where(d => !Path.GetFileName(d).StartsWith("."));
            
            var folders = new List<FolderNode>();
            foreach (var dir in rootDirs)
            {
                var folder = await GetByPathAsync(Path.GetFileName(dir));
                if (folder != null)
                {
                    folders.Add(folder);
                }
            }
            
            return folders;
        }

        public async Task<FolderNode> AddAsync(FolderNode folder)
        {
            var fullPath = Path.Combine(_basePath, folder.Path.TrimStart(Path.DirectorySeparatorChar));
            if (Directory.Exists(fullPath))
            {
                throw new InvalidOperationException($"Folder with path {folder.Path} already exists.");
            }

            Directory.CreateDirectory(fullPath);
            await File.WriteAllTextAsync(
                Path.Combine(fullPath, FolderMetaFile), 
                JsonConvert.SerializeObject(new { Created = DateTime.UtcNow }));

            return folder;
        }

        public async Task UpdateAsync(FolderNode folder)
        {
            var oldPath = Path.Combine(_basePath, folder.Path.TrimStart(Path.DirectorySeparatorChar));
            if (!Directory.Exists(oldPath))
            {
                throw new DirectoryNotFoundException($"Folder with path {folder.Path} not found.");
            }

            // If path has changed, move the directory
            if (folder.Path != folder.Name)
            {
                var newPath = Path.Combine(Path.GetDirectoryName(oldPath), folder.Name);
                Directory.Move(oldPath, newPath);
            }

            // Update metadata if needed
            var metaPath = Path.Combine(
                Path.Combine(_basePath, folder.Path.TrimStart(Path.DirectorySeparatorChar)), 
                FolderMetaFile);
            
            if (File.Exists(metaPath))
            {
                var meta = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    await File.ReadAllTextAsync(metaPath));
                meta["Modified"] = DateTime.UtcNow;
                await File.WriteAllTextAsync(metaPath, JsonConvert.SerializeObject(meta));
            }
        }

        public Task DeleteAsync(string path)
        {
            var fullPath = Path.Combine(_basePath, path.TrimStart(Path.DirectorySeparatorChar));
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            return Task.CompletedTask;
        }
    }
}
