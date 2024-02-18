using System;
using System.Threading;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Grpc.Core;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages;

public partial class Streaming : IDisposable
{
    private string _buttonText = "Start streaming";
    private WeatherForecast? _currentForecast;
    private CancellationTokenSource? _streamingCancellationSource;

    [Inject]
    public IWeatherForecastService Service { get; set; } = null!;

    private Task StartOrStopStreaming()
    {
        if (_streamingCancellationSource == null)
        {
            _buttonText = "Stop streaming";
            return StartStreaming();
        }

        _buttonText = "Start streaming";
        StopStreaming();
        return Task.CompletedTask;
    }

    private async Task StartStreaming()
    {
        try
        {
            using (_streamingCancellationSource = new CancellationTokenSource())
            {
                await foreach (var forecast in Service.StartForecast(_streamingCancellationSource.Token))
                {
                    _currentForecast = forecast;
                    StateHasChanged();
                }
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
        }
    }

    private void StopStreaming()
    {
        _streamingCancellationSource?.Cancel();
        _streamingCancellationSource = null;
        _currentForecast = null;
    }

    public void Dispose() => StopStreaming();
}