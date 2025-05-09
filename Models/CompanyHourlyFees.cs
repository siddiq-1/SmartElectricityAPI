using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class CompanyHourlyFees : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan Time { get; set; }

    [Required]
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public double NetworkServiceFee { get; set; } 
    public double BrokerServiceFee { get; set; }  

}
