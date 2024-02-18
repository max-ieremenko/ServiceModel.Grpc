using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace ServerAspNetHost.Controllers;

// override buffer size in FileStreamResultExecutor.WriteFileAsync
internal sealed class FileStreamResultWithBufferSize : FileStreamResult
{
    private readonly int _bufferSize;

    public FileStreamResultWithBufferSize(
        Stream fileStream,
        string contentType,
        int bufferSize)
        : base(fileStream, contentType)
    {
        _bufferSize = bufferSize;
    }

    public override Task ExecuteResultAsync(ActionContext context)
    {
        var executor = new FileStreamResultExecutorWithBufferSize(
            context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>(),
            _bufferSize);

        return executor.ExecuteAsync(context, this);
    }

    private sealed class FileStreamResultExecutorWithBufferSize : FileStreamResultExecutor
    {
        private readonly int _bufferSize;

        public FileStreamResultExecutorWithBufferSize(ILoggerFactory loggerFactory, int bufferSize)
            : base(loggerFactory)
        {
            _bufferSize = bufferSize;
        }

        protected override Task WriteFileAsync(ActionContext context, FileStreamResult result, RangeItemHeaderValue? range, long rangeLength)
        {
            return WriteFileCoreAsync(context.HttpContext, result.FileStream, range, rangeLength);
        }

        // FileResultExecutorBase.WriteFileAsync with custom bufferSize
        private async Task WriteFileCoreAsync(HttpContext context, Stream fileStream, RangeItemHeaderValue? range, long rangeLength)
        {
            var outputStream = context.Response.Body;
            await using (fileStream)
            {
                try
                {
                    if (range == null)
                    {
                        await StreamCopyOperation.CopyToAsync(fileStream, outputStream, count: null, bufferSize: _bufferSize, cancel: context.RequestAborted);
                    }
                    else
                    {
                        fileStream.Seek(range.From!.Value, SeekOrigin.Begin);
                        await StreamCopyOperation.CopyToAsync(fileStream, outputStream, rangeLength, _bufferSize, context.RequestAborted);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Don't throw this exception, it's most likely caused by the client disconnecting.
                    // However, if it was cancelled for any other reason we need to prevent empty responses.
                    context.Abort();
                }
            }
        }
    }
}