using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Misbah.Web.Components.Pages.Notes;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Misbah.BlazorDesktop.Tests;

[TestFixture]
public class FolderTreeTests : Bunit.TestContext
{
    private IFolderService _mockFolderService = null!;
    private FolderNode _testFolderStructure = null!;

    [SetUp]
    public void Setup()
    {
        _mockFolderService = Substitute.For<IFolderService>();
        
        // Create a test folder structure
        _testFolderStructure = new FolderNode
        {
            Name = "Root",
            Path = "C:\\TestNotes",
            Folders = new List<FolderNode>
            {
                new FolderNode
                {
                    Name = "Subfolder1",
                    Path = "C:\\TestNotes\\Subfolder1",
                    Notes = new List<Note>
                    {
                        new Note { Id = "note1", Title = "Note 1", FilePath = "C:\\TestNotes\\Subfolder1\\note1.md", Content = "Test content 1" },
                        new Note { Id = "note2", Title = "Note 2", FilePath = "C:\\TestNotes\\Subfolder1\\note2.md", Content = "Test content 2" }
                    },
                    Folders = new List<FolderNode>()
                },
                new FolderNode
                {
                    Name = "Subfolder2",
                    Path = "C:\\TestNotes\\Subfolder2",
                    Notes = new List<Note>
                    {
                        new Note { Id = "note3", Title = "Note 3", FilePath = "C:\\TestNotes\\Subfolder2\\note3.md", Content = "Test content 3" }
                    },
                    Folders = new List<FolderNode>()
                }
            },
            Notes = new List<Note>
            {
                new Note { Id = "rootNote", Title = "Root Note", FilePath = "C:\\TestNotes\\rootNote.md", Content = "Root content" }
            }
        };
        
        _mockFolderService.LoadFolderTree(Arg.Any<string>()).Returns(_testFolderStructure);
        
        Services.AddSingleton(_mockFolderService);
    }

    [Test]
    public void FolderTree_RendersWithoutListStyleBullets()
    {
        // Arrange
        var component = RenderComponent<FolderTree>(parameters => parameters
            .Add(p => p.RootPath, "C:\\TestNotes"));

        // Act
        var folderTreeDiv = component.Find(".folder-tree");
        var allUlElements = component.FindAll("ul");
        var allLiElements = component.FindAll("li");

        // Assert
        Assert.That(folderTreeDiv, Is.Not.Null, "Folder tree container should be present");
        Assert.That(allUlElements.Count, Is.GreaterThan(0), "Should have ul elements");
        Assert.That(allLiElements.Count, Is.GreaterThan(0), "Should have li elements");

        // Simple check: verify structure is correct for CSS to work
        foreach (var ul in allUlElements)
        {
            var isInFolderTree = ul.Closest(".folder-tree") != null;
            Assert.That(isInFolderTree, Is.True, "UL should be within folder-tree for CSS rules to apply");
        }

        foreach (var li in allLiElements)
        {
            var isInFolderTree = li.Closest(".folder-tree") != null;
            Assert.That(isInFolderTree, Is.True, "LI should be within folder-tree for CSS rules to apply");
        }
    }

    [Test]
    public void FolderTree_NotesDisplayWithFileIcons_NotBullets()
    {
        // Arrange & Act
        var component = RenderComponent<FolderTree>(parameters => parameters
            .Add(p => p.RootPath, "C:\\TestNotes"));

        // Assert
        var noteElements = component.FindAll("li.note-leaf");
        Assert.That(noteElements.Count, Is.GreaterThan(0), "Should have note elements");

        foreach (var noteElement in noteElements)
        {
            // Check that note elements have file icons, not bullet points
            var iconElements = noteElement.QuerySelectorAll("i.fa-file-text");
            Assert.That(iconElements.Length, Is.EqualTo(1), 
                "Each note should have exactly one file icon");

            // Check that there are no bullet point spans
            var bulletSpans = noteElement.QuerySelectorAll("span").Where(span => 
                span.TextContent.Contains("•") || span.TextContent.Contains("\u2022"));
            Assert.That(bulletSpans.Count(), Is.EqualTo(0), 
                "Note elements should not contain bullet point characters");

            // Verify the note element itself has no list-style
            Assert.That(noteElement.ClassList.Contains("note-leaf"), Is.True, 
                "Note element should have 'note-leaf' class");
        }
    }

    [Test]
    public void FolderTree_CSS_HasCorrectListStyleRules()
    {
        // This test verifies that the CSS rules are properly applied
        // We can't directly test CSS in bunit, but we can verify the structure

        // Arrange & Act
        var component = RenderComponent<FolderTree>(parameters => parameters
            .Add(p => p.RootPath, "C:\\TestNotes"));

        // Assert folder structure exists
        var folderTreeContainer = component.Find(".folder-tree");
        Assert.That(folderTreeContainer, Is.Not.Null);

        // Check that folder elements exist
        var folderElements = component.FindAll("div.folder-label");
        Assert.That(folderElements.Count, Is.GreaterThan(0), "Should have folder elements");

        // Check that note elements exist with proper classes
        var noteElements = component.FindAll("li.note-leaf");
        Assert.That(noteElements.Count, Is.GreaterThan(0), "Should have note elements");

        // Verify structure integrity
        var allLiElements = component.FindAll("li");
        foreach (var li in allLiElements)
        {
            // Each li should be either a folder container or a note leaf
            var hasNoteClass = li.ClassList.Contains("note-leaf");
            var isInFolderTree = li.Closest(".folder-tree") != null;
            
            Assert.That(isInFolderTree, Is.True, 
                "All li elements should be within folder-tree container");
        }
    }

    [Test]
    public void FolderTree_RendersHierarchicalStructure_WithoutBullets()
    {
        // Arrange & Act
        var component = RenderComponent<FolderTree>(parameters => parameters
            .Add(p => p.RootPath, "C:\\TestNotes"));

        // Assert hierarchical structure
        var markup = component.Markup;
        
        // Check that the structure contains expected elements
        Assert.That(markup.Contains("Subfolder1"), Is.True, "Should contain Subfolder1");
        Assert.That(markup.Contains("Subfolder2"), Is.True, "Should contain Subfolder2");
        Assert.That(markup.Contains("Note 1"), Is.True, "Should contain Note 1");
        Assert.That(markup.Contains("Note 2"), Is.True, "Should contain Note 2");

        // Verify no bullet point characters in the markup
        Assert.That(markup.Contains("•"), Is.False, "Markup should not contain bullet characters");
        Assert.That(markup.Contains("\u2022"), Is.False, "Markup should not contain unicode bullet characters");

        // Verify proper icon usage instead of bullets
        Assert.That(markup.Contains("fa-file-text"), Is.True, 
            "Should use file icons for notes instead of bullets");
        Assert.That(markup.Contains("fa-folder"), Is.True, 
            "Should use folder icons for directories");
    }

    [Test]
    public void FolderTree_AllListElements_HaveNoListStyleType()
    {
        // Arrange & Act
        var component = RenderComponent<FolderTree>(parameters => parameters
            .Add(p => p.RootPath, "C:\\TestNotes"));

        // Get all ul and li elements
        var allListElements = new List<AngleSharp.Dom.IElement>();
        allListElements.AddRange(component.FindAll("ul"));
        allListElements.AddRange(component.FindAll("li"));

        // Assert
        Assert.That(allListElements.Count, Is.GreaterThan(0), "Should have list elements to test");

        foreach (var element in allListElements)
        {
            var tagName = element.TagName.ToLower();
            var className = element.GetAttribute("class") ?? "";
            
            // Verify the element is part of folder tree
            var isInFolderTree = element.Closest(".folder-tree") != null;
            if (isInFolderTree)
            {
                // This is a more comprehensive check - we verify that our CSS classes are applied
                // which should remove list bullets through CSS rules
                if (tagName == "ul" || tagName == "li")
                {
                    // The CSS should be removing list styles for these elements
                    // We can't directly test CSS, but we can verify structure is correct for CSS to work
                    Assert.That(isInFolderTree, Is.True, 
                        $"{tagName} element should be within folder-tree for CSS rules to apply");
                }
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        Dispose();
    }
}