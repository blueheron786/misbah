using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
// using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class SetCultureTests
{
    [Fact]
    public void ApplyFromJs_Does_Not_Throw()
    {
        var js = Substitute.For<Microsoft.JSInterop.IJSRuntime>();
        SetCulture.ApplyFromJs(js);
        // No exception means pass
        Assert.True(true);
    }
}
