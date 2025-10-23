using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using FSI.BusinessProcessManagement.Services.Http;

namespace FSI.BusinessProcessManagement.Services.Auth;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenAccessor _tokens;
    private readonly IJSRuntime _js;
    private readonly JwtSecurityTokenHandler _handler = new();

    // sinaliza que já passou do primeiro render no cliente
    private volatile bool _clientReady;

    public JwtAuthenticationStateProvider(TokenAccessor tokens, IJSRuntime js)
    {
        _tokens = tokens;
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Fase 1: antes do primeiro render no cliente, devolve anônimo (NÃO usa LocalStorage)
        if (!_clientReady)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Fase 2: cliente pronto → pode ler LocalStorage
        string? jwt = null;
        try { jwt = await _tokens.GetTokenAsync(); } catch { /* ignore */ }

        var principal = BuildPrincipalFromJwt(jwt);
        return new AuthenticationState(principal);
    }

    // Chamado pelo componente helper após o 1º render
    public async Task NotifyClientReadyAsync()
    {
        _clientReady = true;
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public async Task SignInAsync(string jwt)
    {
        await _tokens.SetTokenAsync(jwt);     // <-- grava cache + localStorage
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _tokens.ClearAsync();           // <-- limpa cache + localStorage
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private ClaimsPrincipal BuildPrincipalFromJwt(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            var token = _handler.ReadJwtToken(jwt);

            if (TryGetExpUnixSeconds(token, out var exp) &&
                DateTimeOffset.FromUnixTimeSeconds(exp) <= DateTimeOffset.UtcNow)
                return new ClaimsPrincipal(new ClaimsIdentity()); // expirado

            var claims = NormalizeClaims(token);
            return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "jwt"));
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

    private static bool TryGetExpUnixSeconds(JwtSecurityToken token, out long exp)
    {
        exp = 0;
        if (!token.Payload.TryGetValue("exp", out var raw) || raw is null)
            return false;

        try
        {
            switch (raw)
            {
                case long l: exp = l; return true;
                case int i: exp = i; return true;
                case string s when long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ls): exp = ls; return true;
                case JsonElement je:
                    if (je.ValueKind == JsonValueKind.Number && je.TryGetInt64(out var jl)) { exp = jl; return true; }
                    if (je.ValueKind == JsonValueKind.String && long.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var jls)) { exp = jls; return true; }
                    break;
                default:
                    exp = Convert.ToInt64(raw, CultureInfo.InvariantCulture);
                    return true;
            }
        }
        catch { /* ignore */ }

        return false;
    }

    private static IEnumerable<Claim> NormalizeClaims(JwtSecurityToken token)
    {
        var claims = token.Claims.ToList();
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        roles.UnionWith(claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
        roles.UnionWith(claims.Where(c => c.Type.Equals("role", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value));

        if (token.Payload.TryGetValue("roles", out var raw))
        {
            if (raw is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Array)
                    foreach (var x in je.EnumerateArray())
                        if (x.ValueKind == JsonValueKind.String) roles.Add(x.GetString()!);
                        else if (je.ValueKind == JsonValueKind.String)
                            roles.Add(je.GetString()!);
            }
            else if (raw is string s) roles.Add(s);
        }

        claims.RemoveAll(c => c.Type == ClaimTypes.Role || c.Type.Equals("role", StringComparison.OrdinalIgnoreCase));
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var sub = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (sub != null && !claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));

        var name = claims.FirstOrDefault(c => c.Type is "name" or "username" or "preferred_username")?.Value;
        if (name != null && !claims.Any(c => c.Type == ClaimTypes.Name))
            claims.Add(new Claim(ClaimTypes.Name, name));

        return claims;
    }
}
