using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models.ViewModel;
using System.Globalization;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BatteryControlHoursController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<BatteryControlHoursController> _logger;
    private readonly IUserInfo _userInfo;

    public BatteryControlHoursController(MySQLDBContext context, ILogger<BatteryControlHoursController> logger, IUserInfo userInfo)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
    }


    [HttpGet, Authorize]
    public async Task<ActionResult> GetBatteryControlHours([FromQuery] string minDate, [FromQuery] string maxDate)
    {

        if (string.IsNullOrEmpty(minDate) || string.IsNullOrEmpty(maxDate))
        {
            return BadRequest("Both minimum date and maximum date parameters are required.");
        }

        DateTime minDateTime;
        DateTime maxDateTime;

        if (!DateTime.TryParseExact(minDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDateTime) ||
            !DateTime.TryParseExact(maxDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDateTime))
        {
            return BadRequest("Invalid date format. Please use 'yyyy-MM-dd'.");
        }

        if (minDateTime > maxDateTime)
        {
            return BadRequest("minimum ddate cannot be greater than maximum date.");
        }

        var inverterWithBattery = await _dbContext.Inverter.Where(x => x.CompanyId == _userInfo.SelectedCompanyId).Include(x => x.InverterBattery).FirstOrDefaultAsync();

        if (inverterWithBattery == null)
        {
            return NotFound();
        }

        if (inverterWithBattery.InverterBattery.Count == 0)
        {
            return NotFound();
        }

        var batteryControlHours = await _dbContext.BatteryControlHours
            .Where(x => x.InverterBatteryId == inverterWithBattery.InverterBattery.FirstOrDefault()!.Id)
            .Include(x => x.SpotPriceMax)
            .Where(x => x.SpotPriceMax.Date >= DateOnly.FromDateTime(minDateTime) && x.SpotPriceMax.Date <= DateOnly.FromDateTime(maxDateTime))
            .Join(
                _dbContext.InverterCompanyHours.Where(x => x.InverterId == inverterWithBattery.Id),
                bch => bch.SpotPriceMaxId,
                ich => ich.SpotPriceId,
                (bch, ich) => new { BatteryControlHours = bch, InverterCompanyHours = ich }
            )
            .Where(x => _userInfo.Companies.Contains(x.InverterCompanyHours.CompanyId))
            .OrderBy(x => x.BatteryControlHours.SpotPriceMax.DateTime)
            .ToListAsync();

        if (batteryControlHours == null)
        {
            return NotFound();
        }

        return Ok(batteryControlHours);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateControlHour(int id, [FromBody] ActionTypeCommand actionType)
    {

        var inverterWithBattery = await _dbContext.Inverter.Where(x => x.CompanyId == _userInfo.SelectedCompanyId).Include(x => x.InverterBattery).FirstOrDefaultAsync();

        if (inverterWithBattery == null)
        {
            return NotFound();
        }

        if (inverterWithBattery.InverterBattery == null)
        {
            return NotFound();
        }


        var batteryControlHours = await _dbContext.BatteryControlHours.
            FirstOrDefaultAsync(x => x.InverterBatteryId == inverterWithBattery!.InverterBattery.FirstOrDefault()!.Id && x.Id == id);
        
        
        if (batteryControlHours == null)
        {
            return BadRequest();
        }

        switch (actionType)
        {
            case ActionTypeCommand.ChargeWithRemainingSun:

                batteryControlHours.MaxAvgHourlyConsumptionOriginal = 0;
                batteryControlHours.MinChargingPowerWhOriginal = 0;
                batteryControlHours.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;

                _dbContext.Entry(batteryControlHours).State = EntityState.Modified;

                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok(batteryControlHours);
                }
                catch (DbUpdateConcurrencyException)
                {

                    return BadRequest();

                }

            case ActionTypeCommand.SellRemainingSunNoCharging:

                batteryControlHours.MaxAvgHourlyConsumptionOriginal = 0;
                batteryControlHours.MinChargingPowerWhOriginal = 0;
                batteryControlHours.ActionTypeCommand = ActionTypeCommand.SellRemainingSunNoCharging;

                _dbContext.Entry(batteryControlHours).State = EntityState.Modified;

                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok(batteryControlHours);
                }
                catch (DbUpdateConcurrencyException)
                {

                    return BadRequest();

                }

            case ActionTypeCommand.ChargeMax:
                batteryControlHours.MaxAvgHourlyConsumptionOriginal = 0;
                batteryControlHours.MinChargingPowerWhOriginal = Convert.ToInt32(inverterWithBattery.InverterBattery.FirstOrDefault().ChargingPowerFromGridKWh * 1000);
                batteryControlHours.ActionTypeCommand = ActionTypeCommand.ChargeMax;

                _dbContext.Entry(batteryControlHours).State = EntityState.Modified;

                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok(batteryControlHours);
                }
                catch (DbUpdateConcurrencyException)
                {

                    return BadRequest();

                }


            case ActionTypeCommand.SelfUse:

                if (!inverterWithBattery.UseInverterSelfUse)
                {
                    if (inverterWithBattery.UseFixedAvgHourlyWatts)
                    {
                        batteryControlHours.MaxAvgHourlyConsumptionOriginal = inverterWithBattery.FixedAvgHourlyWatts;
                    }
                    else
                    {
                        SpotPrice spotPrice = await _dbContext.SpotPrice.FirstOrDefaultAsync(x => x.Id == batteryControlHours.SpotPriceMaxId);
                        DayOfWeek dayOfWeek = spotPrice.DateTime.DayOfWeek;
                        batteryControlHours.MaxAvgHourlyConsumptionOriginal = await CalculateAverageConsumption(dayOfWeek, inverterWithBattery.Id, spotPrice);
                    }

                    batteryControlHours.MinChargingPowerWhOriginal = Convert.ToInt32(inverterWithBattery.MaxSalesPowerCapacity) * 1000;
                    batteryControlHours.ActionTypeCommand = ActionTypeCommand.SelfUse;
                    
                }
                else
                {
                    if (inverterWithBattery.UseFixedAvgHourlyWatts)
                    {
                        batteryControlHours.MaxAvgHourlyConsumptionOriginal = inverterWithBattery.FixedAvgHourlyWatts;
                    }
                    else
                    {
                        SpotPrice spotPrice = await _dbContext.SpotPrice.FirstOrDefaultAsync(x => x.Id == batteryControlHours.SpotPriceMaxId);
                        DayOfWeek dayOfWeek = spotPrice.DateTime.DayOfWeek;
                        batteryControlHours.MaxAvgHourlyConsumptionOriginal = await CalculateAverageConsumption(dayOfWeek, inverterWithBattery.Id, spotPrice);
                    }

                    batteryControlHours.MinChargingPowerWhOriginal = Convert.ToInt32(inverterWithBattery.MaxSalesPowerCapacity) * 1000;
                    batteryControlHours.ActionTypeCommand = ActionTypeCommand.InverterSelfUse;
                }


                _dbContext.Entry(batteryControlHours).State = EntityState.Modified;

                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok(batteryControlHours);
                }
                catch (DbUpdateConcurrencyException)
                {

                    return BadRequest();

                }

            case ActionTypeCommand.ConsumeBatteryWithMaxPower:
                batteryControlHours.MinChargingPowerWhOriginal = Convert.ToInt32(inverterWithBattery.MaxPower) * 1000;
                batteryControlHours.UsableWatts = Convert.ToInt32(inverterWithBattery.MaxPower) * 1000;
                batteryControlHours.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                _dbContext.Entry(batteryControlHours).State = EntityState.Modified;

                try
                {
                    await _dbContext.SaveChangesAsync();
                    return Ok(batteryControlHours);
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest();
                }

            default:
                break;
        }
                                                       

        return Ok();

    }

    private async Task<int> CalculateAverageConsumption(DayOfWeek dayOfWeek, int InverterId, SpotPrice spotPrice)
    {
        var inverterHoursAvgConsumption = await _dbContext.InverterHoursAvgConsumption.Where(x => x.DayOfWeek == dayOfWeek && x.InverterId == InverterId).ToListAsync();

        if (inverterHoursAvgConsumption == null || inverterHoursAvgConsumption.Count == 0)  return 0;


        inverterHoursAvgConsumption = inverterHoursAvgConsumption
                                       .GroupBy(x => new { x.DateCalculated, x.TimeHour, x.DayOfWeek })
                                        .Select(g => g.OrderByDescending(x => x.DateCalculated).FirstOrDefault())
                                        .ToList()!;

       if (inverterHoursAvgConsumption == null || inverterHoursAvgConsumption.Count == 0) return 0;

        return inverterHoursAvgConsumption.FirstOrDefault(x=> x.TimeHour == spotPrice.Time).AvgHourlyConsumption;
    }
}
