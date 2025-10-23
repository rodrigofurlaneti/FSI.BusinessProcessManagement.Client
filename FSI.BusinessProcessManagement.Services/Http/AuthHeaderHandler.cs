using System.Net.Http.Headers;

namespace FSI.BusinessProcessManagement.Services.Http
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly TokenAccessor _tokens;
        public AuthHeaderHandler(TokenAccessor tokens) => _tokens = tokens;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            try
            {
                var token = await _tokens.GetTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // não propaga erro de JS/LocalStorage durante prerender
            }

            return await base.SendAsync(request, ct);
        }
    }

}