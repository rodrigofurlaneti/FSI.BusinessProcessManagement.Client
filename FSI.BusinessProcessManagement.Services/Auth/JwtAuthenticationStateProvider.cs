using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using FSI.BusinessProcessManagement.Services.Http;

namespace FSI.BusinessProcessManagement.Services.Auth;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenAccessor _tokens;

    public JwtAuthenticationStateProvider(TokenAccessor tokens)
        => _tokens = tokens;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var jwt = await _tokens.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(jwt))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var handler = new JwtSecurityTokenHandler();
        ClaimsIdentity identity;

        try
        {
            var token = handler.ReadJwtToken(jwt);
            identity = new ClaimsIdentity(token.Claims, authenticationType: "jwt");
        }
        catch
        {
            identity = new ClaimsIdentity();
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string token)
    {
        await _tokens.SetTokenAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _tokens.ClearAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
