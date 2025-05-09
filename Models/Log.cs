using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class Log : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Level { get; set; }

    [MaxLength(784*3)]
    public string Message { get; set; }
    [MaxLength(256)]
    public string? Origin { get; set; }

}
