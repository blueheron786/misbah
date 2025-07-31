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
            if (string.IsNullOrWhiteSpace(query)) return new List<Note>();
            var phrase = query.ToLower().Trim();
            var words = phrase.Split(' ', '\t', '\n').Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();

            var results = _allNotes
                .Where(note =>
                    words.All(word =>
                        (note.Title?.ToLower().Contains(word) ?? false)
                        || (note.Content?.ToLower().Contains(word) ?? false)
                        || (note.Tags.Any(tag => tag.ToLower().Contains(word)))
                    )
                );

            var toReturn = results
                .Select(note => new
                {
                    Note = note,
                    Score = GetScore(note, phrase, words),
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Note.Title)
                .Select(x => x.Note)
                .DistinctBy(x => x.Id)
                .ToList();

            return toReturn;
        }

        private int GetScore(Note note, string phrase, string[] words)
        {
            int score = 0;
            // Phrase match in title/content/tags
            if ((note.Title?.ToLower().Contains(phrase) ?? false)) score += 100;
            if ((note.Content?.ToLower().Contains(phrase) ?? false)) score += 50;
            if (note.Tags.Any(tag => tag.ToLower().Contains(phrase))) score += 30;
            // Add 1 for each word match (already required by filter)
            score += words.Length;
            return score;
        }
    }
}
