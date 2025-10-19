using ContinetExpress.TT.Logic.Models;
using Refit;

namespace ContinetExpress.TT.Logic.ApiClients
{
    public interface IPlacesApi
    {
        [Get("/airports/{iata}")]
        Task<AirportDto> GetAirportAsync(string iata, CancellationToken cancellationToken);
    }
}
