using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using System.Security.Claims;

namespace SmartElectricityAPI.Middleware;

public class UserData : IUserInfo
{
    public int Id { get; set; }
    public List<int> Companies { get; set; }
    public Level UserLevel { get; set; }
    public bool IsAdmin { get; set; }
    public int? SelectedCompanyId { get; set; }
}
