using Microsoft.AspNetCore.Components.WebView.Wpf;
using System.Windows;
using Misbah.BlazorDesktop.Components;

namespace Misbah.BlazorDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        public IServiceProvider Services
        {
            set 
            { 
                blazorWebView.Services = value;
                blazorWebView.RootComponents.Add(new RootComponent
                {
                    Selector = "#app",
                    ComponentType = typeof(Misbah.BlazorDesktop.Components.Routes)
                });
            }
        }
    }
}
