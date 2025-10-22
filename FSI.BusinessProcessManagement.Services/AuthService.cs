using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using FSI.BusinessProcessManagement.Services.Auth;
using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Request;
using FSI.BusinessProcessManagement.Models.Response;

namespace FSI.BusinessProcessManagement.Services;

public sealed class AuthService
{
    private readonly ApiClient _api;
    private readonly HttpClient _anon;
    private readonly JwtAuthenticationStateProvider _authState;

    public AuthService(ApiClient api, AuthenticationStateProvider provider, IHttpClientFactory factory)
    {
        _api = api;
        _authState = (JwtAuthenticationStateProvider)provider;
        _anon = factory.CreateClient("ApiAnon");
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var req = new LoginRequest { Username = username, Password = password };
        var url = new Uri(_anon.BaseAddress!, "auth/login");
        Console.WriteLine($"POST => {url}");
        var httpResp = await _anon.PostAsJsonAsync("auth/login", req, ct);
        Console.WriteLine($"http Response => {httpResp}");
        if (!httpResp.IsSuccessStatusCode) return false;

        var resp = await httpResp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (resp is null || string.IsNullOrWhiteSpace(resp.AccessToken)) return false;

        await _authState.SignInAsync(resp.AccessToken);
        return true;
    }

    public Task LogoutAsync() => _authState.SignOutAsync();
}
