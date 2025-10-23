using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Misbah.Application.Common;
using Misbah.Application.EventHandlers;
using Misbah.Application.Handlers.Commands;
using Misbah.Application.Handlers.Queries;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Core.Services;
using Misbah.Domain.Events;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Repositories;

namespace Misbah.Infrastructure.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCleanArchitectureServices(
            this IServiceCollection services,
            string notesRootPath)
        {
            // Legacy services (keep these for backwards compatibility)
            services.AddSingleton<INoteService>(sp => 
            {
                var gitSyncService = sp.GetRequiredService<IGitSyncService>();
                return new NoteService(notesRootPath, gitSyncService);
            });
            services.AddSingleton<IFolderService, FolderService>();

            // New Clean Architecture services
            services.AddSingleton<INoteRepository, NoteRepositoryAdapter>();
            services.AddSingleton<INoteApplicationService, NoteApplicationService>();
            
            return services;
        }
        
        /// <summary>
        /// Add the complete Clean Architecture stack with CQRS, Domain Events, and Mediator
        /// </summary>
        public static IServiceCollection AddAdvancedCleanArchitecture(
            this IServiceCollection services,
            string notesRootPath)
        {
            // Legacy services (backwards compatibility)
            services.AddSingleton<INoteService>(sp => 
            {
                var logger = sp.GetRequiredService<ILogger<NoteService>>();
                var gitSyncService = sp.GetRequiredService<IGitSyncService>();
                return new NoteService(logger, gitSyncService);
            });
            services.AddSingleton<IFolderService, FolderService>();

            // Domain layer
            services.AddSingleton<INoteRepository, NoteRepositoryAdapter>();
            
            // Application layer - CQRS Handlers
            services.AddScoped<IMediator, SimpleMediator>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            
            // Command Handlers
            services.AddScoped<CreateNoteCommandHandler>();
            services.AddScoped<UpdateNoteContentCommandHandler>();
            services.AddScoped<UpdateNoteTitleCommandHandler>();
            services.AddScoped<DeleteNoteCommandHandler>();
            services.AddScoped<SaveNoteCommandHandler>();
            
            // Query Handlers
            services.AddScoped<GetAllNotesQueryHandler>();
            services.AddScoped<GetNoteByIdQueryHandler>();
            services.AddScoped<GetNoteByFilePathQueryHandler>();
            services.AddScoped<SearchNotesQueryHandler>();
            services.AddScoped<GetNotesByTagQueryHandler>();
            services.AddScoped<GetAllTagsQueryHandler>();
            
            // Domain Event Handlers
            services.AddScoped<IDomainEventHandler<NoteCreated>, NoteCreatedEventHandler>();
            services.AddScoped<IDomainEventHandler<NoteUpdated>, NoteUpdatedEventHandler>();
            services.AddScoped<IDomainEventHandler<NoteDeleted>, NoteDeletedEventHandler>();
            
            // Legacy Application Services (keep for compatibility)
            services.AddSingleton<INoteApplicationService, NoteApplicationService>();
            
            return services;
        }
    }
}