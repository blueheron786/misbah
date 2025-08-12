using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Core.Services;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Infrastructure.Repositories
{
    /// <summary>
    /// Adapter that wraps the existing NoteService to implement the new repository interface
    /// This allows us to use Clean Architecture while keeping existing functionality intact
    /// </summary>
    public class NoteRepositoryAdapter : INoteRepository
    {
        private readonly INoteService _legacyNoteService;

        public NoteRepositoryAdapter(INoteService legacyNoteService)
        {
            _legacyNoteService = legacyNoteService;
        }

        public IEnumerable<Note> GetAllNotes()
        {
            var legacyNotes = _legacyNoteService.GetAllNotes();
            return MapToNewNotes(legacyNotes);
        }

        public Note GetNote(string filePath)
        {
            var legacyNote = _legacyNoteService.LoadNote(filePath);
            return MapToNewNote(legacyNote);
        }

        public async Task<Note> GetNoteAsync(string filePath)
        {
            // Since the legacy service doesn't have LoadNoteAsync, we'll use the sync version
            // In a real refactor, you'd want to add the async version to the legacy service
            var legacyNote = _legacyNoteService.LoadNote(filePath);
            return await Task.FromResult(MapToNewNote(legacyNote));
        }

        public void SaveNote(Note note)
        {
            var legacyNote = MapToLegacyNote(note);
            _legacyNoteService.SaveNote(legacyNote);
        }

        public async Task SaveNoteAsync(Note note)
        {
            var legacyNote = MapToLegacyNote(note);
            await _legacyNoteService.SaveNoteAsync(legacyNote);
        }

        public void DeleteNote(string filePath)
        {
            _legacyNoteService.DeleteNote(filePath);
        }

        public async Task DeleteNoteAsync(string filePath)
        {
            await _legacyNoteService.DeleteNoteAsync(filePath);
        }

        public Note CreateNote(string folderPath, string title)
        {
            var legacyNote = _legacyNoteService.CreateNote(folderPath, title);
            return MapToNewNote(legacyNote);
        }

        public async Task<Note> CreateNoteAsync(string folderPath, string title)
        {
            var legacyNote = await _legacyNoteService.CreateNoteAsync(folderPath, title);
            return MapToNewNote(legacyNote);
        }

        // Mapping methods to convert between legacy and new models
        private Note MapToNewNote(Misbah.Core.Models.Note legacyNote)
        {
            return new Note
            {
                Id = legacyNote.Id,
                Title = legacyNote.Title,
                Content = legacyNote.Content,
                Tags = ExtractTags(legacyNote.Content ?? string.Empty), // Extract tags from content
                Created = legacyNote.Created,
                Modified = legacyNote.Modified,
                FilePath = legacyNote.FilePath
            };
        }

        private IEnumerable<Note> MapToNewNotes(IEnumerable<Misbah.Core.Models.Note> legacyNotes)
        {
            var notes = new List<Note>();
            foreach (var legacyNote in legacyNotes)
            {
                notes.Add(MapToNewNote(legacyNote));
            }
            return notes;
        }

        private Misbah.Core.Models.Note MapToLegacyNote(Note note)
        {
            return new Misbah.Core.Models.Note
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                Tags = new List<string>(note.Tags),
                Created = note.Created,
                Modified = note.Modified,
                FilePath = note.FilePath
            };
        }

        /// <summary>
        /// Extracts tags from content using regex pattern #tagname
        /// </summary>
        private List<string> ExtractTags(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            var tagMatches = System.Text.RegularExpressions.Regex.Matches(content, @"#(\w+)");
            return tagMatches.Cast<System.Text.RegularExpressions.Match>()
                           .Select(m => m.Groups[1].Value)
                           .Distinct()
                           .ToList();
        }
    }
}
