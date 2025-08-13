using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Application.Interfaces;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Application.Services
{
    /// <summary>
    /// Application service that contains folder-related business logic
    /// This layer orchestrates folder operations and applies business rules
    /// </summary>
    public class FolderApplicationService : IFolderApplicationService
    {
        private readonly IFolderRepository _folderRepository;
        
        public FolderApplicationService(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }
        
        public FolderNode LoadFolderTree(string rootPath)
        {
            // Business rule: Validate root path exists
            if (!IsValidFolderPath(rootPath))
            {
                throw new DirectoryNotFoundException($"Root path does not exist: {rootPath}");
            }
            
            var folderTree = _folderRepository.GetFolderTree(rootPath);
            
            // Business rule: Filter out hidden folders
            return FilterHiddenFolders(folderTree);
        }
        
        public async Task<FolderNode> LoadFolderTreeAsync(string rootPath)
        {
            // Business rule: Validate root path exists
            if (!IsValidFolderPath(rootPath))
            {
                throw new DirectoryNotFoundException($"Root path does not exist: {rootPath}");
            }
            
            var folderTree = await _folderRepository.GetFolderTreeAsync(rootPath);
            
            // Business rule: Filter out hidden folders
            return FilterHiddenFolders(folderTree);
        }
        
        public IEnumerable<FolderNode> GetVisibleFolders(string path)
        {
            var folders = _folderRepository.GetFolders(path);
            
            // Business rule: Filter out hidden folders (starting with '.')
            return folders.Where(f => !IsHiddenFolder(f));
        }
        
        public async Task<IEnumerable<FolderNode>> GetVisibleFoldersAsync(string path)
        {
            var folders = await _folderRepository.GetFoldersAsync(path);
            
            // Business rule: Filter out hidden folders (starting with '.')
            return folders.Where(f => !IsHiddenFolder(f));
        }
        
        public void CreateFolder(string path)
        {
            // Business rule: Validate path before creation
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Folder path cannot be empty");
            }
            
            if (IsHiddenFolder(path))
            {
                throw new ArgumentException("Cannot create hidden folders through this interface");
            }
            
            _folderRepository.CreateFolder(path);
        }
        
        public async Task CreateFolderAsync(string path)
        {
            // Business rule: Validate path before creation
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Folder path cannot be empty");
            }
            
            if (IsHiddenFolder(path))
            {
                throw new ArgumentException("Cannot create hidden folders through this interface");
            }
            
            await _folderRepository.CreateFolderAsync(path);
        }
        
        public void DeleteFolder(string path)
        {
            // Business rule: Don't allow deletion of root folder
            if (path == GetRootPath())
            {
                throw new InvalidOperationException("Cannot delete root folder");
            }
            
            // Business rule: Validate folder exists before deletion
            if (!_folderRepository.FolderExists(path))
            {
                throw new DirectoryNotFoundException($"Folder does not exist: {path}");
            }
            
            _folderRepository.DeleteFolder(path);
        }
        
        public async Task DeleteFolderAsync(string path)
        {
            // Business rule: Don't allow deletion of root folder
            if (path == GetRootPath())
            {
                throw new InvalidOperationException("Cannot delete root folder");
            }
            
            // Business rule: Validate folder exists before deletion
            if (!await _folderRepository.FolderExistsAsync(path))
            {
                throw new DirectoryNotFoundException($"Folder does not exist: {path}");
            }
            
            await _folderRepository.DeleteFolderAsync(path);
        }
        
        public bool IsValidFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
                
            try
            {
                return Directory.Exists(path);
            }
            catch
            {
                return false;
            }
        }
        
        public FolderNode FilterHiddenFolders(FolderNode rootFolder)
        {
            if (rootFolder == null) 
            {
                // Return an empty folder node instead of null
                return new FolderNode { Name = "", Path = "" };
            }
            
            // Create a new folder node with filtered subfolders
            var filteredFolder = new FolderNode
            {
                Name = rootFolder.Name,
                Path = rootFolder.Path,
                Notes = rootFolder.Notes, // Keep all notes
                Folders = rootFolder.Folders
                    .Where(f => !IsHiddenFolder(f))
                    .Select(FilterHiddenFolders) // Recursively filter subfolders
                    .ToList()
            };
            
            return filteredFolder;
        }
        
        public void SetRootPath(string rootPath)
        {
            // Business rule: Validate root path before setting
            if (!IsValidFolderPath(rootPath))
            {
                throw new DirectoryNotFoundException($"Invalid root path: {rootPath}");
            }
            
            _folderRepository.SetRootPath(rootPath);
        }
        
        public string GetRootPath()
        {
            return _folderRepository.GetRootPath();
        }
        
        /// <summary>
        /// Business rule: Determines if a folder should be hidden from the UI
        /// </summary>
        private bool IsHiddenFolder(FolderNode folder)
        {
            return !string.IsNullOrEmpty(folder.Name) && folder.Name.StartsWith(".");
        }
        
        /// <summary>
        /// Business rule: Determines if a folder path represents a hidden folder
        /// </summary>
        private bool IsHiddenFolder(string path)
        {
            var folderName = Path.GetFileName(path);
            return !string.IsNullOrEmpty(folderName) && folderName.StartsWith(".");
        }
    }
}
