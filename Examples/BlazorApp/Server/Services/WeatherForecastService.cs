using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BlazorApp.Shared;

namespace BlazorApp.Server.Services
{
    internal sealed class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = 
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<IList<WeatherForecast>> GetForecasts()
        {
            var random = new Random();

            IList<WeatherForecast> forecasts = Enumerable
                .Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = random.Next(-20, 55),
                    Summary = Summaries[random.Next(Summaries.Length)]
                })
                .ToArray();

            return Task.FromResult(forecasts);
        }

        public async IAsyncEnumerable<WeatherForecast> StartForecast([EnumeratorCancellation] CancellationToken token)
        {
            var random = new Random();

            var dayIndex = 0;
            while (!token.IsCancellationRequested)
            {
                yield return new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(dayIndex),
                    TemperatureC = random.Next(-20, 55),
                    Summary = Summaries[random.Next(Summaries.Length)]
                };

                dayIndex++;
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
            }
        }
    }
}
