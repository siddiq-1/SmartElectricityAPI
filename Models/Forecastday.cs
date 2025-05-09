using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SmartElectricityAPI.Models;

public class Forecastday
{
    [JsonProperty("date")]
    public DateOnly Date { get; set; }
    [JsonProperty("hour")]
    public List<Forecasthour> Forecasthour { get; set; }
}
