using System.Collections.Generic;
using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class NoteViewTests : TestContext
{
    [Fact]
    public void Renders_Note_And_Editor()
    {
        // Arrange
    var note = new NoteDto { Id = "n1", Title = "Test Note", Content = "# Hello" };
    var noteService = Substitute.For<INoteService>();
    noteService.GetNoteByIdAsync("n1").Returns(System.Threading.Tasks.Task.FromResult(note));
    Services.AddSingleton(noteService);
    Services.AddSingleton(new Misbah.Application.Services.MarkdownRenderer());
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(Substitute.For<Microsoft.JSInterop.IJSRuntime>());

        // Act
        var cut = RenderComponent<NoteView>(parameters => parameters.Add(p => p.NoteId, "n1"));

        // Assert
        Assert.Contains("Test Note", cut.Markup);
        Assert.Contains("wysiwyg-markdown-editor", cut.Markup);
    }
}
