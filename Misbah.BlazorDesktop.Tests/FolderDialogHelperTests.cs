using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Models;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class FolderDialogHelperTests
{
    [Fact]
    public void ShowFolderDialog_Returns_Null_When_Cancelled()
    {
        // This is a static WinForms dialog, so we can't test UI interaction in bUnit.
        // But we can test that the method exists and returns null if not selected.
        var result = FolderDialogHelper.ShowFolderDialog();
        Assert.Null(result);
    }
}
