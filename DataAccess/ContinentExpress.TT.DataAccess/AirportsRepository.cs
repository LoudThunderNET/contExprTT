using ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage.Repositories;
using ContinetExpress.TT.Logic.Models.Entities;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;

namespace ContinentExpress.TT.DataAccess
{
    public class AirportsRepository : IAirportsRepository
    {
        private readonly NpgsqlDataSource _npgsqlDataSource;
        private readonly ILogger<AirportsRepository> _logger;
        private readonly AsyncCircuitBreakerPolicy _policy;

        public AirportsRepository(NpgsqlDataSource npgsqlDataSource, ILogger<AirportsRepository> logger)
        {
            _npgsqlDataSource = npgsqlDataSource;
            _logger = logger;
            _policy = Policy
                .Handle<NpgsqlException>()
                .CircuitBreakerAsync(3, TimeSpan.FromMinutes(1));
        }

        public Task<IReadOnlyCollection<Airport>> SearchAsync(string src_airport, string dest_airport, CancellationToken cancellationToken)
        {
            if (!Global.MigrationAppied)
            {
                _logger.LogWarning("Получение координат из БД пропущено из-за отсуствия гарантий применения миграции и наличия данных");
                return Task.FromResult<IReadOnlyCollection<Airport>>([]);
            }

            if (_policy.CircuitState == CircuitState.Open)
            {
                _logger.LogWarning("Получение координат из БД пропущено из-за разрыва цепи");
                return Task.FromResult<IReadOnlyCollection<Airport>>([]);
            }

            return _policy.ExecuteAsync<IReadOnlyCollection<Airport>>(async (token) =>
            {
                await using NpgsqlConnection conn = await _npgsqlDataSource.OpenConnectionAsync(token);
                if (src_airport.Length > 4)
                    src_airport = src_airport[..4];

                if (dest_airport.Length > 4)
                    dest_airport = dest_airport[..4];

                var airports = await conn.QueryAsync<Airport>(
                        $"SELECT iata_code AS {nameof(Airport.IataCode)}, lat, lon FROM public.airport WHERE iata_code = ANY(@iatas)",
                        new { iatas = new[] { src_airport, dest_airport } })
                    .WaitAsync(token);

                return [.. airports];
            }, cancellationToken);
        }
    }
}
