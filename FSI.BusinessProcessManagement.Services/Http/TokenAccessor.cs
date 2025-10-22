using Blazored.LocalStorage;

namespace FSI.BusinessProcessManagement.Services.Http;

public sealed class TokenAccessor
{
    private readonly ILocalStorageService _storage;
    public const string TokenKey = "auth_token";

    public TokenAccessor(ILocalStorageService storage) => _storage = storage;

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _storage.GetItemAsStringAsync(TokenKey);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string? token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                await _storage.RemoveItemAsync(TokenKey);
            else
                await _storage.SetItemAsStringAsync(TokenKey, token);
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _storage.RemoveItemAsync(TokenKey);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
