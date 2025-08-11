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
            var filePath = Path.Combine(basePath, id + FileExtension);

            Console.WriteLine($"DEBUG: Looking for note with ID: '{id}'");
            Console.WriteLine($"DEBUG: Full file path: '{filePath}'");
            Console.WriteLine($"DEBUG: File exists: {File.Exists(filePath)}");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"DEBUG: Base path: '{basePath}'");
                Console.WriteLine($"DEBUG: Files in base path:");
                if (Directory.Exists(basePath))
                {
                    foreach (var file in Directory.GetFiles(basePath, "*.md"))
                    {
                        Console.WriteLine($"DEBUG:   - {Path.GetFileName(file)} (ID would be: {Path.GetFileNameWithoutExtension(file)})");
                    }
                }
                return null;
            }

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

            var mdFiles = Directory.GetFiles(basePath, "*.md");
            
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
