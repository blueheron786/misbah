using System.Collections.Generic;
using System.Linq;
using Misbah.Core.Models;
using Misbah.Core.Services;

namespace Misbah.Core.Utils
{
    public class SearchService
    {
        private readonly Note[] _allNotes;

        private readonly INoteService _noteService;
        public SearchService(INoteService noteService)
        {
            _noteService = noteService;
            _allNotes = _noteService.GetAllNotes().ToArray();
        }

        public List<Note> Search(string query)
        {
            query = query.ToLower();
            return _allNotes.Where(note =>
                (note.Title?.ToLower().Contains(query) ?? false) ||
                (note.Content?.ToLower().Contains(query) ?? false) ||
                (note.Tags.Any(tag => tag.ToLower().Contains(query)))
            ).ToList();
        }
    }
}
