using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Dto;

public class InverterTypeCompanyActionsDto
{
    public int Id { get; set; }
    public Company Company { get; set; }

    public int CompanyId { get; set; }
    public Inverter Inverter { get; set; }
    public int InverterId { get; set; }
    public InverterTypeActions InverterTypeActions { get; set; }

 
    public int InverterTypeActionsId { get; set; }
    public InverterType InverterType { get; set; }
    public int InverterTypeId { get; set; }
    public ActionType ActionType { get; set; }
    public ActionTypeCommand ActionTypeCommand { get; set; }
    public string ActionName { get; set; }
    public bool ActionState { get; set; }
    public bool ButtonEnabled { get; set; } = true;

}
