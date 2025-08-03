using System.Collections.Generic;
using System.Threading.Tasks;
using Misbah.Core.Models;

namespace Misbah.Core.Services
{
    public interface INoteService
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
        
        /// <summary>
        /// Sets the root path for the note service
        /// </summary>
        void SetRootPath(string rootPath);
    }
}
