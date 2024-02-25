using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChat.Client.Shared.Internal;

internal sealed class HttpMessageHandlerWithAuthorization : DelegatingHandler
{
    private readonly IJwtTokenProvider _tokenProvider;

    public HttpMessageHandlerWithAuthorization(IJwtTokenProvider tokenProvider)
        : base(new HttpClientHandler())
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