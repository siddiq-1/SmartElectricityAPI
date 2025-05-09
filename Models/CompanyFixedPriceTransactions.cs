namespace SmartElectricityAPI.Models;

public class CompanyFixedPriceTransactions : BaseEntity
{
    public int Id { get; set; }
    public int CompanyFixedPriceId { get; set; }
    public CompanyFixedPrice CompanyFixedPrice { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public double PurchasePrice { get; set; }
    public double SalesPrice { get; set; }
    public DateTime DateTime { get; set; }
    public DateOnly Date => DateOnly.FromDateTime(DateTime);
    public TimeSpan Time => DateTime.TimeOfDay;
}
