using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models;

public class Region : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [MaxLength(255)]
    public string Abbreviation { get; set; }
    public int OffsetHoursFromEstonianTime { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }



}
