namespace SmartElectricityAPI.Enums;

public enum ActionTypeCommand
{
    Empty,
    ChargeMax,
    ChargeWithRemainingSun,
    SelfUse,
    ConsumeBatteryWithMaxPower,
    SellRemainingSunNoCharging,
    AutoMode,
    ThreePhaseAntiReflux,
    None,
    InverterSelfUse,
    PassiveMode,
    HzMarket
}
