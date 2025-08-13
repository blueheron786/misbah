using System;
using System.Diagnostics;
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
                    catch (Exception ex) { Debug.WriteLine($"[FolderService] Skipped subfolder: {dir}, ex: {ex.Message}"); }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[FolderService] Failed to enumerate directories in {path}: {ex.Message}"); }
            try
            {
                foreach (var file in Directory.GetFiles(path, "*.md"))
                {
                    try
                    {
                        folder.Notes.Add(new Note { Id = file, Title = Path.GetFileNameWithoutExtension(file), Content = "", FilePath = file });
                    }
                    catch (Exception ex) { Debug.WriteLine($"[FolderService] Skipped file: {file}, ex: {ex.Message}"); }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[FolderService] Failed to enumerate files in {path}: {ex.Message}"); }
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
