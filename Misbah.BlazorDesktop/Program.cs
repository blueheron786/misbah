using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Misbah.Web.Components;

namespace Misbah.BlazorDesktop
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
            
            // Import all services from the Web project
            Misbah.Web.Program.ConfigureServices(services);

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
