using System.Collections.Generic;
using System.Linq;
using Misbah.Domain.Entities;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;

namespace Misbah.Infrastructure.Services
{
    public class SearchService
    {
        private readonly INoteService _noteService;
        public SearchService(INoteService noteService)
        {
            _noteService = noteService;
        }

        public async Task<List<NoteDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<NoteDto>();
            var phrase = query.ToLower().Trim();
            var words = phrase.Split(' ', '\t', '\n').Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();

            var allNotes = (await _noteService.GetAllNotesAsync()).ToArray();

            var results = allNotes
                .Where(note =>
                    words.All(word =>
                        (note.Title?.ToLower().Contains(word) ?? false)
                        || (note.Content?.ToLower().Contains(word) ?? false)
                        || (note.Tags != null && note.Tags.Any(tag => tag.ToLower().Contains(word)))
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

        private int GetScore(NoteDto note, string phrase, string[] words)
        {
            int score = 0;
            // Phrase match in title/content/tags
            if ((note.Title?.ToLower().Contains(phrase) ?? false)) score += 100;
            if ((note.Content?.ToLower().Contains(phrase) ?? false)) score += 50;
            if (note.Tags != null && note.Tags.Any(tag => tag.ToLower().Contains(phrase))) score += 30;
            // Add 1 for each word match (already required by filter)
            score += words.Length;
            return score;
        }
    }
}
