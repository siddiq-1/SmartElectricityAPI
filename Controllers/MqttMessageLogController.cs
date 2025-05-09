using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using System.Globalization;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class MqttMessageLogController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;

    public MqttMessageLogController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }

    //getlogperiod like meqttmessagelogcontroller

    [HttpGet, Route("LogPeriod"), Authorize]
    public async Task<ActionResult> GetLogPeriod([FromQuery] string minDate, [FromQuery] string maxDate)
    {

        if (string.IsNullOrEmpty(minDate) || string.IsNullOrEmpty(maxDate))
        {
            return BadRequest("Both minimum date and maximum date parameters are required.");
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
        if (difference.Days > 14)
        {
            return BadRequest("Max period range is 14 days.");
        }
        var inverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.CompanyId == _userInfo.SelectedCompanyId);

        if (inverter == null)
        {
            return NotFound("Inverter not found for the specified company.");
        }

        var selectedLogs = await _dbContext.MqttMessageLog.Where(x => x.InverterId == inverter.Id && x.CreatedAt >= minDateTime && x.CreatedAt <= maxDateTime).OrderBy(x => x.CreatedAt).ToListAsync();

        foreach (var log in selectedLogs)
        {
            int lastIndex = log.Topic.LastIndexOf('/');

            if (lastIndex != -1)
            {
                // Extract substring starting from the index after the last '/'
                log.Topic = log.Topic.Substring(lastIndex + 1);
            }
        }

        return Ok(selectedLogs);
    }


}
