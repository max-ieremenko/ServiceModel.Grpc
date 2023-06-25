using Grpc.Core;

namespace Client.Services;

internal sealed class CallOptionsFactoryWithAuthorization
{
    private readonly IJwtTokenProvider _tokenProvider;

    public CallOptionsFactoryWithAuthorization(IJwtTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public CallOptions Create()
    {
        var metadata = new Metadata();
        SetAuthorization(metadata);

        return new CallOptions(metadata);
    }

    private void SetAuthorization(Metadata metadata)
    {
        // on each gRPC call ask for the current token
        var token = _tokenProvider.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            metadata.Add("Authorization", "Bearer " + token);
        }
    }
}