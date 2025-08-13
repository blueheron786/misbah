using System;

namespace Misbah.Domain.Events
{
    /// <summary>
    /// Base class for all domain events
    /// </summary>
    public abstract record DomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public abstract string EventType { get; }
    }
    
    /// <summary>
    /// Event raised when a note is created
    /// </summary>
    public record NoteCreated : DomainEvent
    {
        public override string EventType => nameof(NoteCreated);
        public string NoteId { get; }
        public string Title { get; }
        public string FilePath { get; }
        
        public NoteCreated(string noteId, string title, string filePath)
        {
            NoteId = noteId;
            Title = title;
            FilePath = filePath;
        }
    }
    
    /// <summary>
    /// Event raised when a note is updated
    /// </summary>
    public record NoteUpdated : DomainEvent
    {
        public override string EventType => nameof(NoteUpdated);
        public string NoteId { get; }
        public string? PreviousTitle { get; }
        public string NewTitle { get; }
        
        public NoteUpdated(string noteId, string? previousTitle, string newTitle)
        {
            NoteId = noteId;
            PreviousTitle = previousTitle;
            NewTitle = newTitle;
        }
    }
    
    /// <summary>
    /// Event raised when a note is deleted
    /// </summary>
    public record NoteDeleted : DomainEvent
    {
        public override string EventType => nameof(NoteDeleted);
        public string NoteId { get; }
        public string Title { get; }
        public string FilePath { get; }
        
        public NoteDeleted(string noteId, string title, string filePath)
        {
            NoteId = noteId;
            Title = title;
            FilePath = filePath;
        }
    }
}
