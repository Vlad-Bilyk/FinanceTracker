using Blazored.LocalStorage;
using FinanceTracker.BlazorUI;
using FinanceTracker.BlazorUI.Services.ApiClients;
using FinanceTracker.BlazorUI.Services.Auth;
using FinanceTracker.BlazorUI.Services.Commons;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Services
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<AuthMessageHandler>();
builder.Services.AddTransient<GlobalHttpErrorHandler>();
builder.Services.AddScoped<IApiErrorHandler, ApiErrorHandler>();

// Http clients
var apiBaseUri = builder.Configuration.GetValue<string>("ApiBaseUri")
    ?? throw new InvalidOperationException("ApiBaseUrl not configured");
var apiBaseAddress = new Uri(apiBaseUri);

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

builder.Services.AddHttpClient<OperationTypesApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

builder.Services.AddHttpClient<CurrencyApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

builder.Services.AddHttpClient<WalletsApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

builder.Services.AddHttpClient<OperationsApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

builder.Services.AddHttpClient<ReportsApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

builder.Services.AddHttpClient<UsersApiClient>(client =>
{
    client.BaseAddress = apiBaseAddress;
})
.AddHttpMessageHandler<AuthMessageHandler>()
.AddHttpMessageHandler<GlobalHttpErrorHandler>();

// Packages
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
