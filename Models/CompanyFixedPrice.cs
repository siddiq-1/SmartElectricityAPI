using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace SmartElectricityAPI.Models;

public class CompanyFixedPrice : BaseEntity
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public double PurchasePrice { get; set; }
    public double SalesPrice { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan Time { get; set; }
}
