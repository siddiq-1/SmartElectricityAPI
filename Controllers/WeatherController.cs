using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    public WeatherController(MySQLDBContext context, ILogger<CompanyController> logger, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }


    [HttpGet, Authorize]
    public async Task<ActionResult> GetWeatherPeriod([FromQuery] string weatherDate, [FromQuery] int inverterId)
    {
 
        var inverter = _dbContext.Inverter.FirstOrDefault(x => x.Id == inverterId);
        DateOnly WeatherDate = DateOnly.FromDateTime(DateTime.Parse(weatherDate));


        if (inverter.CompanyId == _userInfo.SelectedCompanyId || _userInfo.IsAdmin)
        {
            if (inverter != null)
            {
                var joinedResult = _dbContext.WeatherForecastData.Where(x => x.InverterId == inverterId && x.Date >= WeatherDate).OrderBy(x=> x.DateTime).ToList();

                return Ok(joinedResult);
            }
        }

        return NotFound();
    }
}
