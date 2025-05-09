using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models.ViewModel;

public class SensorViewModel
{
    public int Id { get; set; }

    public int? DeviceId { get; set; }
    public int? CompanyId { get; set; }

    [MaxLength(255)]
    [Required]
    public string Name { get; set; }
    [MaxLength(255)]
    public string Description { get; set; }
    [MaxLength(255)]
    public string? Topic { get; set; }
    public DeviceActionType DeviceActionType { get; set; }
    public bool BroadcastToFusebox { get; set; }
    [Required]
    public int SwitchModelId { get; set; }
}
