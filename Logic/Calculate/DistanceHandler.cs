using ContinetExpress.TT.Logic.ApiClients;
using ContinetExpress.TT.Logic.Models;

namespace ContinetExpress.TT.Logic.Calculate
{
    public interface IHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken);
    }

    public class DistanceHandler(
        IDistanceCalculator distanceCalculator,
        IPlacesApi placesApi) : IHandler<DistanceRequest, double?>
    {
        public async Task<double?> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            var airportATask = placesApi.GetAirportAsync(distanceRequest.Source, cancellationToken);
            var airportBTask = placesApi.GetAirportAsync(distanceRequest.Destination, cancellationToken);

            Task.WaitAll(airportATask, airportBTask);

            return distanceCalculator.Calculate(airportATask.Result!.Location, airportBTask.Result!.Location);
        }
    }
}
