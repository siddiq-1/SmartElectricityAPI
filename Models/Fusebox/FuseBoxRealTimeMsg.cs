using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models.Fusebox;

public class FuseBoxRealTimeMsg : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int FuseBoxMessageHeaderId { get; set; }
    public FuseBoxMessageHeader FuseBoxMessageHeader { get; set; }
    public long ts { get; set; }
    public int exp { get; set; }
    public int FuseBoxPowSetId { get; set; }
    public FuseBoxPowSet FuseBoxPowSet { get; set; }




}
