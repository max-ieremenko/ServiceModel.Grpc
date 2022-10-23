using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace Service;

public sealed class FileServiceRentedArray : IFileServiceRentedArray
{
    public async Task<FileMetadata> UploadAsync(IAsyncEnumerable<RentedArray> stream, string fileName, CancellationToken token)
    {
        var tempFileName = Path.GetTempFileName();

        long fileSize;
        using (var tempFile = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite))
        {
            await stream.CopyToAsync(tempFile, token);

            fileSize = tempFile.Length;
        }

        File.Delete(tempFileName);

        return new FileMetadata(fileName, fileSize);
    }

    public ValueTask<(IAsyncEnumerable<RentedArray> Stream, FileMetadata Metadata)> DownloadAsync(string filePath, int maxBufferSize, CancellationToken token)
    {
        var metadata = new FileMetadata(
            Path.GetFileName(filePath),
            new FileInfo(filePath).Length);

        var stream = File.OpenRead(filePath).ToAsyncEnumerableRented(token, maxBufferSize: maxBufferSize);

        return new ValueTask<(IAsyncEnumerable<RentedArray>, FileMetadata)>((stream, metadata));
    }
}