using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
// using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class ConfigManagerTests
{
    [Fact]
    public void ConfigManager_Can_Be_Created()
    {
        var config = new Misbah.BlazorDesktop.Utils.ConfigManager();
        Assert.NotNull(config);
    }
}
