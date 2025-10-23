using ContinetExpress.TT.Logic.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using StackExchange.Redis;

namespace ContinetExpress.TT.Logic.Calculate.Decorators.Caching
{
    public interface IRedisDbFactory : IDisposable, IAsyncDisposable
    {
        Task<IDatabase?> CreateAsync();
    }

    public class RedisDbFactory : IRedisDbFactory
    {
        private ConnectionMultiplexer? _muxer;
        private readonly ConfigurationOptions _configurationOptions;
        private bool _disposedValue;
        private readonly ILogger<RedisDbFactory> _logger;

        public RedisDbFactory(
            IOptions<RedisSettings> options,
            ILoggerFactory loggerFactory)
        {
            _configurationOptions = ConfigurationOptions.Parse(options.Value.ConnectionString);
            _configurationOptions.IncludeDetailInExceptions = true;
            _configurationOptions.LoggerFactory = loggerFactory;
            _configurationOptions.ClientName = "distance-calculator";
            _logger = loggerFactory.CreateLogger<RedisDbFactory>();
        }

        public async Task<IDatabase?> CreateAsync()
        {
            try
            {
                _muxer ??= await ConnectionMultiplexer.ConnectAsync(_configurationOptions);

                if(_muxer.IsConnected)
                    return _muxer.GetDatabase();
            }
            catch (RedisConnectionException e)
            {
                _logger.LogError(e, "не удалось подключиться к Redis");

                return null;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _muxer?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposedValue)
            {
                await (_muxer?.DisposeAsync() ?? ValueTask.CompletedTask);
                _disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
