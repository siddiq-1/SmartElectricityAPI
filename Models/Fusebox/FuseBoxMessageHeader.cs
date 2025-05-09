using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Models.Fusebox;

public class FuseBoxMessageHeader : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Int64 m_id { get; set; }
    public Int64? m_orig_id { get; set; }

    public FuseBoxMessageType m_type { get; set; }
    public string device_id { get; set; }
}
