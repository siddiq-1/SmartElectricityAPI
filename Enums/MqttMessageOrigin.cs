using System.ComponentModel;
namespace SmartElectricityAPI.Enums;

public enum MqttMessageOrigin
{
    AutoServiceBattery,
    [Description("BBS - Battery button service")]
    BBSSMinMaxThresold,
    [Description("BBS - Battery button service")]
    BBSSelfUse,
    BBSChargeWithRemainingSun,
    ManualButtonAction,
    AutoServiceInverter,
    BBSChargeMax,
    BBSSellRemainingSunNoCharging,
    HzMarket,
}
