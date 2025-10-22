using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using FSI.BusinessProcessManagement.Services.Http;

namespace FSI.BusinessProcessManagement.Services.Auth;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenAccessor _tokens;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtAuthenticationStateProvider(TokenAccessor tokens)
        => _tokens = tokens;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var jwt = await _tokens.GetTokenAsync();
        var principal = BuildPrincipalFromJwt(jwt);
        return new AuthenticationState(principal);
    }

    public async Task SignInAsync(string jwt)
    {
        await _tokens.SetTokenAsync(jwt);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _tokens.ClearAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private ClaimsPrincipal BuildPrincipalFromJwt(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            var token = _handler.ReadJwtToken(jwt);

            // ✅ exp robusto (int/long/string/JsonElement)
            if (TryGetExpUnixSeconds(token, out var exp) &&
                DateTimeOffset.FromUnixTimeSeconds(exp) <= DateTimeOffset.UtcNow)
            {
                return new ClaimsPrincipal(new ClaimsIdentity()); // expirado
            }

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
                case long l:
                    exp = l; return true;
                case int i:
                    exp = i; return true;
                case string s when long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ls):
                    exp = ls; return true;
                case JsonElement je:
                    if (je.ValueKind == JsonValueKind.Number && je.TryGetInt64(out var jl)) { exp = jl; return true; }
                    if (je.ValueKind == JsonValueKind.String && long.TryParse(je.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var jls)) { exp = jls; return true; }
                    break;
                default:
                    // tenta conversão genérica
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

        // ===== Roles =====
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // padrões comuns
        roles.UnionWith(claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
        roles.UnionWith(claims.Where(c => c.Type.Equals("role", StringComparison.OrdinalIgnoreCase)).Select(c => c.Value));

        if (token.Payload.TryGetValue("roles", out var raw))
        {
            if (raw is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var x in je.EnumerateArray())
                        if (x.ValueKind == JsonValueKind.String)
                            roles.Add(x.GetString()!);
                }
                else if (je.ValueKind == JsonValueKind.String)
                {
                    roles.Add(je.GetString()!);
                }
            }
            else if (raw is string s)
            {
                roles.Add(s);
            }
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
