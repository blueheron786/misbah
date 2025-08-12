using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Misbah.BlazorDesktop.Components;
using Misbah.Core.Services;
using Misbah.Core.Utils;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Repositories;
using System.Windows;

namespace Misbah.BlazorDesktop
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Register Core services (legacy - for backward compatibility)
            services.AddSingleton<INoteService>(sp => new NoteService("Notes")); // TODO: set actual notes root path
            services.AddSingleton<IFolderService, FolderService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<MarkdownRenderer>();

            // Register Clean Architecture services (new)
            services.AddScoped<INoteRepository, NoteRepositoryAdapter>();
            services.AddScoped<INoteApplicationService, NoteApplicationService>();
            
            // Register Folder Clean Architecture services
            services.AddScoped<IFolderRepository, FolderRepositoryAdapter>();
            services.AddScoped<IFolderApplicationService, FolderApplicationService>();

            var serviceProvider = services.BuildServiceProvider();

            var mainWindow = new MainWindow();
            mainWindow.Services = serviceProvider;
            mainWindow.Show();
        }
    }

    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.Run();
        }
    }
}
