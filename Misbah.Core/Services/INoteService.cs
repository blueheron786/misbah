using System.Collections.Generic;
using Misbah.Core.Models;

namespace Misbah.Core.Services
{
    public interface INoteService
    {
        IEnumerable<Note> GetAllNotes();
        Note LoadNote(string filePath);
        void SaveNote(Note note);
        Note CreateNote(string folderPath, string title);
        void DeleteNote(string filePath);
        List<string> ExtractTags(string content);
    }
}
