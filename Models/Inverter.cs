using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class Inverter : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
 
    [Required]
    public int InverterTypeId { get; set; }
    public InverterType InverterType { get; set; }
    [Required]
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public int? RegisteredInverterId { get; set; }
    public RegisteredInverter RegisteredInverter { get; set; } 
    public double MaxSalesPowerCapacity { get; set; }
    public double MaxPower { get; set; }
    public List<InverterBattery> InverterBattery { get; set; } = new List<InverterBattery>();
    public int FixedAvgHourlyWatts { get; set; }
    public bool UseFixedAvgHourlyWatts { get; set; } = false;
    public bool UseOnlyCompensateMissingEnergy { get; set; } = false;
    public bool UseWeatherForecast { get; set; } = false;
    public double SolarPanelsMaxPower { get; set; }
    public SolarPanelsDirecation SolarPanelsDirecation { get; set; }
    public CalculationFormula CalculationFormula { get; set; } = CalculationFormula.Winter;
    public bool UseInverterSelfUse { get; set; } = false;
    public bool AllowPurchasingFromGridInSummer { get; set; } = false;
    public bool PVInverterIsSeparated { get; set; } = false;
    [NotMapped]
    public double FuseboxDataInterval { get; set; }
    public int NumberOfInverters { get; set; } = 1;
}
