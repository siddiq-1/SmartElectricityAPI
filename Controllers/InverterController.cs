using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ViewModel;
using SmartElectricityAPI.Processors;
using SmartElectricityAPI.Services;
using System.Diagnostics;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InverterController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    private readonly InverterApiService _inverterApiService;
    private readonly MqttSystemMessageService _mqttSystemMessageService;
    private readonly IMqttLogger _mqttLogger;

    public InverterController(MySQLDBContext context, IUserInfo userInfo, InverterApiService inverterApiService, MqttSystemMessageService mqttSystemMessageService, IMqttLogger mqttLogger)
    {
        _dbContext = context;
        _userInfo = userInfo;
        _inverterApiService = inverterApiService;
        _mqttSystemMessageService = mqttSystemMessageService;
        _mqttLogger = mqttLogger;
    }

    [HttpGet, Route("CompanyInverter"), Authorize]
    public async Task<OkObjectResult> GetCompanyInverter()
    {
        var queryResult = await _dbContext.Inverter.Include(i => i.RegisteredInverter).Include(x => x.InverterType).Where(x => x.CompanyId == _userInfo.SelectedCompanyId).ToListAsync();


        foreach (var item in queryResult)
        {
            var batteryInfo = await _dbContext.InverterBattery.Where(x => x.InverterId == item.Id).ToListAsync();
            foreach (var batteryItem in batteryInfo)
            {
                batteryItem.AdditionalTimeForBatteryChargingPercentage = batteryItem.AdditionalTimeForBatteryChargingPercentage * 100;
            }
            item.InverterBattery = batteryInfo;
        }

        string json = JsonConvert.SerializeObject(queryResult, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        return Ok(queryResult);
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> GetInverters()
    {
        if (_userInfo.IsAdmin)
        {
            return Ok(await _dbContext.Inverter.OrderBy(x=> x.Company.Name).Include(i => i.RegisteredInverter).Include(i => i.Company).Include(i => i.InverterType).ToListAsync());
        }
        else
        {
            return BadRequest();
        }

    }


    [HttpPost, Authorize]
    public async Task<ActionResult<Inverter>> PostInverter([FromBody] InverterUserViewModel inverter)
    {
        if (_userInfo.IsAdmin)
        {
            if (await _dbContext.Inverter.AnyAsync(x=> x.RegisteredInverterId == inverter.RegisteredInverterId))
            {
                return Conflict("Registered inverter already assigned.");
            }

            if (inverter.UseFixedAvgHourlyWatts && inverter.FixedAvgHourlyWatts < 50)
            {
                return BadRequest("Average consumption value must be at least 50w when using fixed average hourly watts");
            }

            if (_userInfo.IsAdmin)
            {
                inverter.NumberOfInverters = 1;
            }

            if (inverter.NumberOfInverters < 1)
            {
                return BadRequest("Number of inverters must be at least 1");
            }

            var newInverter = new Inverter
            {
                CompanyId = inverter.CompanyId,
                RegisteredInverterId = inverter.RegisteredInverterId,
                InverterTypeId = inverter.InverterTypeId,
                MaxPower = inverter.MaxPower,
                MaxSalesPowerCapacity = inverter.MaxSalesPowerCapacity,
                UseFixedAvgHourlyWatts = inverter.UseFixedAvgHourlyWatts,
                FixedAvgHourlyWatts = inverter.FixedAvgHourlyWatts,
                UseOnlyCompensateMissingEnergy = inverter.UseOnlyCompensateMissingEnergy,
                UseWeatherForecast = inverter.UseWeatherForecast,
                SolarPanelsMaxPower = inverter.SolarPanelsMaxPower,
                SolarPanelsDirecation = inverter.SolarPanelsDirecation,
                CalculationFormula = inverter.CalculationFormula,
                UseInverterSelfUse = inverter.UseInverterSelfUse,
                AllowPurchasingFromGridInSummer = inverter.AllowPurchasingFromGridInSummer,
                PVInverterIsSeparated = inverter.PVInverterIsSeparated,
                NumberOfInverters = inverter.NumberOfInverters
            };

            _dbContext.Inverter.Add(newInverter);

            await _dbContext.SaveChangesAsync();

           

            if (await _dbContext.InverterTypeActions.AnyAsync(x=> x.InverterTypeId == newInverter.InverterTypeId))
            {
                var inverterTypeActions = await _dbContext.InverterTypeActions.Where(x => x.InverterTypeId == newInverter.InverterTypeId).ToListAsync();

                foreach (var item in inverterTypeActions)
                {
                    _dbContext.InverterTypeCompanyActions.Add(new InverterTypeCompanyActions {
                        CompanyId = newInverter.CompanyId,
                        InverterId = newInverter.Id,
                        ActionName = item.ActionName,
                        ActionType = item.ActionType,
                        ActionTypeCommand = item.ActionTypeCommand,
                        InverterTypeId = item.InverterTypeId,
                        InverterTypeActionsId = item.Id                             
                    });
                }

                await _dbContext.SaveChangesAsync();
            }

            if (!Debugger.IsAttached)
            {
                var authRes = await _inverterApiService.GetToken();

                if (authRes != null)
                {
                    await _inverterApiService.RefreshInverterData(authRes.token);
                    await _mqttSystemMessageService.PublishSystemMesasge(MqttSystemMessageService.MessagePayLoad.Refresh);
                }
            }



            return Ok(newInverter);
        }
        else
        {
            return BadRequest();
        }

    }

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<Inverter>> GetInverter(int id)
    {
        var inverter = await _dbContext.Inverter.Where(x => x.Id == id).FirstOrDefaultAsync();

        if (inverter == null)
        {
            return NotFound();
        }

        return inverter;
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateInverter(int id, InverterUserViewModel inverter)
    {
        if (id != inverter.Id)
        {
            return BadRequest();
        }

        
        if (!_userInfo.IsAdmin  && inverter.CompanyId != _userInfo.SelectedCompanyId)
        {
            return BadRequest();
        }

        if (inverter.UseFixedAvgHourlyWatts && inverter.FixedAvgHourlyWatts < 50)
        {
            return BadRequest("Average consumption value must be at least 50w when using fixed average hourly watts");
        }

        if (inverter.NumberOfInverters < 1)
        {
            return BadRequest("Number of inverters must be at least 1");
        }

        var dbInverter = await _dbContext.Inverter.FindAsync(id);

        dbInverter.InverterTypeId = inverter.InverterTypeId;
        dbInverter.MaxPower = inverter.MaxPower;
        dbInverter.MaxSalesPowerCapacity = inverter.MaxSalesPowerCapacity;
        dbInverter.UseFixedAvgHourlyWatts = inverter.UseFixedAvgHourlyWatts;
        dbInverter.FixedAvgHourlyWatts = inverter.FixedAvgHourlyWatts;
        dbInverter.UseOnlyCompensateMissingEnergy = inverter.UseOnlyCompensateMissingEnergy;
        dbInverter.UseWeatherForecast = inverter.UseWeatherForecast;
        dbInverter.SolarPanelsMaxPower = inverter.SolarPanelsMaxPower;
        dbInverter.SolarPanelsDirecation = inverter.SolarPanelsDirecation;
        dbInverter.CalculationFormula = inverter.CalculationFormula;
        dbInverter.AllowPurchasingFromGridInSummer = inverter.AllowPurchasingFromGridInSummer;
        dbInverter.PVInverterIsSeparated = inverter.PVInverterIsSeparated;
        dbInverter.NumberOfInverters = inverter.NumberOfInverters;

        if (dbInverter.UseInverterSelfUse != inverter.UseInverterSelfUse)
        {
 
            var battery = await _dbContext.InverterBattery.Where(x => x.InverterId == dbInverter.Id).FirstOrDefaultAsync();

            if (battery != null)
            {
                var currentDateTime = DateTime.Now;

                var batteryControlHours = await _dbContext.BatteryControlHours.Where(x => x.InverterBatteryId == battery.Id).
                    Include(x => x.SpotPriceMax).
                    Where(x => x.SpotPriceMax.DateTime >= currentDateTime).OrderBy(x => x.SpotPriceMax.DateTime).ToListAsync();

                if (batteryControlHours.Count > 0)
                {
                    if (inverter.UseInverterSelfUse)
                    {
                        foreach (var item in batteryControlHours)
                        {
                            if (item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                            {
                                item.ActionTypeCommand = ActionTypeCommand.InverterSelfUse;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in batteryControlHours)
                        {
                            if (item.ActionTypeCommand == ActionTypeCommand.InverterSelfUse)
                            {
                                item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                            }
                        }
                    }

                }
                dbInverter.UseInverterSelfUse = inverter.UseInverterSelfUse;

                await _dbContext.SaveChangesAsync();

                if (!inverter.UseInverterSelfUse)
                {
                    RedisCacheService redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());

                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);
                    var inverterWithRegInverter = await _dbContext.Inverter.Where(x=> x.Id == dbInverter.Id).Include(x => x.RegisteredInverter).FirstOrDefaultAsync();
                    await batteryButtonActions.ProcessInverterModeControl(inverterWithRegInverter, _dbContext, ActionTypeCommand.PassiveMode);
         

                    var currentHourCommand = await FindCurrentHourActionTypeCommand(battery, (int)_userInfo.SelectedCompanyId);
                    var InverterTypeCompanyAction = await _dbContext.InverterTypeCompanyActions.Where(x => x.CompanyId == (int)_userInfo.SelectedCompanyId && x.ActionTypeCommand == currentHourCommand).FirstOrDefaultAsync();

                    var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == (int)_userInfo.SelectedCompanyId);
                    var inverterBatteryButtonProcessor = new InverterBatteryButtonProcessor(company, InverterTypeCompanyAction.Id, redisCacheService, _mqttLogger);
                    await inverterBatteryButtonProcessor.Process();
                }
            }
        }   

        _dbContext.Entry(dbInverter).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {

            return BadRequest();

        }

        if (!Debugger.IsAttached)
        {
            var authRes = await _inverterApiService.GetToken();

            if (authRes != null)
            {
                await _inverterApiService.RefreshInverterData(authRes.token);
                await _mqttSystemMessageService.PublishSystemMesasge(MqttSystemMessageService.MessagePayLoad.Refresh);
            }
        }



        return Ok(dbInverter);

    }

    private async Task<ActionTypeCommand> FindCurrentHourActionTypeCommand(InverterBattery inverterBattery, int companyId)
    {
        using (var mySQLDBContexForButtons = await new DatabaseService().CreateDbContextAsync())
        {
            DateTime currentDateTime = DateTime.Now;

            var company = mySQLDBContexForButtons.Company.FirstOrDefault(x => x.Id == companyId);

            var currentDateTimeSpotPrice = mySQLDBContexForButtons.SpotPrice
                .Where(x => x.DateTime.Year == currentDateTime.Year
                 && x.DateTime.Month == currentDateTime.Month
                 && x.DateTime.Day == currentDateTime.Day
                 && x.DateTime.Hour == currentDateTime.Hour
                 && x.RegionId == company.RegionId)
                .Join(mySQLDBContexForButtons.BatteryControlHours
                        .Where(bh => bh.InverterBatteryId == inverterBattery.Id),
                    spotPrice => spotPrice.Id,
                    batteryHours => batteryHours.SpotPriceMaxId,
                    (spotPrice, batteryHours) => new
                    {
                        SpotPrice = spotPrice,
                        BatteryHours = batteryHours
                    }).FirstOrDefault();

            return currentDateTimeSpotPrice.BatteryHours.ActionTypeCommand;
        }
    }

    [HttpGet("LatestState/{id}"), Authorize]
    public async Task<ActionResult> GetInverterLatestStateInfo(int id)
    {
        var inverter = await _dbContext.Inverter.Include(x=> x.InverterBattery).Where(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Id == id).FirstOrDefaultAsync();

        if (inverter == null)
        {
            return NotFound();
        }
         RedisCacheService redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());

        var cacheResult = await redisCacheService.GetKeyValue<SofarState>(inverter.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
        cacheResult = cacheResult.OrderByDescending(x => x.CreatedAt).Take(1).ToList();

        if (cacheResult != null && cacheResult.Count > 0)
        {
            SofarState sofarState = cacheResult.FirstOrDefault();

            if (inverter != null && inverter.InverterBattery != null && inverter.InverterBattery.Count > 0)
            {
                sofarState.battery_power = sofarState.battery_power * inverter.NumberOfInverters * inverter.InverterBattery.FirstOrDefault().NumberOfBatteries;
            }      
      
            try
            {
                double usablePowerWithDeductedMaxLevel = Convert.ToDouble(inverter.InverterBattery.FirstOrDefault().CapacityKWh) * (Convert.ToDouble((inverter.InverterBattery.FirstOrDefault().MaxLevel) / 100d));

                sofarState.UsableBatteryEnergy = Math.Round((Convert.ToDouble((sofarState.batterySOC - inverter.InverterBattery.FirstOrDefault().MinLevel))) / 100 * usablePowerWithDeductedMaxLevel, 2);
            }
            catch (Exception)
            {
                Console.WriteLine($"error with inverter: {inverter.Id} ");
            }


          //  redisCacheService.Disconnect();

            return Ok(sofarState);
        }

        return NotFound();

    }

}
