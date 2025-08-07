using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Misbah.BlazorDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public IServiceProvider Services
        {
            set { 
                blazorWebView.Services = value;
                blazorWebView.RootComponents.Add(new RootComponent
                {
                    Selector = "#app",
                    ComponentType = typeof(Components.Routes)
                });
            }
        }
    }
}
