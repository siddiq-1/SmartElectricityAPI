using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class InverterTypeCommands : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int InverterTypeId { get; set; }
    public InverterType InverterType { get; set; }
    [MaxLength(384)]
    public string MqttTopic { get; set; }
    [MaxLength(255)]
    public ActionType ActionType { get; set; }
    public bool IsPayloadFixed { get; set; }

}
