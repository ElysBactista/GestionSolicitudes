using CurrieTechnologies.Razor.SweetAlert2;
using GestionSolicitudes.Client;
using GestionSolicitudes.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7005/")
});

builder.Services.AddScoped<SolicitudClientService>();
builder.Services.AddSweetAlert2();
builder.Services.AddSingleton<ExpedienteStateService>();

// Autenticación y Layouts
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>(provider =>
    (CustomAuthenticationStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<GestionSolicitudes.Client.Services.ExpedienteStateService>();
await builder.Build().RunAsync();