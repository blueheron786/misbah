using Misbah.Application.Common;
using Misbah.Application.Queries.Notes;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Handlers.Queries
{
    public class GetAllNotesQueryHandler : IQueryHandler<GetAllNotesQuery, IEnumerable<Note>>
    {
        private readonly INoteRepository _noteRepository;
        
        public GetAllNotesQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }
        
        public async Task<IEnumerable<Note>> HandleAsync(GetAllNotesQuery query, CancellationToken cancellationToken = default)
        {
            var notes = await Task.FromResult(_noteRepository.GetAllNotes());
            
            if (!string.IsNullOrEmpty(query.TagFilter))
            {
                notes = notes.Where(n => n.ExtractedTags.Contains(query.TagFilter, StringComparer.OrdinalIgnoreCase));
            }
            
            return notes;
        }
    }
    
    public class GetNoteByIdQueryHandler : IQueryHandler<GetNoteByIdQuery, Note?>
    {
        private readonly INoteRepository _noteRepository;
        
        public GetNoteByIdQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }
        
        public async Task<Note?> HandleAsync(GetNoteByIdQuery query, CancellationToken cancellationToken = default)
        {
            return await _noteRepository.GetNoteByIdAsync(query.NoteId);
        }
    }
    
    public class GetNoteByFilePathQueryHandler : IQueryHandler<GetNoteByFilePathQuery, Note?>
    {
        private readonly INoteRepository _noteRepository;
        
        public GetNoteByFilePathQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }
        
        public async Task<Note?> HandleAsync(GetNoteByFilePathQuery query, CancellationToken cancellationToken = default)
        {
            return await _noteRepository.GetNoteAsync(query.FilePath);
        }
    }
    
    public class SearchNotesQueryHandler : IQueryHandler<SearchNotesQuery, IEnumerable<Note>>
    {
        private readonly INoteRepository _noteRepository;
        
        public SearchNotesQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }
        
        public async Task<IEnumerable<Note>> HandleAsync(SearchNotesQuery query, CancellationToken cancellationToken = default)
        {
            var allNotes = await Task.FromResult(_noteRepository.GetAllNotes());
            var searchTerm = query.SearchTerm.ToLowerInvariant();
            
            return allNotes.Where(note =>
            {
                var matchesTitle = query.SearchInTitle && note.Title.ToLowerInvariant().Contains(searchTerm);
                var matchesContent = query.SearchInContent && note.Content.RawContent.ToLowerInvariant().Contains(searchTerm);
                
                return matchesTitle || matchesContent;
            });
        }
    }
    
    public class GetNotesByTagQueryHandler : IQueryHandler<GetNotesByTagQuery, IEnumerable<Note>>
    {
        private readonly INoteRepository _noteRepository;
        
        public GetNotesByTagQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }
        
        public async Task<IEnumerable<Note>> HandleAsync(GetNotesByTagQuery query, CancellationToken cancellationToken = default)
        {
            var allNotes = await Task.FromResult(_noteRepository.GetAllNotes());
            
            return allNotes.Where(note => note.ExtractedTags.Contains(query.Tag, StringComparer.OrdinalIgnoreCase));
        }
    }
    
    public class GetAllTagsQueryHandler : IQueryHandler<GetAllTagsQuery, IEnumerable<string>>
    {
        private readonly INoteRepository _noteRepository;
        
        public GetAllTagsQueryHandler(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }
        
        public async Task<IEnumerable<string>> HandleAsync(GetAllTagsQuery query, CancellationToken cancellationToken = default)
        {
            var allNotes = await Task.FromResult(_noteRepository.GetAllNotes());
            
            return allNotes
                .SelectMany(note => note.ExtractedTags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag);
        }
    }
}
