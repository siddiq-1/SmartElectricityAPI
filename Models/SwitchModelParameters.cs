using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Models;

public class SwitchModelParameters : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int SwitchModelId { get; set; }
    public SwitchModel SwitchModel { get; set; }

    [MaxLength(255)]
    public string? Payload { get; set; }
    public DeviceActionType DeviceActionType { get; set; }
}
