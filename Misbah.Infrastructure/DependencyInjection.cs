using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Persistence.Repositories;
using Misbah.Infrastructure.Services;

namespace Misbah.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string basePath = null)
        {
            // Register hub path provider
            services.AddScoped<IHubPathProvider, HubPathProvider>();
            
            // Register repositories - they'll use the hub path provider
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IFolderRepository, FolderRepository>();

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
