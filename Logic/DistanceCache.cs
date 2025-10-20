using ContinetExpress.TT.Logic.Models;
using Polly.Registry;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace ContinetExpress.TT.Logic
{
    public class DistanceCacheDecorator 
        : IHandler<DistanceRequest, double>
    {
        private readonly IHandler<DistanceRequest, double> _decoratee;
        private readonly IRedisDatabase _redisDatabase;
        public DistanceCacheDecorator(
            IHandler<DistanceRequest, double> decoratee, 
            IRedisDatabase redisDatabase)
        {
            _decoratee = decoratee;
            _redisDatabase = redisDatabase;
        }

        public async Task<double> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            var redisKey = $"{distanceRequest.Source}->{distanceRequest.Destination}";
            double? distance = null;
            bool redisAvailable = false;
            try
            {
                distance = await _redisDatabase
                    .GetAsync<double>(redisKey)
                    .WaitAsync(cancellationToken);
                redisAvailable = true;
            }
            catch (Exception)
            {
                distance = null;
            }
            
            if (!distance.HasValue)
            {
                distance = await _decoratee.HandleAsync(distanceRequest, cancellationToken);

                if (redisAvailable)
                {
                    await _redisDatabase.AddAsync(redisKey, distance);
                }
            }
            return distance.Value;
        }
    }
}
