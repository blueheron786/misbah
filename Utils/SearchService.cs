using System.Collections.Generic;
using System.IO;
using System.Linq;
using Misbah.Models;
using Misbah.Services;

namespace Misbah.Utils
{
    public class SearchService
    {
        private readonly NoteService _noteService;
        public SearchService(NoteService noteService)
        {
            _noteService = noteService;
        }

        public List<Note> Search(string query)
        {
            var allNotes = _noteService.GetAllNotes();
            query = query.ToLower();
            return allNotes.Where(note =>
                (note.Title?.ToLower().Contains(query) ?? false) ||
                (note.Content?.ToLower().Contains(query) ?? false) ||
                (note.Tags.Any(tag => tag.ToLower().Contains(query)))
            ).ToList();
        }
    }
}
