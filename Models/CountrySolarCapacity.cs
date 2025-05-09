using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class CountrySolarCapacity : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int CountryId { get; set; }
    public Country? Country { get; set; }
    public int Month { get; set; }
    public double SolarCapacity { get; set; }

}
