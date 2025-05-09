using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using System.Diagnostics;
using SmartElectricityAPI.Database;

namespace SmartElectricityAPI.Services;

public class MqttClientService
{
    private readonly IMqttClient mqttClient;
    private readonly IMqttLogger mqttLogger;
    private bool isCommandDispatched = false;
    private bool shouldDispatchCommand = true;

    public MqttClientService(IMqttClient _mqttClient, IMqttLogger _mqttLogger, bool _shouldDispatchCommand)
    {
        mqttClient = _mqttClient;
        mqttLogger = _mqttLogger;
        shouldDispatchCommand = _shouldDispatchCommand;
    }

    public async Task<MqttClientService> PublishMessages(string topic, string payload)
    {
        isCommandDispatched = false;

        if (shouldDispatchCommand)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();
       
            if (mqttClient.IsConnected
                && !Debugger.IsAttached)
            {
                var pubresults = await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                if (pubresults.IsSuccess)
                {
                    isCommandDispatched = true;
                }
            }         
        }

        return this;
    }

    public async Task<MqttClientService> LogMessage(MySQLDBContext mySqldbContext, int inverterId, Direction direction, MqttMessageOrigin mqttMessageOrigin, string topic, string payload, ActionTypeCommand actionTypeCommand, bool commandDispatched, MQttMessageType mQttMessageType, string extraInfo = "")
    {
        if (isCommandDispatched || !shouldDispatchCommand)
        {
            await mqttLogger.Log(mySqldbContext, inverterId, direction, mqttMessageOrigin, topic, payload, actionTypeCommand, commandDispatched, mQttMessageType);
        }        

        return this;
    }


}
