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
    private readonly TokenAccessor _tokens;

    public AuthService(ApiClient api,
                       AuthenticationStateProvider provider,
                       IHttpClientFactory factory,
                       TokenAccessor tokens)
    {
        _api = api;
        _authState = (JwtAuthenticationStateProvider)provider;
        _anon = factory.CreateClient("ApiAnon");
        _tokens = tokens;
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var req = new LoginRequest { Username = username, Password = password };

        var httpResp = await _anon.PostAsJsonAsync("auth/login", req, ct);
        if (!httpResp.IsSuccessStatusCode) return false;

        var resp = await httpResp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (resp is null || string.IsNullOrWhiteSpace(resp.AccessToken)) return false;

        await _tokens.SetTokenAsync(resp.AccessToken);
        await _authState.SignInAsync(resp.AccessToken);

        return true;
    }

    public Task LogoutAsync() => _authState.SignOutAsync();
}
