using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public interface IClientCalls : IAsyncDisposable
    {
        Task UploadFileAsync(string filePath, int bufferSize, CancellationToken token);

        Task DownloadFileAsync(string filePath, int bufferSize, CancellationToken token);
    }
}
