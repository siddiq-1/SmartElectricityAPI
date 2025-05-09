using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Models.ViewModel;

public class InverterUserViewModel
{
    public int Id { get; set; }
    public int InverterTypeId { get; set; }
    public int CompanyId { get; set; }
    public int? RegisteredInverterId { get; set; }
    public double MaxSalesPowerCapacity { get; set; }
    public double MaxPower { get; set; }
    public int FixedAvgHourlyWatts { get; set; }
    public bool UseFixedAvgHourlyWatts { get; set; }
    public bool UseOnlyCompensateMissingEnergy { get; set; }
    public bool UseWeatherForecast { get; set; } = false;
    public double SolarPanelsMaxPower { get; set; }
    public SolarPanelsDirecation SolarPanelsDirecation { get; set; }
    public CalculationFormula CalculationFormula { get; set; }
    public bool UseInverterSelfUse { get; set; } = false;
    public bool AllowPurchasingFromGridInSummer { get; set; }
    public bool PVInverterIsSeparated { get; set; } = false;
    public int NumberOfInverters { get; set; }
}
