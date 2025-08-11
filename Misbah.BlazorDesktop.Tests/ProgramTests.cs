using Bunit;
using Xunit;
using Misbah.BlazorDesktop.Components.Pages.Notes;
using Misbah.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Misbah.BlazorDesktop.Tests;

public class ProgramTests
{
    [Fact]
    public void Program_Main_Does_Not_Throw()
    {
        // Just ensure the entry point exists and does not throw
        // (UI will not actually launch in test)
        Misbah.BlazorDesktop.Program.Main();
        Assert.True(true);
    }
}
