using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SpotPriceController : ControllerBase
{
    private MySQLDBContext _dbContext;

    public SpotPriceController(MySQLDBContext context)
    {
        _dbContext = context;
    }
    [HttpGet, Authorize]
    public async Task<ActionResult<SpotPrice>> GetPeriod([FromQuery]DateTime startDate, [FromQuery]DateTime endDate)
    {

        DateTime endDateActual = endDate == startDate ? new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59) : endDate;
        return Ok(await _dbContext.SpotPrice.Where(x => x.DateTime >= startDate && x.DateTime <= endDateActual).ToListAsync());
    }

}

public class DateRangeModel
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
