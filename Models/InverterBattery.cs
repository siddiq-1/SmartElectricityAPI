using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class InverterBattery : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Inverter Inverter { get; set; }
    public int InverterId { get; set; }
    public double CapacityKWh {  get; set; }
    public double ChargingPowerFromGridKWh { get; set; }
    public double ChargingPowerFromSolarKWh { get; set; }
    public double DischargingPowerToGridKWh { get; set; }
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public bool ConsiderBadWeather75PercentFactor { get; set; }
    public bool LoadBatteryTo95PercentEnabled { get; set; }
    public int LoadBatteryTo95PercentPrice { get; set; }
    public int NumberOfBatteries { get; set; }
    //Laadimiste lisa %. Popup: Antud välja % määrab kui palju laetakse akut rohkem, kui matemaatliselt on vajadus. Näiteks olukorras
    //kus aku ei hakka kohe alguses 100% kiirusega laadima, saab selle väljaga laadimisaega pikendada, et seda kompenseerida
    public double AdditionalTimeForBatteryChargingPercentage { get; set; } = 0;
    public bool CalculateBatterSocFromVolts { get; set; } = false;
    public int BatteryVoltsMin { get; set; } 
    public int BatteryVoltsMax { get; set; }
    public int BatteryMinLevelWithConsumeMax { get; set; }
    public bool UseHzMarket { get; set; } = false;
    public bool ClientUseHzMarket { get; set; } = false;
    public int? HzMarketMinBatteryLevelOnDischargeCommand { get; set; } = 0;
    public double? HzMarketDischargeMinPrice { get; set; } = 0;
    public bool AllowPurchasingFromGridInSummer { get; set; } = false;
    public bool ConsiderRemainingBatteryOnPurchase { get; set; } = false;
    public bool Enabled { get; set; } = true;
}
