using ContinetExpress.TT.Logic.Models;
using Microsoft.Extensions.Options;
using Polly.Registry;
using Refit;
using StackExchange.Redis;

namespace ContinetExpress.TT.Logic
{
    public class DistanceCacheDecorator 
        : IHandler<DistanceRequest, double>
    {
        private readonly IHandler<DistanceRequest, double> _decoratee;
        private readonly IOptions<RedisSettings> _redisSettings;
        private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
        private ConnectionMultiplexer? _muxer;
        private IDatabase? _db;
        public DistanceCacheDecorator(
            IHandler<DistanceRequest, double> decoratee, 
            IOptions<RedisSettings> redisSettings,
            ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            this._decoratee = decoratee;
            _redisSettings = redisSettings;
            this._resiliencePipelineProvider = resiliencePipelineProvider;
        }

        public async Task<double> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            Polly.ResiliencePipeline retryPipeline = _resiliencePipelineProvider.GetPipeline(Consts.PollyRetryPipeline);

            var result = await retryPipeline.ExecuteAsync(callback:token => WorkAsync(distanceRequest, token), cancellationToken);

            return result;
        }

        async Task<IDatabase?> GetDatabaseAsync()
        {
            try
            {
                _muxer ??= await ConnectionMultiplexer.ConnectAsync(_redisSettings.Value.ConnectionString);
                _db ??= _muxer.GetDatabase();

                return _db;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async ValueTask<double> WorkAsync(DistanceRequest distanceRequest, CancellationToken token)
        {
            var db = await GetDatabaseAsync();
            if (db != null)
            {
                var redisKey = new RedisKey($"{distanceRequest.Source}->{distanceRequest.Destination}");
                RedisValue value = await db.StringGetAsync(redisKey);
                if (value == RedisValue.Null)
                {
                    var distance = await _decoratee.HandleAsync(distanceRequest, token);
                    await db.StringSetAsync(redisKey, new RedisValue(distance.ToString()));

                    return distance;
                }

                return double.Parse(value!);
            }
            else
            {
                var distance = await _decoratee.HandleAsync(distanceRequest, token);

                return distance;
            }
        }
    }
}
