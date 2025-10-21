using System.Net.Http.Json;

namespace FSI.BusinessProcessManagement.Services.Http;

public class ApiClient
{
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<T>(url, ct);

    public async Task<TResp?> PostAsync<TReq, TResp>(string url, TReq body, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync(url, body, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TResp>(cancellationToken: ct);
    }

    public async Task PutAsync<TReq>(string url, TReq body, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync(url, body, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync(url, ct);
        resp.EnsureSuccessStatusCode();
    }
}
