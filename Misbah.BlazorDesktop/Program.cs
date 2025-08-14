using System.Windows;
using Misbah.BlazorDesktop.Services;

namespace Misbah.BlazorDesktop
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();

            // Allow access to the underlying dev tools console.
            // Enables CTRL+SHIFT+I and viewing HTML etc.
            services.AddBlazorWebViewDeveloperTools();

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
