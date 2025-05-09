using Newtonsoft.Json;

namespace SmartElectricityAPI.Models;

public class ForecastCondition
{
    [JsonProperty("text")]
    public string ConditionTxt { get; set; }
}
