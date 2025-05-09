using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartElectricityAPI.Models;

public class Forecasthour
{
    [JsonProperty("cloud")]
    public int Cloud { get; set; }

    [JsonProperty("chance_of_snow")]
    public int ChanceOfSnow { get; set; }

    [JsonProperty("time")]
    public DateTime DateTime { get; set; }

    [JsonProperty("humidity")]
    public int Humidity { get; set; }

    [JsonProperty("precip_mm")]
    public double PrecipMM { get; set; }

    [JsonProperty("condition")]
    public ForecastCondition Condition { get; set; }

    [JsonProperty("uv")]
    public double Uv { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan ForecastTime => DateTime.TimeOfDay;
    public DateOnly ForecastDate => DateOnly.FromDateTime(DateTime);
    public string ConditionText => Condition.ConditionTxt;
}
