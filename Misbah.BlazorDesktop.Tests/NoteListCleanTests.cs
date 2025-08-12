using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Xunit;
using NSubstitute;
using Microsoft.AspNetCore.Components;
using Misbah.Application.Interfaces;
using Misbah.Domain.Entities;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Microsoft.Extensions.Localization;
using Misbah.BlazorDesktop.Resources;

namespace Misbah.BlazorDesktop.Tests;

/// <summary>
/// Tests for the NoteListClean component using Clean Architecture
/// </summary>
public class NoteListCleanTests : TestContext
{
    [Fact]
    public void NoteListClean_RendersCorrectly_WithEmptyNotes()
    {
        // Arrange
        var mockNoteService = Substitute.For<INoteApplicationService>();
        var mockFolderService = Substitute.For<IFolderApplicationService>();
        var mockNavigation = Substitute.For<NavigationManager>();
        var mockLocalizer = Substitute.For<IStringLocalizer<AppStrings>>();
        
        mockNoteService.GetAllNotesAsync().Returns(Task.FromResult(Enumerable.Empty<Note>()));
        mockFolderService.GetRootPath().Returns("/test/path");
        mockLocalizer["NoNotesFound"].Returns(new LocalizedString("NoNotesFound", "No notes found"));
        mockLocalizer["Notes"].Returns(new LocalizedString("Notes", "Notes"));
        mockLocalizer["HideFolders"].Returns(new LocalizedString("HideFolders", "Hide Folders"));
        mockLocalizer["ShowFolders"].Returns(new LocalizedString("ShowFolders", "Show Folders"));
        
        Services.AddSingleton(mockNoteService);
        Services.AddSingleton(mockFolderService);
        Services.AddSingleton(mockNavigation);
        Services.AddSingleton(mockLocalizer);

        // Act
        var component = RenderComponent<NoteListClean>(parameters => parameters
            .Add(p => p.RootPath, "/test/path"));

        // Assert
        Assert.Contains("note-list-container", component.Markup);
        Assert.Contains("No notes found", component.Markup);
    }

    [Fact]
    public void NoteListClean_RendersNotes_WhenNotesExist()
    {
        // Arrange
        var mockNoteService = Substitute.For<INoteApplicationService>();
        var mockFolderService = Substitute.For<IFolderApplicationService>();
        var mockNavigation = Substitute.For<NavigationManager>();
        var mockLocalizer = Substitute.For<IStringLocalizer<AppStrings>>();
        
        var testNotes = new[]
        {
            new Note 
            { 
                Id = "note1", 
                Title = "Test Note 1", 
                Content = "Content 1",
                Tags = new List<string> { "tag1", "tag2" },
                Modified = DateTime.Now
            },
            new Note 
            { 
                Id = "note2", 
                Title = "Test Note 2", 
                Content = "Content 2",
                Tags = new List<string>(),
                Modified = DateTime.Now.AddDays(-1)
            }
        };
        
        mockNoteService.GetAllNotesAsync().Returns(Task.FromResult(testNotes.AsEnumerable()));
        mockFolderService.GetRootPath().Returns("/test/path");
        mockLocalizer["Notes"].Returns(new LocalizedString("Notes", "Notes"));
        mockLocalizer["HideFolders"].Returns(new LocalizedString("HideFolders", "Hide Folders"));
        mockLocalizer["ShowFolders"].Returns(new LocalizedString("ShowFolders", "Show Folders"));
        
        Services.AddSingleton(mockNoteService);
        Services.AddSingleton(mockFolderService);
        Services.AddSingleton(mockNavigation);
        Services.AddSingleton(mockLocalizer);

        // Act
        var component = RenderComponent<NoteListClean>(parameters => parameters
            .Add(p => p.RootPath, "/test/path"));

        // Assert
        Assert.Contains("note-list-container", component.Markup);
        Assert.Contains("Test Note 1", component.Markup);
        Assert.Contains("Test Note 2", component.Markup);
        Assert.Contains("tag1", component.Markup);
        Assert.Contains("tag2", component.Markup);
    }

    [Fact]
    public void NoteListClean_CallsNoteService_OnInitialization()
    {
        // Arrange
        var mockNoteService = Substitute.For<INoteApplicationService>();
        var mockFolderService = Substitute.For<IFolderApplicationService>();
        var mockNavigation = Substitute.For<NavigationManager>();
        var mockLocalizer = Substitute.For<IStringLocalizer<AppStrings>>();
        
        mockNoteService.GetAllNotesAsync().Returns(Task.FromResult(Enumerable.Empty<Note>()));
        mockFolderService.GetRootPath().Returns("/test/path");
        mockLocalizer["Notes"].Returns(new LocalizedString("Notes", "Notes"));
        mockLocalizer["HideFolders"].Returns(new LocalizedString("HideFolders", "Hide Folders"));
        mockLocalizer["ShowFolders"].Returns(new LocalizedString("ShowFolders", "Show Folders"));
        
        Services.AddSingleton(mockNoteService);
        Services.AddSingleton(mockFolderService);
        Services.AddSingleton(mockNavigation);
        Services.AddSingleton(mockLocalizer);

        // Act
        var component = RenderComponent<NoteListClean>(parameters => parameters
            .Add(p => p.RootPath, "/test/path"));

        // Assert
        mockNoteService.Received(1).SetRootPath("/test/path");
        mockFolderService.Received(1).SetRootPath("/test/path");
        mockNoteService.Received(1).GetAllNotesAsync();
    }
}
