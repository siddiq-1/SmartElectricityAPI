using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RegionController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<RegionController> _logger;
    private readonly IUserInfo _userInfo;
    public RegionController(MySQLDBContext context, ILogger<RegionController> logger, IUserInfo userInfo)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
    }
    [HttpGet, Authorize]
    public IEnumerable<Region> GetRegions()
    {
        return _dbContext.Region.ToList();
    }
}
