using Xunit;

namespace Misbah.BlazorDesktop.Tests;

public class AppStringsTests
{
    [Fact]
    public void AppStrings_All_Static_Properties_Are_NotNull()
    {
        var type = typeof(Misbah.BlazorShared.Resources.AppStrings);
        var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        foreach (var prop in props)
        {
            var value = prop.GetValue(null);
            Assert.NotNull(value);
        }
    }
}
