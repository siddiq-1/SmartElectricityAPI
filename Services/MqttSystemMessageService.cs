using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Server;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Services;

public class MqttSystemMessageService
{
    private readonly IMqttClient _mqttClient;
    private readonly string topic = "sysmessage";
    public MqttSystemMessageService()
    {
        var mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();

        init();
    }

    private async Task init()
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(CredentialHelpers.MqttServerBasicSettings().Server,CredentialHelpers.MqttServerBasicSettings().Port)
            .WithCredentials(CredentialHelpers.MqttServerBasicSettings().Username, CredentialHelpers.MqttServerBasicSettings().Password)
            .WithCleanSession()
            .Build();

        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
    }

    public async Task PublishSystemMesasge(MessagePayLoad messagePayLoad)
    {
        switch (messagePayLoad)
        {

            case MessagePayLoad.Refresh:
                var applicationMessage = new MqttApplicationMessageBuilder()
                  .WithTopic(topic)
                  .WithPayload("refresh")
                  .Build();

                await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                break;

            default:
                break;
        }
    }

    public enum MessagePayLoad
    {
        None,
        Refresh
    }
}
