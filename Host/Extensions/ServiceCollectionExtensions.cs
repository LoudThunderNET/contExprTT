using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace ContinentExpress.TT.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStackExchangeRedisExtensions<T>(
                this IServiceCollection services,
                Func<IServiceProvider, IEnumerable<RedisConfiguration>> redisConfigurationFactory)
                where T : class, ISerializer
        {
            services.AddSingleton<IRedisClientFactory, RedisClientFactory>();
            services.AddSingleton<ISerializer, T>();

            services.AddSingleton((provider) => provider
                .GetRequiredService<IRedisClientFactory>()
                .GetDefaultRedisClient());

            services.AddSingleton((provider) => provider
                .GetRequiredService<IRedisClientFactory>()
                .GetDefaultRedisClient()
                .GetDefaultDatabase());

            services.AddSingleton(redisConfigurationFactory);

            return services;
        }
    }
}
