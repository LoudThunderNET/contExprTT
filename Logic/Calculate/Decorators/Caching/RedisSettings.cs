#nullable disable
using ContinetExpress;

namespace ContinetExpress.TT.Logic.Calculate.Decorators.Caching
{
    public class RedisSettings
    {
        public const string ConnectionStringName = "Redis";
        public string ConnectionString { get; set; }
    }
}
