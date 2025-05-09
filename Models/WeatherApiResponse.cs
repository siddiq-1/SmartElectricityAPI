using System.Text.Json.Serialization;

namespace SmartElectricityAPI.Models;

public class WeatherApiResponse
{
    [JsonPropertyName("forecast")]
    public Forecast Forecast { get; set; }
}
