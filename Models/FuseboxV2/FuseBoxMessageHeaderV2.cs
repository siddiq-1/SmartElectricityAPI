using Newtonsoft.Json;

namespace SmartElectricityAPI.Models.FuseboxV2;
public class FuseBoxMessageHeaderV2
{
    [JsonProperty("meta")]
    public Meta meta { get; set; }
    [JsonProperty("body")]
    public Body body { get; set; }
}

public class Meta
{
    [JsonProperty("m_id")]
    public int m_id { get; set; }
    [JsonProperty("v")]
    public int v { get; set; }
    [JsonProperty("m_type")]
    public string m_type { get; set; }
    [JsonProperty("device_id")]
    public string device_id { get; set; }
}

public class Body
{
    [JsonProperty("cancel")]
    public bool cancel { get; set; }
    [JsonProperty("pow_set")]
    public PowSet pow_set { get; set; }
    [JsonProperty("start")]
    public long? start { get; set; }
    [JsonProperty("end")]
    public long? end { get; set; }
}

public class PowSet
{
    [JsonProperty("BipolarSetpoint")]
    public int BipolarSetpoint { get; set; }
}