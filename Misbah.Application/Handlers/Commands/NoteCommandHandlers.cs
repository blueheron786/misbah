using Misbah.Application.Commands.Notes;
using Misbah.Application.Common;
using Misbah.Domain.Entities;
using Misbah.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.Handlers.Commands
{
    public class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand, Note>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IDomainEventDispatcher? _eventDispatcher;
        
        public CreateNoteCommandHandler(INoteRepository noteRepository, IDomainEventDispatcher? eventDispatcher = null)
        {
            _noteRepository = noteRepository;
            _eventDispatcher = eventDispatcher;
        }
        
        public async Task<Note> HandleAsync(CreateNoteCommand command, CancellationToken cancellationToken = default)
        {
            var note = Note.CreateNew(command.Title, command.FolderPath, command.Content);
            
            await _noteRepository.SaveNoteAsync(note);
            
            // Dispatch domain events
            if (_eventDispatcher != null)
            {
                await _eventDispatcher.DispatchAsync(note.DomainEvents, cancellationToken);
                note.ClearDomainEvents();
            }
            
            return note;
        }
    }
    
    public class UpdateNoteContentCommandHandler : ICommandHandler<UpdateNoteContentCommand>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IDomainEventDispatcher? _eventDispatcher;
        
        public UpdateNoteContentCommandHandler(INoteRepository noteRepository, IDomainEventDispatcher? eventDispatcher = null)
        {
            _noteRepository = noteRepository;
            _eventDispatcher = eventDispatcher;
        }
        
        public async Task HandleAsync(UpdateNoteContentCommand command, CancellationToken cancellationToken = default)
        {
            var note = await _noteRepository.GetNoteByIdAsync(command.NoteId);
            if (note == null)
                throw new InvalidOperationException($"Note with ID {command.NoteId} not found");
                
            note.UpdateContent(command.Content);
            
            await _noteRepository.SaveNoteAsync(note);
            
            // Dispatch domain events
            if (_eventDispatcher != null)
            {
                await _eventDispatcher.DispatchAsync(note.DomainEvents, cancellationToken);
                note.ClearDomainEvents();
            }
        }
    }
    
    public class UpdateNoteTitleCommandHandler : ICommandHandler<UpdateNoteTitleCommand>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IDomainEventDispatcher? _eventDispatcher;
        
        public UpdateNoteTitleCommandHandler(INoteRepository noteRepository, IDomainEventDispatcher? eventDispatcher = null)
        {
            _noteRepository = noteRepository;
            _eventDispatcher = eventDispatcher;
        }
        
        public async Task HandleAsync(UpdateNoteTitleCommand command, CancellationToken cancellationToken = default)
        {
            var note = await _noteRepository.GetNoteByIdAsync(command.NoteId);
            if (note == null)
                throw new InvalidOperationException($"Note with ID {command.NoteId} not found");
                
            note.UpdateTitle(command.Title);
            
            await _noteRepository.SaveNoteAsync(note);
            
            // Dispatch domain events
            if (_eventDispatcher != null)
            {
                await _eventDispatcher.DispatchAsync(note.DomainEvents, cancellationToken);
                note.ClearDomainEvents();
            }
        }
    }
    
    public class DeleteNoteCommandHandler : ICommandHandler<DeleteNoteCommand>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IDomainEventDispatcher? _eventDispatcher;
        
        public DeleteNoteCommandHandler(INoteRepository noteRepository, IDomainEventDispatcher? eventDispatcher = null)
        {
            _noteRepository = noteRepository;
            _eventDispatcher = eventDispatcher;
        }
        
        public async Task HandleAsync(DeleteNoteCommand command, CancellationToken cancellationToken = default)
        {
            var note = await _noteRepository.GetNoteByIdAsync(command.NoteId);
            if (note == null)
                throw new InvalidOperationException($"Note with ID {command.NoteId} not found");
                
            note.MarkAsDeleted();
            
            // Dispatch domain events before deletion
            if (_eventDispatcher != null)
            {
                await _eventDispatcher.DispatchAsync(note.DomainEvents, cancellationToken);
            }
            
            await _noteRepository.DeleteNoteAsync(command.NoteId);
        }
    }
    
    public class SaveNoteCommandHandler : ICommandHandler<SaveNoteCommand>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IDomainEventDispatcher? _eventDispatcher;
        
        public SaveNoteCommandHandler(INoteRepository noteRepository, IDomainEventDispatcher? eventDispatcher = null)
        {
            _noteRepository = noteRepository;
            _eventDispatcher = eventDispatcher;
        }
        
        public async Task HandleAsync(SaveNoteCommand command, CancellationToken cancellationToken = default)
        {
            await _noteRepository.SaveNoteAsync(command.Note);
            
            // Dispatch domain events
            if (_eventDispatcher != null)
            {
                await _eventDispatcher.DispatchAsync(command.Note.DomainEvents, cancellationToken);
                command.Note.ClearDomainEvents();
            }
        }
    }
}
