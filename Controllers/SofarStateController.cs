using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using System.Globalization;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SofarStateController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    
    public SofarStateController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }



    [HttpGet, Route("LogPeriod"), Authorize]
    public async Task<ActionResult> GetLogPeriod([FromQuery] string deviceName, [FromQuery] string minDate, [FromQuery] string maxDate)
    {
        if (_userInfo.IsAdmin)
        {
            if (string.IsNullOrEmpty(minDate) || string.IsNullOrEmpty(maxDate))
            {
                return BadRequest("Both minimum date and maximum date parameters are required.");
            }

            if (string.IsNullOrEmpty(deviceName))
            {
                return BadRequest("Device name is required.");
            }

            DateTime minDateTime;
            DateTime maxDateTime;

            if (!DateTime.TryParseExact(minDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDateTime) ||
                !DateTime.TryParseExact(maxDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDateTime))
            {
                return BadRequest("Invalid date format. Please use 'yyyy-MM-dd'.");
            }

            if (minDateTime > maxDateTime)
            {
                return BadRequest("minimum ddate cannot be greater than maximum date.");
            }
            TimeSpan difference = maxDateTime - minDateTime;
            if (difference.Days > 3)
            {
                return BadRequest("Max period range is 3 days.");
            }

            var selectedState = await _dbContext.SofarState.Where(x => x.deviceName == deviceName && x.CreatedAt >= minDateTime && x.CreatedAt <= maxDateTime).OrderBy(x => x.CreatedAt).ToListAsync();



            return Ok(selectedState);

        }

        return BadRequest();

    }


}
