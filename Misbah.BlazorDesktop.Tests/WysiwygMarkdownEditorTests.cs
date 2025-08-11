using System.Collections.Generic;
using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class WysiwygMarkdownEditorTests : TestContext
{
    [Fact]
    public void Renders_Placeholder_When_Empty()
    {
        // Arrange
        Services.AddSingleton(new MarkdownRenderer());
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(Substitute.For<Microsoft.JSInterop.IJSRuntime>());
        // Act
        var cut = RenderComponent<WysiwygMarkdownEditor>(parameters => parameters.Add(p => p.Content, ""));
        // Assert
        Assert.Contains("Click here to start writing", cut.Markup);
    }

    [Fact]
    public void Renders_Markdown_Content()
    {
        // Arrange
        Services.AddSingleton(new MarkdownRenderer());
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(Substitute.For<Microsoft.JSInterop.IJSRuntime>());
        // Act
        var cut = RenderComponent<WysiwygMarkdownEditor>(parameters => parameters.Add(p => p.Content, "# Title"));
        // Assert
        Assert.Contains("Title", cut.Markup);
    }
}
