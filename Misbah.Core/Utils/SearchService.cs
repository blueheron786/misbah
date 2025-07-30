using System.Collections.Generic;
using System.Linq;
using Misbah.Core.Models;
using Misbah.Core.Services;

namespace Misbah.Core.Utils
{
    public class SearchService
    {
        private readonly INoteService _noteService;
        public SearchService(INoteService noteService)
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
