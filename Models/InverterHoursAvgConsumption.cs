using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class InverterHoursAvgConsumption : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public RegisteredInverter RegisteredInverter { get; set; }
    public int RegisteredInverterId { get; set; }
    public Company Company { get; set; }
    public int CompanyId { get; set; }
    public Inverter Inverter { get; set; }
    public int InverterId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan TimeHour { get; set; }
    public int AvgHourlyConsumption { get; set; }
    public DateOnly DateCalculated { get; set; }
}
