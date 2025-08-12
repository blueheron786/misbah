using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Application.Interfaces
{
    /// <summary>
    /// Application service interface for folder-related business logic
    /// This contains the business rules and orchestrates folder operations
    /// </summary>
    public interface IFolderApplicationService
    {
        /// <summary>
        /// Loads the complete folder tree with business logic applied
        /// </summary>
        FolderNode LoadFolderTree(string rootPath);
        
        /// <summary>
        /// Async version of loading folder tree
        /// </summary>
        Task<FolderNode> LoadFolderTreeAsync(string rootPath);
        
        /// <summary>
        /// Gets all folders with business rules applied (e.g., filtering hidden folders)
        /// </summary>
        IEnumerable<FolderNode> GetVisibleFolders(string path);
        
        /// <summary>
        /// Gets all folders asynchronously with business rules applied
        /// </summary>
        Task<IEnumerable<FolderNode>> GetVisibleFoldersAsync(string path);
        
        /// <summary>
        /// Creates a new folder with validation
        /// </summary>
        void CreateFolder(string path);
        
        /// <summary>
        /// Creates a new folder with validation asynchronously
        /// </summary>
        Task CreateFolderAsync(string path);
        
        /// <summary>
        /// Deletes a folder with business rule validation
        /// </summary>
        void DeleteFolder(string path);
        
        /// <summary>
        /// Deletes a folder with business rule validation asynchronously
        /// </summary>
        Task DeleteFolderAsync(string path);
        
        /// <summary>
        /// Validates if a folder path is valid according to business rules
        /// </summary>
        bool IsValidFolderPath(string path);
        
        /// <summary>
        /// Filters out hidden folders (those starting with '.')
        /// </summary>
        FolderNode FilterHiddenFolders(FolderNode rootFolder);
        
        /// <summary>
        /// Sets the root path with validation
        /// </summary>
        void SetRootPath(string rootPath);
        
        /// <summary>
        /// Gets the current root path
        /// </summary>
        string GetRootPath();
    }
}
