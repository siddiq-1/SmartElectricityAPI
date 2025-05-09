using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartElectricityAPI.Models;

public class Switch : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }  
    public int? DeviceId { get; set; }
    public Device? Device { get; set; }
    public int ActionWaitTimeInSeconds { get; set; }
    public DeviceActionType DeviceActionType { get; set; }
    [Required]
    public int SwitchModelId { get; set; }
    public SwitchModel SwitchModel { get; set; }
    [Required]
    public Sensor Sensor { get; set; }
    public int? SensorId { get; set; }
    public bool ActionState { get; set; } = false;

    [Column(TypeName = "datetime(0)")]
    public DateTime? DeletedAt { get; set; } = null;
}
