using Newtonsoft.Json;
using SmartElectricityAPI.Dto;
using StackExchange.Redis;

namespace SmartElectricityAPI.Services;

public class RedisCacheService
{
    private IDatabase db;
    private ConnectionMultiplexer connectionMultiplexer;
    public RedisCacheService(ConnectionMultiplexer _connectionMultiplexer)
    {
        db = _connectionMultiplexer.GetDatabase();
        connectionMultiplexer = _connectionMultiplexer;
    }

    public async Task StoreKeyValue(string key, string transaction)
    {
        double score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await db.SortedSetAddAsync(key, transaction, score);

        // Set expiration time for the key
        TimeSpan expiry = TimeSpan.FromSeconds(60 * 60 * 24);
        await db.KeyExpireAsync(key, expiry, ExpireWhen.Always);
    }

    public async Task RemoveOldRange(string key, double unixTimeOffsetSeconds)
    {
        try
        {
            await db.SortedSetRemoveRangeByScoreAsync(key, double.NegativeInfinity, unixTimeOffsetSeconds);
        }

        catch (RedisException ex)
        {
            Console.WriteLine(ex.Message);
   
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
     
        }
    }

    public async Task<List<T>> GetKeyValue<T>(string key, double unixTimeOffsetSeconds)
    {
        var outputList = new List<T>();

        try
        {
            var transactions = await db.SortedSetRangeByScoreAsync(key, unixTimeOffsetSeconds, double.PositiveInfinity);

            if (transactions != null && transactions.Length > 0)
            {
                foreach (var item in transactions)
                {
                    outputList.Add(JsonConvert.DeserializeObject<T>(item));
                }
            }
        }
        catch (RedisException ex)
        {
            Console.WriteLine(ex.Message);
            outputList.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            outputList.Clear();
        }

        return outputList;
    }

    public void Disconnect()
    {
        connectionMultiplexer.Close();
    }
}
