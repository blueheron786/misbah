using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Misbah.Application.Interfaces;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Application.Services
{
    public class NoteApplicationService : INoteApplicationService
    {
        private readonly INoteRepository _noteRepository;
        
        public NoteApplicationService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public IEnumerable<Note> GetAllNotes()
        {
            return _noteRepository.GetAllNotes();
        }

        public Note LoadNote(string filePath)
        {
            return _noteRepository.GetNote(filePath);
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
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            var tagPattern = @"#(\w+)";
            var matches = Regex.Matches(content, tagPattern);
            return matches.Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
        }

        public void SetRootPath(string rootPath)
        {
            // This might need to be handled differently in Clean Architecture
            // For now, we'll need to pass this through to the repository
            // In a proper implementation, this would be configured at the infrastructure level
        }
    }
}
