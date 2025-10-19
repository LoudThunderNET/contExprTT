using ContinetExpress.TT.Logic.ApiClients;
using ContinetExpress.TT.Logic.Models;

namespace ContinetExpress.TT.Logic
{
    public interface IHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken);
    }

    public class DistanceHandler(
        IDistanceCalculator distanceCalculator,
        IPlacesApi placesApi) : IHandler<DistanceRequest, double>
    {
        public async Task<double> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            var airportA = await placesApi.GetAirportAsync(distanceRequest.Source, cancellationToken);
            var airportB = await placesApi.GetAirportAsync(distanceRequest.Destination, cancellationToken);

            return distanceCalculator.Calculate(airportA.Location, airportB.Location);
        }
    }
}
