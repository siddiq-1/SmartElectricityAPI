namespace SmartElectricityAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("SofarStateHourlyTemp")]

public class SofarStateHourlyTemp : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string deviceName { get; set; }

    [Description("Inventer Min temp Current hour C")]
    public double inverter_tempMin { get; set; }

    [Description("Inventer Max temp Current hour C")]
    public double inverter_tempMax { get; set; }

    [Description("Only for statistic: average in current hour inverter power W")]
    public int inverter_power { get; set; }

    [Description("Only for statistic: average in current hour Grid Power W")]
    public int grid_power { get; set; }

    [Description("Only for statistic: average in current hour Consumption W")]
    public int consumption { get; set; }

    [Description("Only for statistic: average in current hour Solar W")]
    public int solarPV { get; set; }

    [Description("Only for statistic: average in current hour Battery V")]
    public double battery_voltage { get; set; }

    [Description("Only for statistic: average in current hour Battery A")]
    public double battery_current { get; set; }

    [Description("Only for statistic: average in current hour Battery (W)")]
    public double battery_power { get; set; }

    [Description("Min Bat temp C")]
    public double battery_tempMin { get; set; }

    [Description("Max Bat temp C")]
    public double battery_tempMax { get; set; }

    [Description("Battery SOC in end of hour %")]
    public int batterySOC { get; set; }

    [Description("This hour generated Kw")]
    public double hour_generation { get; set; }

    [Description("Today generated Kw")]
    public double today_generation { get; set; }

    [Description("This hour consumption Kw")]
    public double hour_consumption { get; set; }

    [Description("Today consumption Kw")]
    public double today_consumption { get; set; }

    [Description("Total generated Kw")]
    public double total_generation { get; set; }

    [Description("Total consumption Kw")]
    public double total_consumption { get; set; }

    [Description("This hour purchased Kw")]
    public double hour_purchase { get; set; }

    [Description("Today purchased Kw")]
    public double today_purchase { get; set; }

    [Description("Total purchased Kw")]
    public double total_purchase { get; set; }

    [Description("This hour exported Kw")]
    public double hour_exported { get; set; }

    [Description("Today exported Kw")]
    public double today_exported { get; set; }

    [Description("Total exported Kw")]
    public double total_exported { get; set; }

    [Description("This hour Charged Kw")]
    public double hour_charged { get; set; }

    [Description("Today Charged Kw")]
    public double today_charged { get; set; }

    [Description("Total Charged Kw")]
    public double total_charged { get; set; }

    [Description("This hour Discharged Kw")]
    public double hour_discharged { get; set; }

    [Description("Today Discharged Kw")]
    public double today_discharged { get; set; }

    [Description("Total Discharged Kw")]
    public double total_discharged { get; set; }

    [Description("Start time")]
    [Column(TypeName = "datetime(0)")]
    public DateTime startTime { get; set; }

    [Description("End time")]
    [Column(TypeName = "datetime(0)")]
    public DateTime endTime { get; set; }
    public RegisteredInverter RegisteredInverter { get; set; }
    public int? RegisteredInverterId { get; set; }
    public Company Company { get; set; }
    public int? CompanyId { get; set; }
    public DateOnly? Date { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan TimeHour { get; set; }
    public int NoOfGroupedTransactions { get; set; }
}
