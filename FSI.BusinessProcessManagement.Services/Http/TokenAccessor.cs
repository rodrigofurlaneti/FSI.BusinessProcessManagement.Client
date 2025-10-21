using Blazored.LocalStorage;

namespace FSI.BusinessProcessManagement.Services.Http;

public sealed class TokenAccessor
{
    private readonly ILocalStorageService _storage;
    public const string TokenKey = "auth_token";

    public TokenAccessor(ILocalStorageService storage) => _storage = storage;

    public async Task<string?> GetTokenAsync()
        => await _storage.GetItemAsStringAsync(TokenKey);

    public async Task SetTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            await _storage.RemoveItemAsync(TokenKey);
        else
            await _storage.SetItemAsStringAsync(TokenKey, token);
    }

    public async Task ClearAsync() => await _storage.RemoveItemAsync(TokenKey);
}
