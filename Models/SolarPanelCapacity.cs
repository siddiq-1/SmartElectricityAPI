using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Models;

public class SolarPanelCapacity :BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan Time { get; set; }
    public int PanelTilt { get; set; }
    public SolarPanelsDirecation SolarPanelsDirecation { get; set; }
    public double MaxPercentage { get; set; }

}
