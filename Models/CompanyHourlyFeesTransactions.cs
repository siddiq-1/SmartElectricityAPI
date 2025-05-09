using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class CompanyHourlyFeesTransactions : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    [Column(TypeName = "time(0)")]
    public TimeSpan Time { get; set; }

    [Required]
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public double NetworkServiceFree { get; set; }
    public double BrokerServiceFree { get; set; }
   
}
