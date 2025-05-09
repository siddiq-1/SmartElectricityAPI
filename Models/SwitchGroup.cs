using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class SwitchGroup : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int CompanyId { get; set; }
    public Company Company { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
}
