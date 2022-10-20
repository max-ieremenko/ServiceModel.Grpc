using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Contract;
using Shouldly;

namespace ConsoleClient.Internal;

internal sealed class HttpClientCalls : IClientCalls
{
    private readonly bool _useCompression;
    private readonly Uri _address;
    private readonly HttpClient _httpClient;

    public HttpClientCalls(string address, bool useCompression)
    {
        _useCompression = useCompression;
        _address = new Uri(address);
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });
    }

    public async Task UploadFileAsync(string filePath, int bufferSize, CancellationToken token)
    {
        var requestUri = new Uri(_address, "api/FileService/upload?bufferSize=" + bufferSize);
        var fileSize = new FileInfo(filePath).Length;
        var fileName = Path.GetFileName(filePath);

        var progress = new ProgressBar("upload " + fileName, fileSize);

        HttpResponseMessage response;

        using (var file = File.OpenRead(filePath))
        using (var content = new StreamContentWithProgress(file, bufferSize, _useCompression, progress.Report))
        {
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Octet);
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            response = await _httpClient.PostAsync(requestUri, content, token);
        }

        FileMetadata metadata;
        using (response)
        {
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(token);
            metadata = JsonSerializer.Deserialize<FileMetadata>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        metadata.FileName.ShouldBe(fileName);
        metadata.Size.ShouldBe(fileSize);
    }

    public async Task DownloadFileAsync(string filePath, int bufferSize, CancellationToken token)
    {
        var requestUri = new Uri(
            _address,
            string.Format("api/FileService/download?filePath={0}&bufferSize={1}", HttpUtility.UrlEncode(filePath), bufferSize));

        var expectedFieName = Path.GetFileName(filePath);
        var expectedFileSize = new FileInfo(filePath).Length;

        using (var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, token))
        {
            response.EnsureSuccessStatusCode();

            var fileName = response.Content.Headers.ContentDisposition?.FileName;
            var xContentLength = response.Headers.GetValues("X-Content-Length").Single();
            var fileSize = long.Parse(xContentLength, CultureInfo.InvariantCulture);

            fileName.ShouldBe(expectedFieName);
            fileSize.ShouldBe(expectedFileSize);

            var progress = new ProgressBar("download " + fileName, fileSize);

            var tempFileName = Path.GetTempFileName();
            using (var stream = await response.Content.ReadAsStreamAsync(token))
            using (var tempFile = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                await stream.CopyToAsync(tempFile, bufferSize, token, progress.Report);

                tempFile.Length.ShouldBe(expectedFileSize);
            }

            File.Delete(tempFileName);
        }
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return new ValueTask(Task.CompletedTask);
    }
}