using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Misbah.Application.Interfaces;
using Misbah.BlazorDesktop.Components;
using Misbah.Infrastructure;
using System;
using System.IO;
using System.Windows;

namespace Misbah.BlazorDesktop
{
    public partial class App : System.Windows.Application
    {
        private IHost _host;
        private static string AppDataPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Misbah");

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure application data directory exists
            Directory.CreateDirectory(AppDataPath);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Add WPF Blazor
                    services.AddWpfBlazorWebView();
                    services.AddLocalization(options => options.ResourcesPath = "Resources");

                    // Add infrastructure services
                    services.AddInfrastructure(AppDataPath);
                    
                    // Add application services
                    services.AddApplicationServices();

                    // Register main window
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            using var scope = _host.Services.CreateScope();
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
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
