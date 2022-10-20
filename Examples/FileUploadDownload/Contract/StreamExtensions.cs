using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Contract;

public static class StreamExtensions
{
    // see Microsoft.AspNetCore.Http.StreamCopyOperationInternal
    public const int HttpStreamCopyDefaultBufferSize = 4096;

    // see Microsoft.AspNetCore.Mvc.Infrastructure.FileResultExecutorBase.BufferSize
    public const int FileResultExecutorBufferSize = 64 * 1024;

    // see Stream.StreamDefaultCopyBufferSize
    public const int StreamDefaultCopyBufferSize = 80 * 1024;

    public static string SizeToString(long size)
    {
        string units;
        double value;
        if (size >= 1024 * 1024)
        {
            units = "MB";
            value = size >> 10;
        }
        else if (size >= 1024)
        {
            units = "KB";
            value = size;
        }
        else
        {
            return size.ToString("0 B");
        }

        value /= 1024;
        return value.ToString("0.00 ") + units;
    }

    public static async IAsyncEnumerable<byte[]> ToAsyncEnumerable(
        this Stream stream,
        [EnumeratorCancellation] CancellationToken token,
        Action<long> onProgress = default,
        int? maxBufferSize = default)
    {
        var chunkSize = CalculateChunkSize(maxBufferSize, stream);

        await using (stream)
        {
            var chunk = new byte[chunkSize];

            var progress = 0L;
            var chunkOffset = 0;
            int readLength;
            while ((readLength = await stream.ReadAsync(chunk, chunkOffset, chunkSize - chunkOffset, token)) > 0)
            {
                chunkOffset += readLength;
                if (chunkOffset == chunkSize)
                {
                    chunkOffset = 0;
                    yield return chunk;

                    progress += chunkSize;
                    onProgress?.Invoke(progress);
                }
            }

            if (chunkOffset > 0)
            {
                Array.Resize(ref chunk, chunkOffset);
                yield return chunk;

                progress += chunkOffset;
                onProgress?.Invoke(progress);
            }
        }
    }

    public static async Task CopyToAsync(
        this IAsyncEnumerable<byte[]> stream,
        Stream destination,
        CancellationToken token,
        Action<long> onProgress = default)
    {
        var progressValue = 0L;

        await foreach (var chunk in stream.WithCancellation(token))
        {
            await destination.WriteAsync(chunk, 0, chunk.Length, token);

            progressValue += chunk.Length;
            onProgress?.Invoke(progressValue);
        }
    }

    public static async IAsyncEnumerable<RentedArray> ToAsyncEnumerableRented(
        this Stream stream,
        [EnumeratorCancellation] CancellationToken token,
        Action<long> onProgress = default,
        int? maxBufferSize = default)
    {
        var chunkSize = CalculateChunkSize(maxBufferSize, stream);

        await using (stream)
        using (var chunk = RentedArray.Rent(chunkSize))
        {
            var progress = 0L;
            var chunkOffset = 0;
            int readLength;
            while ((readLength = await stream.ReadAsync(chunk.Array, chunkOffset, chunkSize - chunkOffset, token)) > 0)
            {
                chunkOffset += readLength;
                if (chunkOffset == chunkSize)
                {
                    chunkOffset = 0;
                    yield return chunk;

                    progress += chunkSize;
                    onProgress?.Invoke(progress);
                }
            }

            if (chunkOffset > 0)
            {
                chunk.Resize(chunkOffset);
                yield return chunk;

                progress += chunkOffset;
                onProgress?.Invoke(progress);
            }
        }
    }

    public static async Task CopyToAsync(
        this IAsyncEnumerable<RentedArray> stream,
        Stream destination,
        CancellationToken token,
        Action<long> onProgress = default)
    {
        var progressValue = 0L;

        await foreach (var chunk in stream.WithCancellation(token))
        {
            using (chunk)
            {
                await destination.WriteAsync(chunk.Array, 0, chunk.Length, token);
            }

            progressValue += chunk.Length;
            onProgress?.Invoke(progressValue);
        }
    }

    public static async Task CopyToAsync(
        this Stream stream,
        Stream destination,
        int bufferSize,
        CancellationToken token,
        Action<long> onProgress = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            var progress = 0L;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, token);
                    
                progress += bytesRead;
                onProgress?.Invoke(progress);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static int CalculateChunkSize(int? maxBufferSize, Stream stream)
    {
        var result = maxBufferSize ?? StreamDefaultCopyBufferSize;
        if (stream.CanSeek)
        {
            result = (int)Math.Min(result, stream.Length);
        }

        return result;
    }
}