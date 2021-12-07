using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Contract
{
    [ServiceContract]
    public interface IFileServiceRentedArray
    {
        [OperationContract]
        Task<FileMetadata> UploadAsync(IAsyncEnumerable<RentedArray> stream, string fileName, CancellationToken token);

        [OperationContract]
        ValueTask<(IAsyncEnumerable<RentedArray> Stream, FileMetadata Metadata)> DownloadAsync(string filePath, int maxBufferSize, CancellationToken token);
    }
}
