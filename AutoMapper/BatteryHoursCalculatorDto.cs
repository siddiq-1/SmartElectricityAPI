using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.AutoMapper;

public class BatteryHoursCalculatorDto : SpotPrice
{
    public int AvgHourlyConsumption { get; set; }
    public int ChargingPowerWh { get; set; }
    public int? AmountCharged { get; set; }
    public double? CostWithSalesMargin { get; set; }
}
