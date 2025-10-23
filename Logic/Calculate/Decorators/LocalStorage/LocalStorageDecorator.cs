using ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage.Repositories;
using ContinetExpress.TT.Logic.Models;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;

namespace ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage
{
    public class LocalStorageDecorator(
        IHandler<DistanceRequest, double?> decoratee,
        IAirportsRepository airportsRepository,
        IDistanceCalculator distanceCalculator,
        ILogger<DistanceCalculator> logger) : IHandler<DistanceRequest, double?>
    {
        public async Task<double?> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            try
            {
                var airports = await airportsRepository.SearchAsync(distanceRequest.Source, distanceRequest.Destination, cancellationToken);
                var dbSrcAirport = airports.FirstOrDefault(a => a.IataCode == distanceRequest.Source);
                if (dbSrcAirport == null)
                {
                    goto from_db;
                }
                var dbDstAirport = airports.FirstOrDefault(a => a.IataCode == distanceRequest.Destination);
                if (dbDstAirport == null)
                {
                    goto from_db;
                }

                return distanceCalculator.Calculate(new Location(dbSrcAirport.Lon, dbSrcAirport.Lat), new Location(dbDstAirport.Lon, dbDstAirport.Lat));
            }
            catch (Exception e)
            {
                logger.LogError(e, "не удалось получить координаты из БД");
            }
        from_db:
            return await decoratee.HandleAsync(distanceRequest, cancellationToken);
        }
    }
}
