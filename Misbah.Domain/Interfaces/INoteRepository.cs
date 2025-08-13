using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Domain.Interfaces
{
    public interface INoteRepository
    {
        // Existing methods
        IEnumerable<Note> GetAllNotes();
        Note GetNote(string filePath);
        Task<Note> GetNoteAsync(string filePath);
        void SaveNote(Note note);
        Task SaveNoteAsync(Note note);
        void DeleteNote(string filePath);
        Task DeleteNoteAsync(string filePath);
        Note CreateNote(string folderPath, string title);
        Task<Note> CreateNoteAsync(string folderPath, string title);
        
        // New enhanced methods for rich domain support
        Task<Note?> GetNoteByIdAsync(string noteId);
        Note? GetNoteById(string noteId);
        Task<IEnumerable<Note>> SearchNotesAsync(string searchTerm);
        IEnumerable<Note> SearchNotes(string searchTerm);
        Task<IEnumerable<Note>> GetNotesByTagAsync(string tag);
        IEnumerable<Note> GetNotesByTag(string tag);
        Task<IEnumerable<string>> GetAllTagsAsync();
        IEnumerable<string> GetAllTags();
    }
}
