namespace SmartElectricityAPI.Models.ViewModel;

public class UpdateUserViewModel
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    public int? SelectedCompanyId { get; set; }
    public string? ClientId { get; set; }
}
