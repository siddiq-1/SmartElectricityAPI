namespace SmartElectricityAPI.Dto;

public class RegisteredInverterTopicDto
{
    public int RegisteredInverterId { get; set; }
    public string RegisteredInverterName { get; set; }
    public string TopicName { get; set; }
    public string RegisteredInverterAndTopic { get; set; }
    public int CompanyId { get; set; }
}
