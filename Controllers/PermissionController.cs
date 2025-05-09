using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PermissionController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<PermissionController> _logger;
    private readonly IUserInfo _userInfo;
    public PermissionController(MySQLDBContext context, ILogger<PermissionController> logger, IUserInfo userInfo)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
    }
    [HttpGet, Authorize]
    public IEnumerable<Permission> GetPermissions()
    {
        return _dbContext.Permission.ToList();
    }
}
