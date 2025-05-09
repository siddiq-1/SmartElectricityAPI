using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class InverterCompanyCommandsPayload : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int InverterTypeCommandsId { get; set; }
    public InverterTypeCommands InverterTypeCommands { get; set; }

    [Required]
    public int InverterId { get; set; }
    public Inverter Inverter { get; set; }

    [MaxLength(512)]
    public string Payload { get; set; }



}
