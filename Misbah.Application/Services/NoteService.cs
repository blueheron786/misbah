using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;

namespace Misbah.Application.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public NoteService(INoteRepository noteRepository, IMapper mapper)
        {
            _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<NoteDto> GetNoteByIdAsync(string id)
        {
            var note = await _noteRepository.GetByIdAsync(id);
            return _mapper.Map<NoteDto>(note);
        }

        public async Task<IEnumerable<NoteDto>> GetAllNotesAsync()
        {
            var notes = await _noteRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<NoteDto>>(notes);
        }

        public async Task<NoteDto> CreateNoteAsync(NoteDto noteDto)
        {
            var note = _mapper.Map<Note>(noteDto);
            note.Created = DateTime.UtcNow;
            note.Modified = DateTime.UtcNow;
            
            var createdNote = await _noteRepository.AddAsync(note);
            return _mapper.Map<NoteDto>(createdNote);
        }

        public async Task UpdateNoteAsync(NoteDto noteDto)
        {
            var existingNote = await _noteRepository.GetByIdAsync(noteDto.Id);
            if (existingNote == null)
            {
                throw new KeyNotFoundException($"Note with id {noteDto.Id} not found.");
            }

            var note = _mapper.Map<Note>(noteDto);
            note.Modified = DateTime.UtcNow;
            
            await _noteRepository.UpdateAsync(note);
        }

        public async Task DeleteNoteAsync(string id)
        {
            await _noteRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<NoteDto>> SearchNotesAsync(string searchTerm)
        {
            var notes = await _noteRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<NoteDto>>(
                notes.Where(n => 
                    n.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    n.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
