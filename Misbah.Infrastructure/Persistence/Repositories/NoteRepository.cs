using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Services;

namespace Misbah.Infrastructure.Persistence.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly IHubPathProvider _hubPathProvider;
        private const string FileExtension = ".md";

        public NoteRepository(IHubPathProvider hubPathProvider)
        {
            _hubPathProvider = hubPathProvider;
        }

        public async Task<Note> GetByIdAsync(string id)
        {
            var basePath = _hubPathProvider.GetCurrentHubPath();
            
            // Search for the note file recursively in all subdirectories
            if (!Directory.Exists(basePath))
            {
                return null!;
            }

            // Search for the note file with this ID in any subdirectory
            var noteFileName = id + FileExtension;
            var foundFiles = Directory.GetFiles(basePath, noteFileName, SearchOption.AllDirectories);
            
            if (foundFiles.Length == 0)
            {
                return null!;
            }

            // Use the first match (in case of duplicates)
            var filePath = foundFiles[0];

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var content = await File.ReadAllTextAsync(filePath);
            var fileInfo = new FileInfo(filePath);
            
            return new Note
            {
                Id = fileName,
                Title = fileName, // Just use filename
                Content = content,
                Tags = new List<string>(),
                Created = fileInfo.CreationTimeUtc,
                Modified = fileInfo.LastWriteTimeUtc,
                FilePath = filePath
            };
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            var notes = new List<Note>();
            var basePath = _hubPathProvider.GetCurrentHubPath();
            
            if (!Directory.Exists(basePath))
            {
                return notes;
            }

            // Search recursively for all .md files in all subdirectories
            var mdFiles = Directory.GetFiles(basePath, "*.md", SearchOption.AllDirectories);
            
            foreach (var filePath in mdFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var content = await File.ReadAllTextAsync(filePath);
                
                var fileInfo = new FileInfo(filePath);
                notes.Add(new Note
                {
                    Id = fileName,
                    Title = fileName, // Just use the filename
                    Content = content,
                    Tags = new List<string>(),
                    Created = fileInfo.CreationTimeUtc,
                    Modified = fileInfo.LastWriteTimeUtc,
                    FilePath = filePath
                });
            }

            return notes;
        }

        public Task AddAsync(Note note)
        {
            // For now, just implement basic creation
            throw new NotImplementedException("AddAsync not implemented yet");
        }

        public Task UpdateAsync(Note note)
        {
            // For now, just implement basic update  
            throw new NotImplementedException("UpdateAsync not implemented yet");
        }

        public Task DeleteAsync(string id)
        {
            // For now, just implement basic deletion
            throw new NotImplementedException("DeleteAsync not implemented yet");
        }
    }
}
