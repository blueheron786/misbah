using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
// using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class MainWindowTests
{
    [Fact]
    public void MainWindow_Can_Be_Created()
    {
        var window = new Misbah.BlazorDesktop.MainWindow();
        Assert.NotNull(window);
    }
}
