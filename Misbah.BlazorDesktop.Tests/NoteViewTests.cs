using System.Collections.Generic;
using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class NoteViewTests : TestContext
{
    [Fact]
    public void Renders_Note_And_Editor()
    {
        // Arrange
        var note = new Note { Id = "n1", Title = "Test Note", Content = "# Hello" };
        var noteService = Substitute.For<INoteService>();
        noteService.LoadNote("n1").Returns(note);
        Services.AddSingleton(noteService);
        Services.AddSingleton(new Misbah.Core.Services.MarkdownRenderer());
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(Substitute.For<Microsoft.JSInterop.IJSRuntime>());

        // Act
        var cut = RenderComponent<NoteView>(parameters => parameters.Add(p => p.NoteId, "n1"));

        // Assert
        Assert.Contains("Test Note", cut.Markup);
        Assert.Contains("WysiwygMarkdownEditor", cut.Markup);
    }
}
