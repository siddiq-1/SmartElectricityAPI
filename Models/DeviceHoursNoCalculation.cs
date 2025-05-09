using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class DeviceHoursNoCalculation : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; }
    public TimeSpan Time { get; set; }
}
