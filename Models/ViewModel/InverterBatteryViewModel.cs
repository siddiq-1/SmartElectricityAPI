namespace SmartElectricityAPI.Models.ViewModel;

public class InverterBatteryViewModel
{
    public int Id { get; set; }
    public double CapacityKWh { get; set; }
    public double ChargingPowerFromGridKWh { get; set; }
    public double ChargingPowerFromSolarKWh { get; set; }    
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool ConsiderBadWeather75PercentFactor { get; set; }
    public bool LoadBatteryTo95PercentEnabled { get; set; }
    public int LoadBatteryTo95PercentPrice { get; set; }
    public int ExpectedProfitProducedAndSoldDifference { get; set; }
    public int NumberOfBatteries { get; set; }
    public double AdditionalTimeForBatteryChargingPercentage { get; set; }
    public double DischargingPowerToGridKWh { get; set; }
    public bool CalculateBatterSocFromVolts { get; set; }
    public int BatteryVoltsMin { get; set; }
    public int BatteryVoltsMax { get; set; }
    public int BatteryMinLevelWithConsumeMax { get; set; }
    public bool UseHzMarket { get; set; }
    public bool ClientUseHzMarket { get; set; }
    public double HzMarketDischargeMinPrice { get; set; }
    public int HzMarketMinBatteryLevelOnDischargeCommand { get; set; }
    public bool AllowPurchasingFromGridInSummer { get; set; }
    public bool ConsiderRemainingBatteryOnPurchase { get; set; }
    public bool Enabled { get; set; }
}
