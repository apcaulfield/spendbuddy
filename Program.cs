using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpendBuddy;
using SpendBuddy.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("API URL is not configured.");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped(sp => new AccountService(sp.GetRequiredService<HttpClient>()));
builder.Services.AddScoped(sp => new ExpenseService(sp.GetRequiredService<HttpClient>()));

await builder.Build().RunAsync();
