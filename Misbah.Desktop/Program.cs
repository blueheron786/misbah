using Microsoft.AspNetCore.Components.Web;
using BlazorDesktop.Hosting;
using Misbah.Desktop.Components;
using Misbah.Core.Services;
using Misbah.Core.Utils;

var builder = BlazorDesktopHostBuilder.CreateDefault(args);
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.UseDeveloperTools();
}


// Register services for DI
builder.Services.AddSingleton<INoteService>(sp => new NoteService("Notes")); // TODO: set actual notes root path
builder.Services.AddSingleton<IFolderService, FolderService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<MarkdownRenderer>();

await builder.Build().RunAsync();
