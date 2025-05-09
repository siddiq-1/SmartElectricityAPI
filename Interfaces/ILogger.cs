using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Interfaces;

public interface IMqttLogger
{
    Task Log(MySQLDBContext mySqldbContext, int inverterId, Direction direction, MqttMessageOrigin mqttMessageOrigin, string topic, string payload, ActionTypeCommand actionTypeCommand, bool commandDispatched, MQttMessageType mQttMessageType, string extraInfo = "");
}
