using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class WeatherForecastData : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Inverter Inverter { get; set; }
    public int InverterId { get; set; }
    [Column(TypeName = "datetime(0)")]
    public DateTime DateTime { get; set; }
    public DateOnly Date {get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan Time { get; set; }
    public string ConditionTxt { get; set; }
    public int Humidity { get; set; }
    public double PrecipMM { get; set; }
    public int ChanceOfSnow { get; set; }
    public int Cloud { get; set; }
    public double Uv { get; set; }
    public int WeatherPercentage { get; set; }
    public int EstimatedSolarPower { get; set; }

}
