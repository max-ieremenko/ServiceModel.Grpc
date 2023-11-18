using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ServerAspNetHost.Controllers;

[Route("api/[controller]")]
public sealed class FileServiceController : Controller
{
    [HttpPost("upload")]
    [DisableFormValueModelBinding]
    public async Task<FileMetadata> UploadAsync([FromQuery] int bufferSize, CancellationToken token)
    {
        var fileName = new ContentDisposition(HttpContext.Request.Headers[HeaderNames.ContentDisposition]).FileName;

        long fileSize;
        var tempFileName = Path.GetTempFileName();
        using (var tempFile = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite))
        {
            var source = HttpContext.Request.Body;

            var encoding = HttpContext.Request.Headers[HeaderNames.ContentEncoding];
            if (encoding.Contains(CompressionSettings.Algorithm))
            {
                using (var zip = new GZipStream(source, CompressionMode.Decompress, true))
                {
                    await zip.CopyToAsync(tempFile, bufferSize, token);
                }
            }
            else
            {
                await source.CopyToAsync(tempFile, bufferSize, token);
            }

            fileSize = tempFile.Length;
        }

        var metadata = new FileMetadata(fileName, fileSize);
        System.IO.File.Delete(tempFileName);

        return metadata;
    }

    [HttpGet("download")]
    public IActionResult DownloadAsync([FromQuery] string filePath, [FromQuery] int bufferSize)
    {
        ////var provider = new FileExtensionContentTypeProvider();
        ////if (!provider.TryGetContentType(filePath, out var contentType))
        ////{
        ////    contentType = MediaTypeNames.Application.Octet;
        ////}

        // see Startup, ResponseCompressionOptions.MimeTypes
        var contentType = MediaTypeNames.Application.Octet;

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        Response.Headers.Append("X-Content-Length", stream.Length.ToString(CultureInfo.InvariantCulture));

        return new FileStreamResultWithBufferSize(
            stream,
            contentType,
            bufferSize)
        {
            FileDownloadName = Path.GetFileName(filePath)
        };
    }
}