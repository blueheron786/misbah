using System.Collections.Generic;
using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class FolderTreeTests : TestContext
{
    [Fact]
    public void Renders_Folder_And_Notes()
    {
        // Arrange
        var folder = new FolderNodeDto
        {
            Name = "root",
            Path = "root",
            Folders = new List<FolderNodeDto> { new FolderNodeDto { Name = "sub", Path = "root/sub" } },
            Notes = new List<NoteDto> { new NoteDto { Id = "n1", Title = "Note 1", FilePath = "n1" } }
        };
    var folderService = Substitute.For<IFolderService>();
    folderService.GetFolderByPathAsync(Arg.Any<string>()).Returns(System.Threading.Tasks.Task.FromResult(folder));
    Services.AddSingleton(folderService);

        // Act
        var cut = RenderComponent<FolderTree>(parameters => parameters.Add(p => p.RootPath, "root"));

        // Assert
        Assert.Contains("root", cut.Markup);
        Assert.Contains("sub", cut.Markup);
        Assert.Contains("Note 1", cut.Markup);
    }
}
