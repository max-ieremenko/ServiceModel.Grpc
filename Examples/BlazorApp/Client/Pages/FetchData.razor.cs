using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Client.Pages;

public partial class FetchData
{
    private IList<WeatherForecast> _forecasts;

    [Inject]
    public IWeatherForecastService Service { get; set; }

    protected override Task OnInitializedAsync()
    {
        return Reload();
    }

    private async Task Reload()
    {
        _forecasts = null;
        StateHasChanged();

        _forecasts = await Service.GetForecasts();
    }
}