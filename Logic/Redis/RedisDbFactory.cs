using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ContinetExpress.TT.Logic.Redis
{
    public interface IRedisDbFactory : IDisposable, IAsyncDisposable
    {
        IDatabase? CreateAsync();
    }

    public class RedisDbFactory : IRedisDbFactory
    {
        private ConnectionMultiplexer _muxer;
        private bool _disposedValue;

        public RedisDbFactory(IOptions<RedisSettings> options)
        {
            _muxer = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
            _muxer.ConfigurationChanged += ConfigurationChanged;
        }

        private void ConfigurationChanged(object? sender, EndPointEventArgs e)
        {
            throw new NotImplementedException();
        }

        public IDatabase? CreateAsync()
        {
            if(_muxer.IsConnected)
                return _muxer.GetDatabase();

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _muxer.Dispose();
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
                await _muxer.DisposeAsync();
                _disposedValue = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
