using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Core.Services;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Infrastructure.Repositories
{
    /// <summary>
    /// Adapter that bridges the legacy IFolderService to the new Clean Architecture IFolderRepository
    /// This enables gradual migration without breaking existing functionality
    /// </summary>
    public class FolderRepositoryAdapter : IFolderRepository
    {
        private readonly IFolderService _legacyFolderService;
        
        public FolderRepositoryAdapter(IFolderService legacyFolderService)
        {
            _legacyFolderService = legacyFolderService;
        }
        
        public FolderNode GetFolderTree(string rootPath)
        {
            var legacyFolderNode = _legacyFolderService.LoadFolderTree(rootPath);
            return MapToDomainFolderNode(legacyFolderNode);
        }
        
        public async Task<FolderNode> GetFolderTreeAsync(string rootPath)
        {
            // Legacy service is synchronous, so we wrap it in Task.Run for async compatibility
            var legacyFolderNode = await Task.Run(() => _legacyFolderService.LoadFolderTree(rootPath));
            return MapToDomainFolderNode(legacyFolderNode);
        }
        
        public FolderNode GetFolder(string path)
        {
            // For individual folders, we load the tree and find the specific folder
            var folderTree = _legacyFolderService.LoadFolderTree(path);
            return MapToDomainFolderNode(folderTree);
        }
        
        public async Task<FolderNode> GetFolderAsync(string path)
        {
            return await Task.Run(() => GetFolder(path));
        }
        
        public IEnumerable<FolderNode> GetFolders(string path)
        {
            var folder = GetFolder(path);
            return folder?.Folders ?? Enumerable.Empty<FolderNode>();
        }
        
        public async Task<IEnumerable<FolderNode>> GetFoldersAsync(string path)
        {
            var folder = await GetFolderAsync(path);
            return folder?.Folders ?? Enumerable.Empty<FolderNode>();
        }
        
        public void CreateFolder(string path)
        {
            var parentPath = System.IO.Path.GetDirectoryName(path);
            var folderName = System.IO.Path.GetFileName(path);
            _legacyFolderService.CreateFolder(parentPath, folderName);
        }
        
        public async Task CreateFolderAsync(string path)
        {
            await Task.Run(() => CreateFolder(path));
        }
        
        public void DeleteFolder(string path)
        {
            _legacyFolderService.DeleteFolder(path);
        }
        
        public async Task DeleteFolderAsync(string path)
        {
            await Task.Run(() => DeleteFolder(path));
        }
        
        public bool FolderExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }
        
        public async Task<bool> FolderExistsAsync(string path)
        {
            return await Task.Run(() => FolderExists(path));
        }
        
        public void SetRootPath(string rootPath)
        {
            // Legacy service doesn't have explicit root path setting
            // This would need to be coordinated with configuration
        }
        
        public string GetRootPath()
        {
            // For now, return current directory - this could be enhanced with configuration
            return System.IO.Directory.GetCurrentDirectory();
        }
        
        /// <summary>
        /// Maps a legacy Core FolderNode to a Domain FolderNode
        /// </summary>
        private FolderNode MapToDomainFolderNode(Misbah.Core.Models.FolderNode legacyNode)
        {
            if (legacyNode == null)
                return new FolderNode { Name = "", Path = "" };

            return new FolderNode
            {
                Name = legacyNode.Name ?? string.Empty,
                Path = legacyNode.Path ?? string.Empty,
                Folders = legacyNode.Folders?.Select(MapToDomainFolderNode).ToList() ?? new List<FolderNode>(),
                Notes = legacyNode.Notes?.Select(MapToDomainNote).ToList() ?? new List<Note>()
            };
        }

        /// <summary>
        /// Maps a legacy Core Note to a Domain Note (reusing the mapping logic from NoteRepositoryAdapter)
        /// </summary>
        private Note MapToDomainNote(Misbah.Core.Models.Note legacyNote)
        {
            if (legacyNote == null)
                return new Note { Id = "", Title = "", Content = "", FilePath = "" };

            return new Note
            {
                Id = legacyNote.Id ?? string.Empty,
                Title = legacyNote.Title ?? string.Empty,
                Content = legacyNote.Content ?? string.Empty,
                Modified = legacyNote.Modified,
                FilePath = legacyNote.FilePath ?? string.Empty,
                Tags = ExtractTags(legacyNote.Content ?? string.Empty)
            };
        }

        /// <summary>
        /// Extracts tags from content using regex (same logic as in NoteRepositoryAdapter)
        /// </summary>
        private List<string> ExtractTags(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            var tagMatches = System.Text.RegularExpressions.Regex.Matches(content, @"#(\w+)");
            return tagMatches.Cast<System.Text.RegularExpressions.Match>()
                           .Select(m => m.Groups[1].Value)
                           .Distinct()
                           .ToList();
        }
    }
}
