using Grpc.Core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Client;

internal static class SwaggerClientTools
{
    private const string MediaTypeNameSwaggerRequest = MediaTypeNames.Application.Json + "+servicemodel.grpc";

    public static async Task<TResponse> PostAsync<TResponse>(this HttpClient httpClient, string requestUri, object? request, CancellationToken token = default)
    {
        using var requestContent = JsonContent.Create(request, new MediaTypeHeaderValue(MediaTypeNameSwaggerRequest));

        using var response = await httpClient.PostAsync(requestUri, requestContent, token);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: token);
            return result!;
        }

        var status = await response.Content.ReadFromJsonAsync<GrpcStatus>(cancellationToken: token);
        throw new RpcException(new Status(
            Enum.Parse<StatusCode>(status!.StatusCode),
            status.Detail));
    }

    private sealed class GrpcStatus
    {
        public string StatusCode { get; set; } = null!;

        public string Detail { get; set; } = null!;
    }
}