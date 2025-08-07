using Microsoft.AspNetCore.Components.WebView.Wpf;
using System.Windows;

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
            }
        }
    }
}
