using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Services;

internal sealed class HttpMessageHandlerWithAuthorization : DelegatingHandler
{
    private readonly IJwtTokenProvider _tokenProvider;

    public HttpMessageHandlerWithAuthorization(IJwtTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public HttpMessageHandlerWithAuthorization(HttpMessageHandler innerHandler, IJwtTokenProvider tokenProvider)
        : base(innerHandler)
    {
        _tokenProvider = tokenProvider;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SetAuthorization(request.Headers);
        return base.SendAsync(request, cancellationToken);
    }

    private void SetAuthorization(HttpRequestHeaders requestHeaders)
    {
        // on each gRPC call ask for the current token
        var token = _tokenProvider.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            requestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}