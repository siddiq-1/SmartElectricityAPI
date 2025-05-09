using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class InverterCompanyHours : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int InverterId { get; set; }
    public Inverter Inverter { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    [MaxLength(255)]
    public ActionType ActionType { get; set; }
    public int SpotPriceId { get; set; }
    public SpotPrice SpotPrice { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? CostWithSalesMargin { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? CostWithPurchaseMargin { get; set; }

}
