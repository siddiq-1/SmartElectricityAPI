using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Dto;

public class SofarState : BaseEntity, IDisposable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string deviceName { get; set; }
    public int running_state { get; set; }
    public double inverter_temp { get; set; }
    public double inverter_HStemp { get; set; }
    public int inverter_power { get; set; }
    public int grid_power { get; set; }
    public int consumption { get; set; }
     public double solarPV1Current { get; set; }
    public int solarPV1 { get; set; }
    public double solarPV2Current { get; set; }
    public int solarPV2 { get; set; }
    public int solarPV { get; set; }
    public double battery_voltage { get; set; }
    public double battery_current { get; set; }
    public double battery_power { get; set; }
    public double battery_temp { get; set; }
    public int batterySOC { get; set; }
    public double battery_cycles { get; set; }
    public double today_generation { get; set; }
    public double total_generation { get; set; }
    public double today_consumption { get; set; }
    public double total_consumption { get; set; }
    public double today_purchase { get; set; }
    public double total_purchase { get; set; }
    public double today_exported { get; set; }
    public double total_exported { get; set; }
    public double today_charged { get; set; }
    public double total_charged { get; set; }
    public double today_discharged { get; set; }
    public double total_discharged { get; set; }
    public RegisteredInverter RegisteredInverter { get; set; }
    public int? RegisteredInverterId { get; set; }
    public Company Company { get; set; }
    public int? CompanyId { get; set; }
    public DateOnly? Date {  get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan TimeHour { get; set; }

    [NotMapped]
    public double UsableBatteryEnergy { get; set; }

    public void Dispose()
    {
        
    }
}
