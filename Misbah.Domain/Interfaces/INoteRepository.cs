using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Domain.Interfaces
{
    public interface INoteRepository
    {
        IEnumerable<Note> GetAllNotes();
        Note GetNote(string filePath);
        Task<Note> GetNoteAsync(string filePath);
        void SaveNote(Note note);
        Task SaveNoteAsync(Note note);
        void DeleteNote(string filePath);
        Task DeleteNoteAsync(string filePath);
        Note CreateNote(string folderPath, string title);
        Task<Note> CreateNoteAsync(string folderPath, string title);
    }
}
