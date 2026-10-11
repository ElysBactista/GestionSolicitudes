using GestionSolicitudes.Client;
using GestionSolicitudes.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Manejador de token Bearer para HttpClient
builder.Services.AddTransient<AuthHeaderHandler>();

// 2. HttpClient configurado con interceptor automático de autorización
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7005/")
    };
});

// 3. Soporte de Autenticación y Autorización en Blazor WASM
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped(sp => (CustomAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<GestionSolicitudes.Client.Services.IdiomaService>();

// 4. Servicios de Estado y Negocio
builder.Services.AddScoped<ExpedienteStateService>();
builder.Services.AddScoped<SolicitudClientService>();

await builder.Build().RunAsync();