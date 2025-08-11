using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Application.DTOs;

namespace Misbah.Application.Interfaces
{
    public interface INoteService
    {
        Task<NoteDto> GetNoteByIdAsync(string id);
        Task<IEnumerable<NoteDto>> GetAllNotesAsync();
        Task<NoteDto> CreateNoteAsync(NoteDto noteDto);
        Task UpdateNoteAsync(NoteDto noteDto);
        Task DeleteNoteAsync(string id);
        Task<IEnumerable<NoteDto>> SearchNotesAsync(string searchTerm);
    }
}
