using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Helpers;

public static class CredentialHelpers
{
    private static readonly WebApplicationBuilder _builder = WebApplication.CreateBuilder();
    public static MqttServerSettings MqttServerBasicSettings()
    {
        MqttServerSettings mqttServerSettings = new();
        _builder.Configuration.GetSection("MqttServerBasic").Bind(mqttServerSettings);
        return mqttServerSettings;

    }

    public static MqttServerSettings MqttServerBasicSSLSettings()
    {
        MqttServerSettings mqttServerSettings = new();
        _builder.Configuration.GetSection("MqttServerBasicSSL").Bind(mqttServerSettings);
        return mqttServerSettings;

    }
}
