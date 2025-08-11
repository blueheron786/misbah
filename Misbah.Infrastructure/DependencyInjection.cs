using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Persistence.Repositories;

namespace Misbah.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string basePath = null)
        {
            // Register repositories
            services.AddScoped<INoteRepository>(_ => new NoteRepository(
                basePath != null ? Path.Combine(basePath, "Notes") : null));
                
            services.AddScoped<IFolderRepository>(_ => new FolderRepository(
                basePath != null ? Path.Combine(basePath, "Folders") : null));

            // Register AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IFolderService, FolderService>();

            return services;
        }
    }
}
