using Misbah.BlazorDesktop.Components;
using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class LangSelectorTests : TestContext
{
    [Fact]
    public void LangSelector_Can_Be_Rendered()
    {
        var cut = RenderComponent<LangSelector>();
        Assert.NotNull(cut);
    }
}
