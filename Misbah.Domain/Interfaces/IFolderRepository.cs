using Misbah.Domain.Entities;

namespace Misbah.Domain.Interfaces
{
    public interface IFolderRepository
    {
        FolderNode GetFolderTree(string rootPath);
        void SetRootPath(string rootPath);
        string GetRootPath();
    }
}
