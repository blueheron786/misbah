using System.Globalization;
using System.Threading;
using Microsoft.JSInterop;

namespace Misbah.Web
{
    public static class SetCulture
    {
        public static void ApplyFromJs(IJSRuntime js)
        {
            var culture = "en";
            try
            {
                // Try to get the culture from the global JS variable set by set-culture.js
                var result = js.InvokeAsync<string>("eval", "window.__misbahCulture").GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(result))
                    culture = result;
            }
            catch { }
            var ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}
