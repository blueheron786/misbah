using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for folder operations following Clean Architecture principles
    /// </summary>
    public interface IFolderRepository
    {
        /// <summary>
        /// Gets the complete folder tree from the specified root path
        /// </summary>
        FolderNode GetFolderTree(string rootPath);
        
        /// <summary>
        /// Async version of loading folder tree
        /// </summary>
        Task<FolderNode> GetFolderTreeAsync(string rootPath);
        
        /// <summary>
        /// Gets all folders at a specific path
        /// </summary>
        IEnumerable<FolderNode> GetFolders(string path);
        
        /// <summary>
        /// Gets all folders at a specific path asynchronously  
        /// </summary>
        Task<IEnumerable<FolderNode>> GetFoldersAsync(string path);
        
        /// <summary>
        /// Creates a new folder at the specified path
        /// </summary>
        void CreateFolder(string path);
        
        /// <summary>
        /// Creates a new folder at the specified path asynchronously
        /// </summary>
        Task CreateFolderAsync(string path);
        
        /// <summary>
        /// Deletes a folder at the specified path
        /// </summary>
        void DeleteFolder(string path);
        
        /// <summary>
        /// Deletes a folder at the specified path asynchronously
        /// </summary>
        Task DeleteFolderAsync(string path);
        
        /// <summary>
        /// Checks if a folder exists at the specified path
        /// </summary>
        bool FolderExists(string path);
        
        /// <summary>
        /// Checks if a folder exists at the specified path asynchronously
        /// </summary>
        Task<bool> FolderExistsAsync(string path);
        
        /// <summary>
        /// Sets the root path for folder operations
        /// </summary>
        void SetRootPath(string rootPath);
        
        /// <summary>
        /// Gets the current root path
        /// </summary>
        string GetRootPath();
    }
}
