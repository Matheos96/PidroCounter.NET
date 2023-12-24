using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PidroCounter.NET;
using PidroCounter.NET.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<PidroCounterStateService>();

builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
