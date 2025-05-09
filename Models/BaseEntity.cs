using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class BaseEntity
{
  
    [Column(TypeName = "datetime(0)")]
    public DateTime? CreatedAt { get; set; }
    [Column(TypeName = "datetime(0)")]
    public DateTime? UpdatedAt { get; set; }
}
