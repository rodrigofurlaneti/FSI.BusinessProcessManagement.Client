using Microsoft.AspNetCore.Components.Authorization;
using FSI.BusinessProcessManagement.Services.Auth;
using FSI.BusinessProcessManagement.Services.Http;
using FSI.BusinessProcessManagement.Models.Request;
using FSI.BusinessProcessManagement.Models.Response;

namespace FSI.BusinessProcessManagement.Services;

public sealed class AuthService
{
    private readonly ApiClient _api;
    private readonly JwtAuthenticationStateProvider _authState;

    public AuthService(ApiClient api, AuthenticationStateProvider provider)
    {
        _api = api;
        _authState = (JwtAuthenticationStateProvider)provider;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var req = new LoginRequest { Username = username, Password = password };
        var resp = await _api.PostAsync<LoginRequest, LoginResponse>("/auth/login", req);
        if (resp is null || string.IsNullOrWhiteSpace(resp.AccessToken))
            return false;

        await _authState.SignInAsync(resp.AccessToken);
        return true;
    }

    public Task LogoutAsync() => _authState.SignOutAsync();
}
