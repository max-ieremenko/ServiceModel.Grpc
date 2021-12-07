using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IFileService
    {
        [OperationContract]
        Task<FileMetadata> UploadAsync(IAsyncEnumerable<byte[]> stream, string fileName, CancellationToken token = default);

        [OperationContract]
        ValueTask<(IAsyncEnumerable<byte[]> Stream, FileMetadata Metadata)> DownloadAsync(string filePath, int maxBufferSize, CancellationToken token = default);
    }
}
