using System.Collections.Generic;
using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class FolderTreeTests : TestContext
{
    [Fact]
    public void Renders_Folder_And_Notes()
    {
        // Arrange
        var folder = new FolderNode
        {
            Name = "root",
            Path = "root",
            Folders = new List<FolderNode> { new FolderNode { Name = "sub", Path = "root/sub" } },
            Notes = new List<Note> { new Note { Id = "n1", Title = "Note 1", FilePath = "n1" } }
        };
        var folderService = Substitute.For<IFolderService>();
        folderService.LoadFolderTree(Arg.Any<string>()).Returns(folder);
        Services.AddSingleton(folderService);

        // Act
        var cut = RenderComponent<FolderTree>(parameters => parameters.Add(p => p.RootPath, "root"));

        // Assert
        Assert.Contains("root", cut.Markup);
        Assert.Contains("sub", cut.Markup);
        Assert.Contains("Note 1", cut.Markup);
    }
}
