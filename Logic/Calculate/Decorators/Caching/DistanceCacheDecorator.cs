using ContinetExpress.TT.Logic.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ContinetExpress.TT.Logic.Calculate.Decorators.Caching
{
    public class DistanceCacheDecorator(
        IHandler<DistanceRequest, double?> decoratee,
        IRedisDbFactory redisDbFactory,
        ILogger<DistanceCacheDecorator> logger)
                : IHandler<DistanceRequest, double?>
    {
        public async Task<double?> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            var redisKey = $"{distanceRequest.Source}->{distanceRequest.Destination}";
            double? distance = null;
            bool redisAvailable = false;
            IDatabase? redisDb = null;
            try
            {
                redisDb = await redisDbFactory.CreateAsync();
                if (redisDb != null)
                {
                    RedisValue redisValue = await redisDb.StringGetAsync(redisKey)
                                    .WaitAsync(cancellationToken);
                    if (redisValue.HasValue)
                    {
                        distance = (double?)redisValue;
                    }
                    redisAvailable = true;
                }
            }
            catch (Exception e)
            {
                distance = null;
                logger.LogError(e, "Не удалось получить данны из кэша");
            }
            
            if (!distance.HasValue)
            {
                distance = await decoratee.HandleAsync(distanceRequest, cancellationToken);

                if (redisAvailable)
                {
                    await redisDb!.StringSetAsync(redisKey, distance);
                }
            }
            return distance;
        }
    }
}
