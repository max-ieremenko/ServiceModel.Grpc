using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Client.Internal;
using Contract;
using Shouldly;

namespace Client;

public sealed class GrpcClientCalls
{
    private readonly IFileService _fileService;

    public GrpcClientCalls(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task UploadFileAsync(string filePath, int bufferSize, CancellationToken token)
    {
        var fileSize = new FileInfo(filePath).Length;
        var fileName = Path.GetFileName(filePath);

        var progress = new ProgressBar("upload " + fileName, fileSize);
        var stream = File.OpenRead(filePath).ToAsyncEnumerable(token, progress.Report, bufferSize);

        var response = await _fileService.UploadAsync(stream, fileName, token);

        response.FileName.ShouldBe(fileName);
        response.Size.ShouldBe(fileSize);
    }

    public async Task DownloadFileAsync(string filePath, int bufferSize, CancellationToken token)
    {
        var expectedFileSize = new FileInfo(filePath).Length;
        var expectedFileName = Path.GetFileName(filePath);

        var (stream, metadata) = await _fileService.DownloadAsync(filePath, bufferSize, token);

        metadata.FileName.ShouldBe(expectedFileName);
        metadata.Size.ShouldBe(expectedFileSize);

        var progress = new ProgressBar("download " + metadata.FileName, metadata.Size);

        var tempFileName = Path.GetTempFileName();
        using (var tempFile = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite))
        {
            await stream.CopyToAsync(tempFile, token, progress.Report);

            tempFile.Length.ShouldBe(expectedFileSize);
        }

        File.Delete(tempFileName);
    }
}