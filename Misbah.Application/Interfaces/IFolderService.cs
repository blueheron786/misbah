using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Application.DTOs;

namespace Misbah.Application.Interfaces
{
    public interface IFolderService
    {
        Task<FolderNodeDto> GetFolderByPathAsync(string path);
        Task<IEnumerable<FolderNodeDto>> GetAllFoldersAsync();
        Task<FolderNodeDto> CreateFolderAsync(FolderNodeDto folderDto);
        Task UpdateFolderAsync(FolderNodeDto folderDto);
        Task DeleteFolderAsync(string path);
    }
}
