using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp.Shared
{
    [ServiceContract]
    public interface IWeatherForecastService
    {
        [OperationContract]
        Task<IList<WeatherForecast>> GetForecasts();

        [OperationContract]
        IAsyncEnumerable<WeatherForecast> StartForecast(CancellationToken token);
    }
}
