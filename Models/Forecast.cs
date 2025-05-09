using System.Text.Json.Serialization;

namespace SmartElectricityAPI.Models;

public class Forecast
{
    [JsonPropertyName("forecastday")]
    public List<Forecastday> Forecastday { get; set; }
}
