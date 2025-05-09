using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class InverterTypeCompanyActions: BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Company Company { get; set; }
    [Required]
    public int CompanyId { get; set; }
    public Inverter Inverter { get; set; }
    public int InverterId { get; set; }
    public InverterTypeActions InverterTypeActions { get; set; }

    [Required]
    public int InverterTypeActionsId { get; set; }
    public InverterType InverterType { get; set; }
    public int InverterTypeId { get; set; }
    public ActionType ActionType { get; set; }
    public ActionTypeCommand ActionTypeCommand { get; set; }
    public string ActionName { get; set; }
    public bool ActionState { get;set; }
}
