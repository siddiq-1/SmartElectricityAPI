using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartElectricityAPI.Models.Fusebox;

public class FuseBoxSchedRegMsg : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int FuseBoxMessageHeaderId { get; set; }
    public FuseBoxMessageHeader FuseBoxMessageHeader { get; set; }
    public long? start { get; set; }
    public long? end { get; set; }
    public long? actualStart { get; set; }
    public long? actualEnd { get; set; }
    public bool cancel { get; set; } = false;
    public int FuseBoxPowSetId { get; set; }
    public FuseBoxPowSet FuseBoxPowSet { get; set; }

}
