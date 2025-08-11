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
            
            Console.WriteLine($"DEBUG: Looking for note with ID: '{id}'");
            Console.WriteLine($"DEBUG: Base path: '{basePath}'");

            // Search for the note file recursively in all subdirectories
            if (!Directory.Exists(basePath))
            {
                Console.WriteLine($"DEBUG: Base path does not exist");
                return null!;
            }

            // Search for the note file with this ID in any subdirectory
            var noteFileName = id + FileExtension;
            var foundFiles = Directory.GetFiles(basePath, noteFileName, SearchOption.AllDirectories);
            
            Console.WriteLine($"DEBUG: Searching for file: '{noteFileName}'");
            Console.WriteLine($"DEBUG: Found {foundFiles.Length} matching files:");
            foreach (var file in foundFiles)
            {
                Console.WriteLine($"DEBUG:   - {file}");
            }

            if (foundFiles.Length == 0)
            {
                Console.WriteLine($"DEBUG: No files found with name '{noteFileName}'");
                return null!;
            }

            // Use the first match (in case of duplicates)
            var filePath = foundFiles[0];
            Console.WriteLine($"DEBUG: Using file: '{filePath}'");

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
            
            Console.WriteLine($"DEBUG: GetAllAsync - Base path: '{basePath}'");
            
            if (!Directory.Exists(basePath))
            {
                Console.WriteLine($"DEBUG: GetAllAsync - Base path does not exist");
                return notes;
            }

            // Search recursively for all .md files in all subdirectories
            var mdFiles = Directory.GetFiles(basePath, "*.md", SearchOption.AllDirectories);
            
            Console.WriteLine($"DEBUG: GetAllAsync - Found {mdFiles.Length} total .md files");
            
            foreach (var filePath in mdFiles)
            {
                Console.WriteLine($"DEBUG: GetAllAsync - Processing file: '{filePath}'");
                
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

            Console.WriteLine($"DEBUG: GetAllAsync - Returning {notes.Count} notes");
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
