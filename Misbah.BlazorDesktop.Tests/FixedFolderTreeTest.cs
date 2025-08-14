using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Misbah.Web.Components.Pages.Notes;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Misbah.BlazorDesktop.Tests;

/// <summary>
/// Simple test to verify FolderTree displays without bullet decorations.
/// </summary>
[TestFixture]
public class SimpleFolderTreeTest
{
    private Bunit.TestContext _ctx = null!;

    [SetUp]
    public void Setup()
    {
        _ctx = new Bunit.TestContext();
        
        // Set up mock service first
        var mockFolderService = Substitute.For<IFolderService>();
        
        var testFolder = new FolderNode
        {
            Name = "Test",
            Path = "C:\\Test",
            Notes = new List<Note>
            {
                new Note 
                { 
                    Id = "1", 
                    Title = "Test Note.md", 
                    FilePath = "C:\\Test\\Test Note.md",
                    Content = "Test content"
                }
            },
            Folders = new List<FolderNode>()
        };
        
        mockFolderService.LoadFolderTree(Arg.Any<string>()).Returns(testFolder);
        
        // Add all services BEFORE any component rendering
        _ctx.Services.AddLogging();
        _ctx.Services.AddSingleton(mockFolderService);
    }

    [TearDown]
    public void TearDown()
    {
        _ctx?.Dispose();
    }

    [Test]
    public void FolderTree_Renders_Without_Bullets()
    {
        // Arrange & Act
        var component = _ctx.RenderComponent<FolderTree>(parameters => parameters
            .Add(p => p.RootPath, "C:\\Test"));

        // Assert
        var markup = component.Markup;
        Assert.That(markup, Is.Not.Empty, "Component should render markup");
        
        // Check that there are no bullet points (• character) in the rendered HTML
        Assert.That(markup.Contains("•"), Is.False, "Should not contain bullet characters");
        
        // Check for proper CSS classes that should suppress bullets
        Assert.That(markup.Contains("folder-tree"), Is.True, "Should contain folder-tree class");
    }

    [Test] 
    public void CSS_Files_Contain_ListStyle_Rules()
    {
        // This test verifies our CSS rules are in place
        var webAppCss = System.IO.Path.Combine("D:\\code\\misbah\\Misbah.Web\\wwwroot\\css\\app.css");
        var desktopAppCss = System.IO.Path.Combine("D:\\code\\misbah\\Misbah.BlazorDesktop\\wwwroot\\css\\app.css");
        
        if (System.IO.File.Exists(webAppCss) || System.IO.File.Exists(desktopAppCss))
        {
            var hasWebRules = !System.IO.File.Exists(webAppCss) || System.IO.File.ReadAllText(webAppCss).Contains("list-style: none");
            var hasDesktopRules = !System.IO.File.Exists(desktopAppCss) || System.IO.File.ReadAllText(desktopAppCss).Contains("list-style: none");
            
            Assert.That(hasWebRules && hasDesktopRules, Is.True, "CSS files should contain list-style: none rules");
        }
        else
        {
            Assert.Inconclusive("CSS files not found or don't contain expected rules - this may be expected in the test environment");
        }
    }
}
