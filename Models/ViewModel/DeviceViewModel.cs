
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models.ViewModel;

public class DeviceViewModel
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public bool FuseboxForcedOff { get; set; }
    public bool FuseboxForcedOn { get; set; }
    public double MaxStopHoursIn24h { get; set; }
    public double MaxStopHoursConsecutive { get; set; }
    public double MaxForcedOnHoursIn24h { get; set; }
    public double ForcedOnPercentageForComingHourToEnable { get; set; }
    public bool ForcedOn { get; set; }
    public bool ForcedOff { get; set; }
    public bool MediumOn { get; set; }
    public double? TemperatureInStandardMode { get; set; }
    public double? TemperatureInForcedOnMode { get; set; }
    public double FirstHourPercentageKwPriceRequirementBeforeHeating { get; set; }
    public int CompanyId { get; set; }
    public bool AutoModeEnabled { get; set; } = false;
    public SensorViewModel? sensorViewModel { get; set; }
}
