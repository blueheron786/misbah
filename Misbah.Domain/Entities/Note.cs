using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Misbah.Domain.ValueObjects;
using Misbah.Domain.Events;

namespace Misbah.Domain.Entities
{
    /// <summary>
    /// Rich domain entity representing a note with business logic
    /// </summary>
    public class Note
    {
        private readonly List<DomainEvent> _domainEvents = new();
        private MarkdownContent _content = new("");
        
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        
        public MarkdownContent Content 
        { 
            get => _content ?? new MarkdownContent("");
            set => _content = value;
        }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        // Rich domain properties
        public IReadOnlyList<string> ExtractedTags => Content.ExtractTags();
        public IReadOnlyList<string> WikiLinks => Content.ExtractWikiLinks();
        public int WordCount => Content.WordCount;
        public bool IsEmpty => Content.IsEmpty;
        public bool HasBeenModified { get; private set; }
        
        // Domain events
        public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        // Factory methods
        public static Note CreateNew(string title, string folderPath, string? content = null)
        {
            var id = GenerateId(title);
            var filePath = Path.Combine(folderPath, $"{SanitizeFileName(title)}.md");
            var now = DateTime.UtcNow;
            
            var note = new Note
            {
                Id = id,
                Title = title,
                FilePath = filePath,
                Content = new MarkdownContent(content ?? ""),
                Created = now,
                Modified = now
            };
            
            note.AddDomainEvent(new NoteCreated(id, title, filePath));
            return note;
        }
        
        public static Note FromExisting(string id, string title, string filePath, string content, DateTime created, DateTime modified)
        {
            return new Note
            {
                Id = id,
                Title = title,
                FilePath = filePath,
                Content = new MarkdownContent(content),
                Created = created,
                Modified = modified
            };
        }
        
        // Business logic methods
        public void UpdateContent(string newContent)
        {
            var oldTitle = Title;
            _content = new MarkdownContent(newContent);
            
            // Extract title from content if available
            var extractedTitle = _content.ExtractTitle();
            if (!string.IsNullOrEmpty(extractedTitle) && extractedTitle != Title)
            {
                Title = extractedTitle;
                AddDomainEvent(new NoteUpdated(Id, oldTitle, Title));
            }
            
            Modified = DateTime.UtcNow;
            HasBeenModified = true;
        }
        
        public void UpdateTitle(string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title cannot be empty", nameof(newTitle));
                
            var oldTitle = Title;
            Title = newTitle;
            Modified = DateTime.UtcNow;
            HasBeenModified = true;
            
            AddDomainEvent(new NoteUpdated(Id, oldTitle, newTitle));
        }
        
        public void MarkAsDeleted()
        {
            AddDomainEvent(new NoteDeleted(Id, Title, FilePath));
        }
        
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
        
        private void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        
        private static string GenerateId(string title)
        {
            // Create a deterministic ID based on title and timestamp
            var sanitized = SanitizeFileName(title);
            var timestamp = DateTime.UtcNow.Ticks.ToString("x");
            return $"{sanitized}-{timestamp}";
        }
        
        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var result = fileName;
            foreach (var c in invalidChars)
            {
                result = result.Replace(c, '-');
            }
            return result.Replace(" ", "-").ToLowerInvariant();
        }
    }
}
