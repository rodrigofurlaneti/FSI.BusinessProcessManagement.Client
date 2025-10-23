using System.Net.Http.Headers;

namespace FSI.BusinessProcessManagement.Services.Http;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenAccessor _tokens;
    public AuthHeaderHandler(TokenAccessor tokens) => _tokens = tokens;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _tokens.WaitForTokenAsync(TimeSpan.FromSeconds(3), ct);

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
