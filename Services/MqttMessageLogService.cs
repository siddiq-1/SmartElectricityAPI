using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using System.Diagnostics;

namespace SmartElectricityAPI.Services;

public class MqttMessageLogService : IMqttLogger
{
    private RedisCacheService redisCacheService;
    public MqttMessageLogService()
    {

        redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());
    }

    public async Task Log(MySQLDBContext mySqldbContext, int inverterId, Direction direction, MqttMessageOrigin mqttMessageOrigin, string topic, string payload, ActionTypeCommand actionTypeCommand, bool commandDispatched, MQttMessageType mQttMessageType, string extraInfo = "")
    {
          if (!Debugger.IsAttached)
          {
        try
        {
           // using (var dbContext = new DatabaseService().CreateDbContext())
           // {
                MqttMessageLog mqttMessageLog = new MqttMessageLog
                {
                    InverterId = inverterId,
                    Direction = direction,
                    MqttMessageOrigin = mqttMessageOrigin,
                    Topic = topic,
                    Payload = payload,
                    ActionTypeCommand = actionTypeCommand,
                    CommandDispatched = commandDispatched,
                    ExtraInfo = extraInfo,
                    MQttMessageType = mQttMessageType
                };

                mySqldbContext.MqttMessageLog.Add(mqttMessageLog);
                await mySqldbContext.SaveChangesAsync();

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore // Ignore circular references
                };

                var serializedMqttMessageLog = JsonConvert.SerializeObject(mqttMessageLog, settings);

                await redisCacheService.StoreKeyValue(Constants.CacheKeys.MqttLogKey(inverterId), serializedMqttMessageLog);
           // }
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync($"Error with inverterid: {inverterId} messageorigin: {mqttMessageOrigin} topic: {topic} payload: {payload} actionTypeCommand: {actionTypeCommand} commandDispatched: {commandDispatched} extraInfo: {extraInfo}");
            await Console.Out.WriteLineAsync(ex.Message);
            await Console.Out.WriteLineAsync(ex.StackTrace);
            await Console.Out.WriteLineAsync(ex.Source);
  
        }

       }
    }
}
