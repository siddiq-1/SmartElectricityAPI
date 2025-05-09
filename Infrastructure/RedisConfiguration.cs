using SmartElectricityAPI.Models;
using StackExchange.Redis;

namespace SmartElectricityAPI.Infrastructure;

public class RedisConfiguration
{
    private static Lazy<ConnectionMultiplexer> _connectionMultiplexer;

    static RedisConfiguration()
    {
        _connectionMultiplexer = new Lazy<ConnectionMultiplexer>(() =>
        {
            var redisSettings = new RedisSettings();
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.GetSection("Redis").Bind(redisSettings);

            ConfigurationOptions options = new ConfigurationOptions
            {
                EndPoints = { { redisSettings.Server, redisSettings.Port } },
                Password = redisSettings.Password,
                Ssl = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                AbortOnConnectFail = false,
                ConnectTimeout = 10000,
                SyncTimeout = 10000,
                ConnectRetry = 3,
            };

            try
            {
                return ConnectionMultiplexer.Connect(options);
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        });
    }

    public static ConnectionMultiplexer GetConnection()
    {
        return _connectionMultiplexer.Value;
    }
}
