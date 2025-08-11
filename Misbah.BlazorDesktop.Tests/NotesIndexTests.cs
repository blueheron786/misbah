using System.Collections.Generic;
using Misbah.BlazorShared.Pages;
using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
using Misbah.Application.DTOs;
using Misbah.Application.Interfaces;
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
