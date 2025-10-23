using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace FSI.BusinessProcessManagement.Services.Http;

public sealed class TokenAccessor
{
    private readonly ILocalStorageService _storage;
    public const string TokenKey = "auth_token";
    private string? _cached;

    public TokenAccessor(ILocalStorageService storage) => _storage = storage;

    public async Task<string?> GetTokenAsync()
    {
        // 1º: cache (não depende de JS)
        if (!string.IsNullOrWhiteSpace(_cached))
            return _cached;

        // 2º: fallback para localStorage (JS) — pode não estar disponível em todos os momentos
        try
        {
            var fromLs = await _storage.GetItemAsStringAsync(TokenKey);
            _cached = string.IsNullOrWhiteSpace(fromLs) ? null : fromLs;
            return _cached;
        }
        catch (InvalidOperationException) { return null; }
        catch (JSDisconnectedException) { return null; }
        catch (JSException) { return null; }
    }

    public async Task SetTokenAsync(string? token)
    {
        _cached = string.IsNullOrWhiteSpace(token) ? null : token;

        try
        {
            if (_cached is null)
                await _storage.RemoveItemAsync(TokenKey);
            else
                await _storage.SetItemAsStringAsync(TokenKey, _cached);
        }
        catch (InvalidOperationException) { }
        catch (JSDisconnectedException) { }
        catch (JSException) { }
    }

    public async Task ClearAsync()
    {
        _cached = null;
        try { await _storage.RemoveItemAsync(TokenKey); }
        catch (InvalidOperationException) { }
        catch (JSDisconnectedException) { }
        catch (JSException) { }
    }
}
