using SmartElectricityAPI.Enums;
using System.Security.Claims;

namespace SmartElectricityAPI.Interfaces;

public interface IUserInfo
{
    public int Id { get; set; }
    List<int> Companies { get; set; }
    Level UserLevel { get;set; }
    bool IsAdmin { get; set; }
    public int? SelectedCompanyId { get; set; }
}
