using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using Newtonsoft.Json;

namespace Misbah.Infrastructure.Persistence.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly string _basePath;
        private const string NotesDirectory = "Notes";
        private const string FileExtension = ".md";
        private const string MetaExtension = ".meta";

        public NoteRepository(string basePath = null)
        {
            _basePath = basePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NotesDirectory);
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<Note> GetByIdAsync(string id)
        {
            var filePath = Path.Combine(_basePath, id + FileExtension);

            if (!File.Exists(filePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // Extract title from first line if it's a heading, otherwise use filename
            var title = fileName;
            var lines = content.Split('\n');
            if (lines.Length > 0 && lines[0].StartsWith("# "))
            {
                title = lines[0].Substring(2).Trim();
            }

            return new Note
            {
                Id = id,
                Title = title,
                Content = content,
                Tags = new List<string>(), // No tags from plain markdown files
                Created = File.GetCreationTime(filePath),
                Modified = File.GetLastWriteTime(filePath),
                FilePath = filePath
            };
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            var notes = new List<Note>();
            
            if (!Directory.Exists(_basePath))
            {
                return notes;
            }

            var mdFiles = Directory.GetFiles(_basePath, "*.md");
            
            foreach (var filePath in mdFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var content = await File.ReadAllTextAsync(filePath);
                
                // Use filename as title
                var title = fileName;
                
                var fileInfo = new FileInfo(filePath);
                notes.Add(new Note
                {
                    Id = fileName,
                    Title = title,
                    Content = content,
                    Tags = new List<string>(),
                    Created = fileInfo.CreationTimeUtc,
                    Modified = fileInfo.LastWriteTimeUtc,
                    FilePath = filePath
                });
            }
            
            return notes;
        }

    public async Task AddAsync(Note note)
        {
            var id = string.IsNullOrEmpty(note.Id) ? Guid.NewGuid().ToString() : note.Id;
            var filePath = Path.Combine(_basePath, id + FileExtension);
            var metaPath = GetMetaPath(id);

            var meta = new NoteMeta
            {
                Title = note.Title,
                Tags = note.Tags,
                Created = note.Created != default ? note.Created : DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            await File.WriteAllTextAsync(filePath, note.Content);
            await File.WriteAllTextAsync(metaPath, JsonConvert.SerializeObject(meta, Formatting.Indented));

            note.Id = id;
            note.FilePath = filePath;
            note.Created = meta.Created;
            note.Modified = meta.Modified;

            return;
        }

        public async Task UpdateAsync(Note note)
        {
            if (string.IsNullOrEmpty(note.Id))
            {
                throw new ArgumentException("Note must have an ID to be updated.");
            }

            var filePath = Path.Combine(_basePath, note.Id + FileExtension);
            var metaPath = GetMetaPath(note.Id);

            if (!File.Exists(filePath) || !File.Exists(metaPath))
            {
                throw new FileNotFoundException($"Note with id {note.Id} not found.");
            }

            var meta = new NoteMeta
            {
                Title = note.Title,
                Tags = note.Tags,
                Created = note.Created,
                Modified = DateTime.UtcNow
            };

            await File.WriteAllTextAsync(filePath, note.Content);
            await File.WriteAllTextAsync(metaPath, JsonConvert.SerializeObject(meta, Formatting.Indented));

            note.Modified = meta.Modified;
        }

        public Task DeleteAsync(string id)
        {
            var filePath = Path.Combine(_basePath, id + FileExtension);
            var metaPath = GetMetaPath(id);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }

            return Task.CompletedTask;
        }

        private string GetMetaPath(string id) => Path.Combine(_basePath, $"{id}{MetaExtension}");

        private class NoteMeta
        {
            public string Title { get; set; }
            public List<string> Tags { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
        }
    }
}
