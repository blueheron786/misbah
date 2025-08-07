using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Misbah.BlazorDesktop.Components;
using Misbah.Core.Services;
using Misbah.Core.Utils;
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

            // Register services for DI
            services.AddSingleton<INoteService>(sp => new NoteService("Notes")); // TODO: set actual notes root path
            services.AddSingleton<IFolderService, FolderService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<MarkdownRenderer>();

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
