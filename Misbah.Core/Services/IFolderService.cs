using Misbah.Core.Models;

namespace Misbah.Core.Services
{
    public interface IFolderService
    {
        FolderNode LoadFolderTree(string rootPath);
        void CreateFolder(string parentPath, string name);
        void DeleteFolder(string path);
    }
}
