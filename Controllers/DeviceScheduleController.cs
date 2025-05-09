using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DeviceScheduleController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;

    public DeviceScheduleController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }
    [HttpGet, Authorize]
    public async Task<ActionResult> GetDevicePeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int deviceId)
    {

        var selectedCompany = await _dbContext.Company.FirstOrDefaultAsync(c => c.Id == _userInfo.SelectedCompanyId);


        DateTime endDateActual = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

        var deviceCompanyHours = await _dbContext.DeviceCompanyHours
            .Where(dch => dch.DeviceId == deviceId && dch.SpotPrice.DateTime >= startDate && dch.SpotPrice.DateTime <= endDateActual)
            .Join(
                _dbContext.SpotPrice,
                dch => dch.SpotPriceId,
                sp => sp.Id,
                (dch, sp) => new { DeviceCompanyHours = dch, SpotPrice = sp }
            )
            .ToListAsync();

        var allSpotPrices = await _dbContext.SpotPrice
            .Where(sp => sp.DateTime >= startDate && sp.DateTime <= endDateActual && sp.RegionId == selectedCompany.RegionId)
            .ToListAsync();


        var result = allSpotPrices.GroupJoin(
            deviceCompanyHours,
            sp => sp.Id,
            dch => dch.SpotPrice.Id,
            (sp, dchGroup) => new
            {
                SpotPrice = sp,
                DeviceCompanyHours = dchGroup.FirstOrDefault()?.DeviceCompanyHours ?? new DeviceCompanyHours
                {
                    DeviceId = deviceId,
                    SpotPriceId = sp.Id,
                    DeviceActionType = DeviceActionType.None
                },
            
            }
        )
        .OrderBy(x => x.SpotPrice.DateTime)
        .ToList();

        return Ok(result);
    }

    [HttpGet, Route("DeviceMaxDate"), Authorize]
    public async Task<ActionResult> GetInverterMaxDate([FromQuery] int deviceId)
    {
        var joinedResult = await _dbContext.DeviceCompanyHours.Where(x => x.DeviceId == deviceId && _userInfo.Companies.Contains(x.CompanyId))
            .Join(_dbContext.SpotPrice,
            deviceCompanyHours => deviceCompanyHours.SpotPriceId,
            spotPrice => spotPrice.Id,
            (deviceCompanyHours, spotPrice) => new
            {
                SpotPrice = spotPrice,
                deviceCompanyHours
            }).OrderByDescending(x => x.SpotPrice.Date).Select(x => x.SpotPrice).Take(1).FirstOrDefaultAsync();

        return Ok(joinedResult);
    }
    [HttpGet, Route("DeviceMinDate"), Authorize]
    public async Task<ActionResult> GetInverterMinDate([FromQuery] int deviceId)
    {
        var joinedResult = await _dbContext.DeviceCompanyHours.Where(x => x.DeviceId == deviceId && _userInfo.Companies.Contains(x.CompanyId))
            .Join(_dbContext.SpotPrice,
            deviceCompanyHours => deviceCompanyHours.SpotPriceId,
            spotPrice => spotPrice.Id,
            (deviceCompanyHours, spotPrice) => new
            {
                SpotPrice = spotPrice,
                 deviceCompanyHours
            }).OrderBy(x => x.SpotPrice.Date).Select(x => x.SpotPrice).Take(1).FirstOrDefaultAsync();

        return Ok(joinedResult);
    }
}
