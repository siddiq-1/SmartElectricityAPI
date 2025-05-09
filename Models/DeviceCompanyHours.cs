using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class DeviceCompanyHours : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    [MaxLength(255)]
    public DeviceActionType DeviceActionType { get; set; }
    public int SpotPriceId { get; set; }
    public SpotPrice SpotPrice { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public double? CostWithSalesMargin { get; set; }
    public bool IsProcessed { get; set; } = false;
}
