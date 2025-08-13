using Misbah.Application.Common;
using Misbah.Domain.Entities;
using System.Collections.Generic;

namespace Misbah.Application.Queries.Notes
{
    /// <summary>
    /// Query to get all notes
    /// </summary>
    public record GetAllNotesQuery : IQuery<IEnumerable<Note>>
    {
        public bool IncludeContent { get; }
        public string? TagFilter { get; }
        
        public GetAllNotesQuery(bool includeContent = false, string? tagFilter = null)
        {
            IncludeContent = includeContent;
            TagFilter = tagFilter;
        }
    }
    
    /// <summary>
    /// Query to get a specific note by ID
    /// </summary>
    public record GetNoteByIdQuery : IQuery<Note?>
    {
        public string NoteId { get; }
        
        public GetNoteByIdQuery(string noteId)
        {
            NoteId = noteId;
        }
    }
    
    /// <summary>
    /// Query to get a note by file path
    /// </summary>
    public record GetNoteByFilePathQuery : IQuery<Note?>
    {
        public string FilePath { get; }
        
        public GetNoteByFilePathQuery(string filePath)
        {
            FilePath = filePath;
        }
    }
    
    /// <summary>
    /// Query to search notes by content
    /// </summary>
    public record SearchNotesQuery : IQuery<IEnumerable<Note>>
    {
        public string SearchTerm { get; }
        public bool SearchInContent { get; }
        public bool SearchInTitle { get; }
        
        public SearchNotesQuery(string searchTerm, bool searchInContent = true, bool searchInTitle = true)
        {
            SearchTerm = searchTerm;
            SearchInContent = searchInContent;
            SearchInTitle = searchInTitle;
        }
    }
    
    /// <summary>
    /// Query to get notes by tag
    /// </summary>
    public record GetNotesByTagQuery : IQuery<IEnumerable<Note>>
    {
        public string Tag { get; }
        
        public GetNotesByTagQuery(string tag)
        {
            Tag = tag;
        }
    }
    
    /// <summary>
    /// Query to get all unique tags
    /// </summary>
    public record GetAllTagsQuery : IQuery<IEnumerable<string>>
    {
    }
}
