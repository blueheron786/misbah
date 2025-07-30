using System.Collections.Generic;
using System.IO;
using Misbah.Models;

namespace Misbah.Services
{
    public class FolderService
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
            foreach (var dir in Directory.GetDirectories(path))
            {
                folder.Folders.Add(LoadFolder(dir));
            }
            foreach (var file in Directory.GetFiles(path, "*.md"))
            {
                folder.Notes.Add(new Note { Id = file, Title = Path.GetFileNameWithoutExtension(file), FilePath = file });
            }
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
