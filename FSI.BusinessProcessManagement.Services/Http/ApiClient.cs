using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace FSI.BusinessProcessManagement.Services.Http;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly TokenAccessor _tokens;

    public ApiClient(HttpClient http, TokenAccessor tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    private async Task EnsureAuthAsync(CancellationToken ct)
    {
        if (_http.DefaultRequestHeaders.Authorization is not null) return;

        var tk = await _tokens.WaitForTokenAsync(TimeSpan.FromSeconds(3), ct);
        if (!string.IsNullOrWhiteSpace(tk))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tk);
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        await EnsureAuthAsync(ct);
        return await _http.GetFromJsonAsync<T>(url, ct);
    }

    public async Task<TResp?> PostAsync<TReq, TResp>(string url, TReq body, CancellationToken ct = default)
    {
        await EnsureAuthAsync(ct);
        var resp = await _http.PostAsJsonAsync(url, body, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TResp>(cancellationToken: ct);
    }

    public async Task PutAsync<TReq>(string url, TReq body, CancellationToken ct = default)
    {
        await EnsureAuthAsync(ct);
        var resp = await _http.PutAsJsonAsync(url, body, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        await EnsureAuthAsync(ct);
        var resp = await _http.DeleteAsync(url, ct);
        resp.EnsureSuccessStatusCode();
    }
}
