using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace GestionSolicitudes.Client.Services;

public class AuthHeaderHandler(IJSRuntime jsRuntime) : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken", cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "token", cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization == null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Evitar bloqueos durante prerendering o desconexión JS
        }

        return await base.SendAsync(request, cancellationToken);
    }
}