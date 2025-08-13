using Misbah.Web.Components;
using Misbah.Core.Services;
using Misbah.Core.Utils;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Repositories;
using Misbah.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Misbah.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Configure shared services
            ConfigureServices(builder.Services);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            // Add localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Legacy services (maintained for backward compatibility)
            services.AddSingleton<SearchService>();
            services.AddSingleton<MarkdownRenderer>();

            // Advanced Clean Architecture with CQRS and Domain Events
            services.AddAdvancedCleanArchitecture("Notes");

            // Keep legacy clean architecture services for existing components
            services.AddScoped<IFolderRepository, FolderRepositoryAdapter>();
            services.AddScoped<IFolderApplicationService, FolderApplicationService>();
        }
    }
}
