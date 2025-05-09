namespace SmartElectricityAPI.Models.ViewModel;

public class AddCompanyUserViewModel
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Email { get; set; }
    public int PermissionId { get; set; }   
}
