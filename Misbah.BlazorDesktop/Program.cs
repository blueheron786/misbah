using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Misbah.BlazorDesktop.Components;
using Misbah.Core.Services;
using Misbah.Core.Utils;
using Misbah.Application.Interfaces;
using Misbah.Application.Services;
using Misbah.Domain.Interfaces;
using Misbah.Infrastructure.Repositories;
using Misbah.Infrastructure.Configuration;
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
            services.AddLogging(builder => builder.AddConsole().AddDebug());

            // Legacy services (maintained for backward compatibility)
            services.AddSingleton<SearchService>();
            services.AddSingleton<MarkdownRenderer>();

            // Advanced Clean Architecture with CQRS and Domain Events
            services.AddAdvancedCleanArchitecture("Notes");
            
            // Keep legacy clean architecture services for existing components
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
