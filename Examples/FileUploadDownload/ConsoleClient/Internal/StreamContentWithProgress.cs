using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Contract;

namespace ConsoleClient.Internal;

internal sealed class StreamContentWithProgress : StreamContent
{
    private readonly Stream _content;
    private readonly int _bufferSize;
    private readonly bool _useCompression;
    private readonly Action<long> _onProgress;

    public StreamContentWithProgress(
        Stream content,
        int bufferSize,
        bool useCompression,
        Action<long> onProgress)
        : base(content)
    {
        _content = content;
        _bufferSize = bufferSize;
        _useCompression = useCompression;
        _onProgress = onProgress;

        if (useCompression)
        {
            Headers.ContentEncoding.Add(CompressionSettings.Algorithm);
            Headers.ContentLength = null;
        }
    }

    protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        if (_useCompression)
        {
            return SerializeCompressedToStreamAsync(stream, cancellationToken);
        }

        return _content.CopyToAsync(stream, _bufferSize, cancellationToken, _onProgress);
    }

    private async Task SerializeCompressedToStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using (var zip = new GZipStream(stream, CompressionLevel.Optimal, true))
        {
            await _content.CopyToAsync(zip, _bufferSize, cancellationToken, _onProgress);
        }
    }
}