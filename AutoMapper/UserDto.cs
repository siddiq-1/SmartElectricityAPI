using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.AutoMapper;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public List<Permission> Permission { get; set; }
    public List<CompanyDto> CompanyIds { get; set; }
}
