using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class NoteListTests : TestContext
{
    [Fact]
    public void Renders_Notes_And_Selects_Note()
    {
        // Arrange
        var notes = new List<NoteDto>
        {
            new NoteDto { Id = "1", Title = "Note 1", Content = "A" },
            new NoteDto { Id = "2", Title = "Note 2", Content = "B" }
        };
        var noteService = Substitute.For<INoteService>();
        noteService.GetAllNotesAsync().Returns(System.Threading.Tasks.Task.FromResult((IEnumerable<NoteDto>)notes));
        var folderService = Substitute.For<IFolderService>();
        folderService.GetFolderByPathAsync(Arg.Any<string>()).Returns(System.Threading.Tasks.Task.FromResult(new FolderNodeDto { Name = "root", Path = "root" }));
        Services.AddSingleton(noteService);
        Services.AddSingleton(folderService);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());

        // Act - test NoteList without RootPath parameter to avoid FolderTree issues
        var cut = RenderComponent<NoteList>();

        // Assert
        Assert.Contains("Note 1", cut.Markup);
        Assert.Contains("Note 2", cut.Markup);
        // Simulate click
        cut.FindAll("li").First().Click();
        Assert.Contains("selected", cut.FindAll("li").First().GetAttribute("class"));
    }
}

// Minimal NavigationManager for test
public class TestNavigationManager : NavigationManager
{
    public TestNavigationManager() { Initialize("http://localhost/", "http://localhost/"); }
    protected override void NavigateToCore(string uri, bool forceLoad) { }
}
