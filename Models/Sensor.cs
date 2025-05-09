using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartElectricityAPI.Models;

public class Sensor : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int? DeviceId { get; set; }
    public Device? Device { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    [MaxLength(255)]
    [Required]
    public string Name { get; set; }
    [MaxLength(255)]
    public string Description { get; set; }
    [MaxLength(255)]
    public string? Topic { get; set; }

    public DeviceActionType DeviceActionType { get; set; }
    public bool BroadcastToFusebox { get; set; } = false;
   
    public SwitchModel SwitchModel { get; set; }
    [Required]
    public int SwitchModelId { get; set; }

}
