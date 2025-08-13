using Microsoft.Extensions.Logging;
using Misbah.Domain.Events;
using Misbah.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Misbah.Application.EventHandlers
{
    /// <summary>
    /// Handler for note created events - could be used for logging, notifications, etc.
    /// </summary>
    public class NoteCreatedEventHandler : IDomainEventHandler<NoteCreated>
    {
        private readonly ILogger<NoteCreatedEventHandler> _logger;
        
        public NoteCreatedEventHandler(ILogger<NoteCreatedEventHandler> logger)
        {
            _logger = logger;
        }
        
        public async Task HandleAsync(NoteCreated domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Note created: {Title} (ID: {NoteId}) at {FilePath}", 
                domainEvent.Title, domainEvent.NoteId, domainEvent.FilePath);
                
            // Here you could add additional logic like:
            // - Send notifications
            // - Update search indexes
            // - Trigger background processes
            // - Update statistics
            
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Handler for note updated events
    /// </summary>
    public class NoteUpdatedEventHandler : IDomainEventHandler<NoteUpdated>
    {
        private readonly ILogger<NoteUpdatedEventHandler> _logger;
        
        public NoteUpdatedEventHandler(ILogger<NoteUpdatedEventHandler> logger)
        {
            _logger = logger;
        }
        
        public async Task HandleAsync(NoteUpdated domainEvent, CancellationToken cancellationToken = default)
        {
            if (domainEvent.PreviousTitle != domainEvent.NewTitle)
            {
                _logger.LogInformation("Note title changed: '{PreviousTitle}' -> '{NewTitle}' (ID: {NoteId})",
                    domainEvent.PreviousTitle, domainEvent.NewTitle, domainEvent.NoteId);
            }
            else
            {
                _logger.LogDebug("Note updated: {NoteId}", domainEvent.NoteId);
            }
            
            // Additional logic for updates:
            // - Update search indexes
            // - Invalidate caches
            // - Update linked notes
            // - Trigger auto-backup
            
            await Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Handler for note deleted events
    /// </summary>
    public class NoteDeletedEventHandler : IDomainEventHandler<NoteDeleted>
    {
        private readonly ILogger<NoteDeletedEventHandler> _logger;
        
        public NoteDeletedEventHandler(ILogger<NoteDeletedEventHandler> logger)
        {
            _logger = logger;
        }
        
        public async Task HandleAsync(NoteDeleted domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Note deleted: {Title} (ID: {NoteId}) from {FilePath}",
                domainEvent.Title, domainEvent.NoteId, domainEvent.FilePath);
                
            // Additional logic for deletions:
            // - Clean up references
            // - Archive content
            // - Update statistics
            // - Remove from search indexes
            
            await Task.CompletedTask;
        }
    }
}
