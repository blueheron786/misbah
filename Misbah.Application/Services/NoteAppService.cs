using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Misbah.Application.Interfaces;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Application.Services
{
    public class NoteAppService : INoteAppService
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILogger<NoteAppService> _logger;

        public NoteAppService(INoteRepository noteRepository, ILogger<NoteAppService> logger)
        {
            _noteRepository = noteRepository;
            _logger = logger;
        }

        public IEnumerable<Note> GetAllNotes()
        {
            return _noteRepository.GetAllNotes();
        }

        public Note LoadNote(string filePath)
        {
            return _noteRepository.GetNote(filePath);
        }

        public async Task<Note> LoadNoteAsync(string filePath)
        {
            return await _noteRepository.GetNoteAsync(filePath);
        }

        public void SaveNote(Note note)
        {
            _noteRepository.SaveNote(note);
        }

        public async Task SaveNoteAsync(Note note)
        {
            await _noteRepository.SaveNoteAsync(note);
        }

        public Note CreateNote(string folderPath, string title)
        {
            return _noteRepository.CreateNote(folderPath, title);
        }

        public async Task<Note> CreateNoteAsync(string folderPath, string title)
        {
            return await _noteRepository.CreateNoteAsync(folderPath, title);
        }

        public void DeleteNote(string filePath)
        {
            _noteRepository.DeleteNote(filePath);
        }

        public async Task DeleteNoteAsync(string filePath)
        {
            await _noteRepository.DeleteNoteAsync(filePath);
        }

        public List<string> ExtractTags(string content)
        {
            var tags = new List<string>();
            var lines = content.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.StartsWith("#") && line.Contains(' '))
                {
                    var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        if (word.StartsWith("#") && word.Length > 1)
                        {
                            tags.Add(word.Substring(1));
                        }
                    }
                }
            }
            
            return tags;
        }

        public void SetRootPath(string rootPath)
        {
            // This could be handled through configuration instead
            // For now, we'll pass it through to the repository
        }
    }
}
