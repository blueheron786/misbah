using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;
using Misbah.Web;
using Misbah.Core.Services;
using Misbah.Core.Utils;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Register services for DI
builder.Services.AddSingleton<INoteService>(sp => new NoteService("Notes")); 
builder.Services.AddSingleton<IFolderService, FolderService>();
builder.Services.AddSingleton<SearchService>();
builder.Services.AddSingleton<MarkdownRenderer>();

var host = builder.Build();
await host.RunAsync();
