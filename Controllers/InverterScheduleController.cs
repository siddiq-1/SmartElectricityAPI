using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using System;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InverterScheduleController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    public InverterScheduleController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }
    [HttpGet, Authorize]
    public async Task<ActionResult> GetInverterPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int inverterId)
    {

        DateTime endDateActual =  new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59);

        var inverterBattery = _dbContext.InverterBattery.FirstOrDefault(x=> x.InverterId == inverterId);
        var weatherForecastData = _dbContext.WeatherForecastData.Any(x => x.InverterId == inverterId && x.DateTime >= startDate && x.DateTime <= endDateActual);
        //TODO: check if spot prices exist and inverter company hours
        if (inverterBattery != null && weatherForecastData)
        {
            var joinedResult = _dbContext.SpotPrice
            .Where(x => x.DateTime >= startDate && x.DateTime <= endDateActual)
            .Join(
                _dbContext.InverterCompanyHours.Where(x => x.InverterId == inverterId),
                spotPrice => spotPrice.Id,
                inverterCompanyHours => inverterCompanyHours.SpotPriceId,
                (spotPrice, inverterCompanyHours) => new { SpotPrice = spotPrice, InverterCompanyHours = inverterCompanyHours }
            )
            .SelectMany(
            previousJoin => _dbContext.BatteryControlHours
                                .Where(bch => bch.InverterBatteryId == inverterBattery.Id && bch.SpotPriceMaxId == previousJoin.SpotPrice.Id)
                                .DefaultIfEmpty(),
            (previousJoin, batteryControlHours) => new
            {
                previousJoin.SpotPrice,
                previousJoin.InverterCompanyHours,
                BatteryControlHours = batteryControlHours
            })
             .SelectMany(
            previousJoin => _dbContext.WeatherForecastData
                                .Where(bch => bch.InverterId == previousJoin.InverterCompanyHours.InverterId
                                && bch.DateTime == previousJoin.SpotPrice.DateTime)
                                .DefaultIfEmpty(),
            (previousJoin, weatherForecastData) => new
            {
                previousJoin.SpotPrice,
                previousJoin.InverterCompanyHours,
                previousJoin.BatteryControlHours,
                WeatherForecastData = weatherForecastData
            })
            .Where(x => _userInfo.Companies.Contains(x.InverterCompanyHours.CompanyId))
            .OrderBy(o => o.SpotPrice.DateTime)
            .ToList();


            return Ok(joinedResult);
        }
        else if (inverterBattery != null && !weatherForecastData)
        {
            var joinedResult = _dbContext.SpotPrice
                 .Where(x => x.DateTime >= startDate && x.DateTime <= endDateActual)
                 .Join(
                     _dbContext.InverterCompanyHours.Where(x => x.InverterId == inverterId),
                     spotPrice => spotPrice.Id,
                     inverterCompanyHours => inverterCompanyHours.SpotPriceId,
                     (spotPrice, inverterCompanyHours) => new { SpotPrice = spotPrice, InverterCompanyHours = inverterCompanyHours }
                 )
                 .SelectMany(
                 previousJoin => _dbContext.BatteryControlHours
                                     .Where(bch => bch.InverterBatteryId == inverterBattery.Id && bch.SpotPriceMaxId == previousJoin.SpotPrice.Id)
                                     .DefaultIfEmpty(),
                 (previousJoin, batteryControlHours) => new
                 {
                     SpotPrice = previousJoin.SpotPrice,
                     InverterCompanyHours = previousJoin.InverterCompanyHours,
                     BatteryControlHours = batteryControlHours
                 })
                 .Where(x => _userInfo.Companies.Contains(x.InverterCompanyHours.CompanyId))
                 .OrderBy(o => o.SpotPrice.DateTime)
                 .ToList();

                  return Ok(joinedResult);
        }
        else
        {
            var joinedResult = _dbContext.SpotPrice
                .Where(x => x.DateTime >= startDate && x.DateTime <= endDateActual)
                .Join(
                    _dbContext.InverterCompanyHours.Where(x => x.InverterId == inverterId),
                    spotPrice => spotPrice.Id,
                    inverterCompanyHours => inverterCompanyHours.SpotPriceId,
                    (spotPrice, inverterCompanyHours) => new { SpotPrice = spotPrice, InverterCompanyHours = inverterCompanyHours }
                )
                .Where(x => _userInfo.Companies.Contains(x.InverterCompanyHours.CompanyId))
                .OrderBy(o => o.SpotPrice.DateTime)
                .ToList();

            return Ok(joinedResult);
        }
    }

    [HttpGet, Route("TotalSolarValue")]
    public async Task<ActionResult> GetTotalSolarValue(int inverterId , DateTime fromDate, DateTime toDate)
    {
        var result = Convert.ToDouble(await _dbContext.WeatherForecastData.Where(x => x.InverterId == inverterId && x.DateTime.Date >= fromDate.Date && x.DateTime.Date <= toDate.Date).SumAsync(x => x.EstimatedSolarPower));
        return Ok(Math.Round(result > 0.0d ? (result / 1000.00) : 0.0, 2));
    }

    [HttpGet,Route("InverterMaxDate"), Authorize]
    public async Task<ActionResult> GetInverterMaxDate([FromQuery] int inverterId)
    {
        var joinedResult = _dbContext.InverterCompanyHours.Where(x => x.InverterId == inverterId && _userInfo.Companies.Contains(x.CompanyId))
            .Join(_dbContext.SpotPrice,
            inverterCompanyHours => inverterCompanyHours.SpotPriceId,
            spotPrice => spotPrice.Id,
            (inverterCompanyHours, spotPrice) => new
            {
                SpotPrice = spotPrice,
                InverterCompanyHours = inverterCompanyHours
            }).OrderByDescending(x => x.SpotPrice.Date).Select(x => x.SpotPrice).Take(1).FirstOrDefault();

        return Ok(joinedResult);
    }


    [HttpGet, Route("InverterMinDate"), Authorize]
    public async Task<ActionResult> GetInverterMinDate([FromQuery] int inverterId)
    {
        var joinedResult = _dbContext.InverterCompanyHours.Where(x => x.InverterId == inverterId && _userInfo.Companies.Contains(x.CompanyId))
            .Join(_dbContext.SpotPrice,
            inverterCompanyHours => inverterCompanyHours.SpotPriceId,
            spotPrice => spotPrice.Id,
            (inverterCompanyHours, spotPrice) => new
            {
                SpotPrice = spotPrice,
                InverterCompanyHours = inverterCompanyHours
            }).OrderBy(x => x.SpotPrice.Date).Select(x => x.SpotPrice).Take(1).FirstOrDefault();

        return Ok(joinedResult);
    }


}
