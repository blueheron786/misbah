using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Domain.Entities;

namespace Misbah.Application.Interfaces
{
    public interface INoteApplicationService
    {
        IEnumerable<Note> GetAllNotes();
        Note LoadNote(string filePath);
        void SaveNote(Note note);
        Task SaveNoteAsync(Note note);
        Note CreateNote(string folderPath, string title);
        Task<Note> CreateNoteAsync(string folderPath, string title);
        void DeleteNote(string filePath);
        Task DeleteNoteAsync(string filePath);
        List<string> ExtractTags(string content);
        void SetRootPath(string rootPath);
    }
}
