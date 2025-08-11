using Bunit;
using Xunit;
using Misbah.BlazorShared.Pages.Notes;
// using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class AppStringsTests
{
    [Fact]
    public void AppStrings_Properties_Are_Accessible()
    {
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.GitHubRepo);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.WelcomeToMisbah);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.NoHubLoaded);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.LoadHub);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.HideFolders);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.ShowFolders);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.Notes);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.NoNotesFound);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.Preview);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.Edit);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.Save);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.SelectNoteToViewOrEdit);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.NoFoldersFound);
        Assert.NotNull(Misbah.BlazorDesktop.Resources.AppStrings.Cancel);
    }
}
