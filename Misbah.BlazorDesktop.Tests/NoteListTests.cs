using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class NoteListTests : TestContext
{
    [Fact]
    public void Renders_Notes_And_Selects_Note()
    {
        // Arrange
        var notes = new List<Note>
        {
            new Note { Id = "1", Title = "Note 1", Content = "A" },
            new Note { Id = "2", Title = "Note 2", Content = "B" }
        };
        var noteService = Substitute.For<INoteService>();
        noteService.GetAllNotes().Returns(notes);
        var folderService = Substitute.For<IFolderService>();
        folderService.LoadFolderTree(Arg.Any<string>()).Returns(new FolderNode { Name = "root", Path = "root" });
        Services.AddSingleton(noteService);
        Services.AddSingleton(folderService);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());

        // Act
        var cut = RenderComponent<NoteList>(parameters => parameters.Add(p => p.RootPath, "root"));

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
