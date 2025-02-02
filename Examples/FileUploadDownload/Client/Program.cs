using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Client.Internal;
using Contract;

namespace Client;

public static class Program
{
    // BuildHost in Server
    private const bool UseCompression = false;

    public static async Task Main()
    {
        using (var tokenSource = new AppExitTokenSource())
        {
            await RunGrpcClientAsync(tokenSource.Token);

            await RunHttpClientAsync(tokenSource.Token);
        }
    }

    public static string FindDemoFile()
    {
        const string fileName = "Files/taxi-fare-test.csv";

        string? result = null;

        var directory = AppDomain.CurrentDomain.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                result = path;
                break;
            }

            directory = Path.GetDirectoryName(directory);
        }

        if (result == null)
        {
            throw new InvalidOperationException($"{fileName} not found.");
        }

        return Path.GetFullPath(result);
    }

    private static async Task RunGrpcClientAsync(CancellationToken token)
    {
        var filePath = FindDemoFile();

        var calls = ClientCallsFactory.CreateGrpcClient(UseCompression);

        Console.WriteLine("----- Grpc UploadFile -----");
        await calls.UploadFileAsync(filePath, StreamExtensions.StreamDefaultCopyBufferSize, token);

        Console.WriteLine("----- Grpc DownloadFile -----");
        await calls.DownloadFileAsync(filePath, StreamExtensions.StreamDefaultCopyBufferSize, token);
    }

    private static async Task RunHttpClientAsync(CancellationToken token)
    {
        var filePath = FindDemoFile();

        var calls = ClientCallsFactory.CreateHttpClient(UseCompression);

        Console.WriteLine("----- Http UploadFile -----");
        await calls.UploadFileAsync(filePath, StreamExtensions.StreamDefaultCopyBufferSize, token);

        Console.WriteLine("----- Http DownloadFile -----");
        await calls.DownloadFileAsync(filePath, StreamExtensions.StreamDefaultCopyBufferSize, token);
    }
}