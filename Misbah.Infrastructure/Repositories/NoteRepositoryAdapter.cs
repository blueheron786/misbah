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
    /// Enhanced adapter that wraps the existing NoteService to implement the new repository interface
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
        
        // New enhanced methods for rich domain support
        public async Task<Note?> GetNoteByIdAsync(string noteId)
        {
            var allNotes = GetAllNotes();
            return await Task.FromResult(allNotes.FirstOrDefault(n => n.Id == noteId));
        }
        
        public Note? GetNoteById(string noteId)
        {
            return GetAllNotes().FirstOrDefault(n => n.Id == noteId);
        }
        
        public async Task<IEnumerable<Note>> SearchNotesAsync(string searchTerm)
        {
            var allNotes = GetAllNotes();
            var searchTermLower = searchTerm.ToLowerInvariant();
            
            var results = allNotes.Where(note =>
                note.Title.ToLowerInvariant().Contains(searchTermLower) ||
                note.Content.RawContent.ToLowerInvariant().Contains(searchTermLower));
                
            return await Task.FromResult(results);
        }
        
        public IEnumerable<Note> SearchNotes(string searchTerm)
        {
            var allNotes = GetAllNotes();
            var searchTermLower = searchTerm.ToLowerInvariant();
            
            return allNotes.Where(note =>
                note.Title.ToLowerInvariant().Contains(searchTermLower) ||
                note.Content.RawContent.ToLowerInvariant().Contains(searchTermLower));
        }
        
        public async Task<IEnumerable<Note>> GetNotesByTagAsync(string tag)
        {
            var allNotes = GetAllNotes();
            var results = allNotes.Where(note => note.ExtractedTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            return await Task.FromResult(results);
        }
        
        public IEnumerable<Note> GetNotesByTag(string tag)
        {
            return GetAllNotes().Where(note => note.ExtractedTags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }
        
        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            var allNotes = GetAllNotes();
            var tags = allNotes
                .SelectMany(note => note.ExtractedTags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag);
                
            return await Task.FromResult(tags);
        }
        
        public IEnumerable<string> GetAllTags()
        {
            return GetAllNotes()
                .SelectMany(note => note.ExtractedTags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag);
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
            return Note.FromExisting(
                legacyNote.Id,
                legacyNote.Title,
                legacyNote.FilePath,
                legacyNote.Content ?? "",
                legacyNote.Created,
                legacyNote.Modified
            );
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
                Content = note.Content.RawContent,
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
