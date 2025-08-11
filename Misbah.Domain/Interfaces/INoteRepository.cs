using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Domain.Interfaces
{
    public interface INoteRepository
    {
        Task<Note> GetByIdAsync(string id);
        Task<IEnumerable<Note>> GetAllAsync();
        Task AddAsync(Note note);
        Task UpdateAsync(Note note);
        Task DeleteAsync(string id);
    }
}
