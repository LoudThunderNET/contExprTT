using ContinetExpress.TT.Logic.Models.Entities;
using Npgsql;
using Dapper;
using ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage.Repositories;

namespace ContinentExpress.TT.DataAccess
{
    public class AirportsRepository(NpgsqlDataSource npgsqlDataSource) : IAirportsRepository
    {
        public async Task<IReadOnlyCollection<Airport>> SearchAsync(string src_airport, string dest_airport, CancellationToken cancellationToken)
        {
            await using NpgsqlConnection conn = await npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            if (src_airport.Length > 4)
                src_airport = src_airport[..4];

            if (dest_airport.Length > 4)
                dest_airport = dest_airport[..4];

            var airports = await conn.QueryAsync<Airport>(
                    $"SELECT iata_code AS {nameof(Airport.IataCode)}, lat, lon FROM public.airport WHERE iata_code = ANY(@iatas)", 
                    new { iatas = new[] { src_airport, dest_airport } } )
                .WaitAsync(cancellationToken);

            return [.. airports];
        }
    }
}
