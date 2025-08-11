using System.Collections.Generic;
using Misbah.BlazorDesktop.Pages;
using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class NotesIndexTests : TestContext
{
    [Fact]
    public void Renders_NotesIndex_Page()
    {
        // Arrange
        Services.AddSingleton(Substitute.For<INoteService>());
        Services.AddSingleton(Substitute.For<IFolderService>());
        // Act
        var cut = RenderComponent<NotesIndex>();
        // Assert
        Assert.Contains("Notes", cut.Markup);
        Assert.Contains("New Note", cut.Markup);
    }
}
