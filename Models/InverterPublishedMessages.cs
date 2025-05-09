namespace SmartElectricityAPI.Models;

public class InverterPublishedMessages : BaseEntity
{
    public int Id { get; set; } 
    public RegisteredInverter RegisteredInverter { get; set; }
    public int RegisteredInverterId { get; set; }
    public string? Topic { get; set; }   
    public string? Message { get; set; }


}
