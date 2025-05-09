using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartElectricityAPI.Models.Fusebox;

public class FuseBoxPowSet : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int FuseBoxMessageHeaderId { get; set; }
    public FuseBoxMessageHeader FuseBoxMessageHeader { get; set; }
    public string BipolarControl { get; set; }
    public int PowerValue { get; set; }
}
