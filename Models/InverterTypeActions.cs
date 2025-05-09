using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class InverterTypeActions : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public InverterType InverterType { get; set; }
    public int InverterTypeId { get; set; }
    public ActionType ActionType { get; set; }
    public ActionTypeCommand ActionTypeCommand { get; set; }
    public string ActionName { get; set; }
    public int OrderSequence { get; set; }
    [MaxLength(30)]
    public string ButtonBorderColor { get; set; } = string.Empty;
    public bool IsClickable { get; set; } = true;
}
