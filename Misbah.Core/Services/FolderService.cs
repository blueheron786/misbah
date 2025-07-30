using System.Collections.Generic;
using System.IO;
using Misbah.Core.Models;

namespace Misbah.Core.Services
{
    public class FolderService : IFolderService
    {
        public FolderNode LoadFolderTree(string rootPath)
        {
            return LoadFolder(rootPath);
        }

        private FolderNode LoadFolder(string path)
        {
            var folder = new FolderNode
            {
                Name = Path.GetFileName(path),
                Path = path
            };
            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    // Avoid symlink recursion
                    if ((File.GetAttributes(dir) & FileAttributes.ReparsePoint) != 0)
                        continue;
                    try
                    {
                        folder.Folders.Add(LoadFolder(dir));
                    }
                    catch { /* skip inaccessible subfolder */ }
                }
            }
            catch { /* skip inaccessible folder */ }
            try
            {
                foreach (var file in Directory.GetFiles(path, "*.md"))
                {
                    try
                    {
                        folder.Notes.Add(new Note { Id = file, Title = Path.GetFileNameWithoutExtension(file), FilePath = file });
                    }
                    catch { /* skip inaccessible file */ }
                }
            }
            catch { /* skip files if folder is inaccessible */ }
            return folder;
        }

        public void CreateFolder(string parentPath, string name)
        {
            var newPath = Path.Combine(parentPath, name);
            Directory.CreateDirectory(newPath);
        }

        public void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
