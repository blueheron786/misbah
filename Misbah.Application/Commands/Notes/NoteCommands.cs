using Misbah.Application.Common;
using Misbah.Domain.Entities;
using System.Collections.Generic;

namespace Misbah.Application.Commands.Notes
{
    /// <summary>
    /// Command to create a new note
    /// </summary>
    public record CreateNoteCommand : ICommand<Note>
    {
        public string Title { get; }
        public string FolderPath { get; }
        public string? Content { get; }
        
        public CreateNoteCommand(string title, string folderPath, string? content = null)
        {
            Title = title;
            FolderPath = folderPath;
            Content = content;
        }
    }
    
    /// <summary>
    /// Command to update note content
    /// </summary>
    public record UpdateNoteContentCommand : ICommand
    {
        public string NoteId { get; }
        public string Content { get; }
        
        public UpdateNoteContentCommand(string noteId, string content)
        {
            NoteId = noteId;
            Content = content;
        }
    }
    
    /// <summary>
    /// Command to update note title
    /// </summary>
    public record UpdateNoteTitleCommand : ICommand
    {
        public string NoteId { get; }
        public string Title { get; }
        
        public UpdateNoteTitleCommand(string noteId, string title)
        {
            NoteId = noteId;
            Title = title;
        }
    }
    
    /// <summary>
    /// Command to delete a note
    /// </summary>
    public record DeleteNoteCommand : ICommand
    {
        public string NoteId { get; }
        
        public DeleteNoteCommand(string noteId)
        {
            NoteId = noteId;
        }
    }
    
    /// <summary>
    /// Command to save a note
    /// </summary>
    public record SaveNoteCommand : ICommand
    {
        public Note Note { get; }
        
        public SaveNoteCommand(Note note)
        {
            Note = note;
        }
    }
}
