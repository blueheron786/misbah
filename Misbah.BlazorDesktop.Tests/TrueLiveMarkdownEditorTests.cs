using System.Collections.Generic;
using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
using Misbah.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class TrueLiveMarkdownEditorTests : TestContext
{
    [Fact]
    public void Renders_Empty_Placeholder()
    {
        // Arrange
        Services.AddSingleton(new MarkdownRenderer());
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(Substitute.For<Microsoft.JSInterop.IJSRuntime>());
        // Act
        var cut = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters.Add(p => p.Content, ""));
        // Assert
        Assert.Contains("Click here to start writing", cut.Markup);
    }

    [Fact]
    public void Renders_Markdown_Blocks()
    {
        // Arrange
        Services.AddSingleton(new MarkdownRenderer());
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(Substitute.For<Microsoft.JSInterop.IJSRuntime>());
        // Act
        var cut = RenderComponent<TrueLiveMarkdownEditor>(parameters => parameters.Add(p => p.Content, "# Heading\n\nSome text"));
        // Assert
        Assert.Contains("Heading", cut.Markup);
        Assert.Contains("Some text", cut.Markup);
    }
}
