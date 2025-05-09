namespace SmartElectricityAPI.Models;

public class ConsumerPriceV2
{
    public DateTime DeliveryStart { get; set; }
    public DateTime DeliveryEnd { get; set; }
    public Dictionary<string, double> EntryPerArea { get; set; }
}

public class NordPoolPriceResponse
{
    public string DeliveryDateCET { get; set; }
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> DeliveryAreas { get; set; }
    public string Market { get; set; }
    public List<MultiAreaEntry> MultiAreaEntries { get; set; }
}

public class MultiAreaEntry
{
    public DateTime DeliveryStart { get; set; }
    public DateTime DeliveryEnd { get; set; }
    public Dictionary<string, double> EntryPerArea { get; set; }
}
