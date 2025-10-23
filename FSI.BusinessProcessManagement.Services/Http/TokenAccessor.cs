using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace FSI.BusinessProcessManagement.Services.Http
{
    public sealed class TokenAccessor
    {
        private readonly ILocalStorageService _storage;
        public const string TokenKey = "auth_token";
        private string? _cached;

        public TokenAccessor(ILocalStorageService storage) => _storage = storage;
        public Task<string?> GetTokenAsync() => Task.FromResult(_cached);

        public async Task<string?> WaitForTokenAsync(TimeSpan? timeout = null, CancellationToken ct = default)
        {
            timeout ??= TimeSpan.FromSeconds(3);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (string.IsNullOrWhiteSpace(_cached) && sw.Elapsed < timeout && !ct.IsCancellationRequested)
                await Task.Delay(100, ct); 

            return _cached;
        }

        public async Task SetTokenAsync(string? token)
        {
            _cached = string.IsNullOrWhiteSpace(token) ? null : token;

            try
            {
                if (_cached is null) await _storage.RemoveItemAsync(TokenKey);
                else await _storage.SetItemAsStringAsync(TokenKey, _cached);
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

}

