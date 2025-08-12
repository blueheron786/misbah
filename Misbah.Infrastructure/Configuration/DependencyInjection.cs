using Microsoft.Extensions.DependencyInjection;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Core.Services;
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
            services.AddSingleton<INoteService>(sp => new NoteService(notesRootPath));
            services.AddSingleton<IFolderService, FolderService>();
            
            // New Clean Architecture services
            services.AddSingleton<INoteRepository, NoteRepositoryAdapter>();
            services.AddSingleton<INoteAppService, NoteAppService>();
            
            return services;
        }
    }
}
