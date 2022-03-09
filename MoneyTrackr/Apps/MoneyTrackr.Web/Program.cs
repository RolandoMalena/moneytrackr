using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MoneyTrackr.ApiClient;
using MoneyTrackr.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ClientConfigurationSettings clientSettings = new();

if (builder.HostEnvironment.IsDevelopment())
{
    clientSettings.BaseApiAddress = new Uri("https://localhost:44356");
}

if (builder.HostEnvironment.IsStaging())
{
    clientSettings.BaseApiAddress = new Uri("http://137.117.75.48:8081/");
}

if (builder.HostEnvironment.IsProduction())
{
    clientSettings.BaseApiAddress = new Uri("http://137.117.75.48:8091/");
}

builder.Services.AddMoneyTrackrClient(clientSettings);

await builder.Build().RunAsync();
