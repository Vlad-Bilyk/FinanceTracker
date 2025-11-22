using Blazored.LocalStorage;
using FinanceTracker.BlazorUI;
using FinanceTracker.BlazorUI.Services.Auth;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUri = builder.Configuration.GetValue<string>("ApiBaseUri")
    ?? throw new InvalidOperationException("ApiBaseUrl not configured");
var apiBaseAddress = new Uri(apiBaseUri);

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
}).AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<AuthMessageHandler>();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
