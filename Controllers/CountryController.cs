using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CountryController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<CountryController> _logger;
    private readonly IUserInfo _userInfo;
    public CountryController(MySQLDBContext context, ILogger<CountryController> logger, IUserInfo userInfo)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
    }
    [HttpGet, Authorize]
    public async Task<IEnumerable<Country>> GetCountries()
    {
        return await _dbContext.Country.ToListAsync();
    }
}
