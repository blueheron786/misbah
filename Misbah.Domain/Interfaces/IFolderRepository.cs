using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Domain.Interfaces
{
    public interface IFolderRepository
    {
        Task<FolderNode> GetByPathAsync(string path);
        Task<IEnumerable<FolderNode>> GetAllAsync();
        Task AddAsync(FolderNode folder);
        Task UpdateAsync(FolderNode folder);
        Task DeleteAsync(string path);
    }
}
