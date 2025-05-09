using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ViewModel;
using SmartElectricityAPI.Services;
using System;
using System.Diagnostics;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InverterBatteryController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    private readonly InverterApiService _inverterApiService;
    private readonly MqttSystemMessageService _mqttSystemMessageService;
    public InverterBatteryController(MySQLDBContext context, IUserInfo userInfo, InverterApiService inverterApiService, MqttSystemMessageService mqttSystemMessageService)
    {
        _dbContext = context;
        _userInfo = userInfo;
        _inverterApiService = inverterApiService;
        _mqttSystemMessageService = mqttSystemMessageService;
    }

    [HttpGet("{id}"),  Authorize]
    public async Task<ActionResult<List<InverterBattery>>> GetInverterBattery(int id)
    {
        List<InverterBattery> inverterBattery = new List<InverterBattery>();

        if (await _dbContext.Inverter.AnyAsync(x=> x.CompanyId == _userInfo.SelectedCompanyId && x.Id == id))
        {
           inverterBattery = await _dbContext.InverterBattery.Where(x => x.InverterId == id).ToListAsync();
        }  

        if (inverterBattery == null)
        {
            return NotFound();
        }
        foreach (var item in inverterBattery)
        {
            item.AdditionalTimeForBatteryChargingPercentage = item.AdditionalTimeForBatteryChargingPercentage * 100;
        }
        return inverterBattery;
    }

    [HttpGet("ByBatteryId/{id}"), Authorize]
    public async Task<ActionResult> GetBattery(int id)
    {
        InverterBattery inverterBattery = new InverterBattery();

        if (await _dbContext.Inverter.AnyAsync(x => x.CompanyId == _userInfo.SelectedCompanyId))
        {
            var companyInverters = await _dbContext.Inverter.Where(x => x.CompanyId == _userInfo.SelectedCompanyId).ToListAsync();

            inverterBattery = await _dbContext.InverterBattery.Where(x => x.Id == id && companyInverters.Select(y => y.Id).ToList().Contains(x.InverterId)).FirstOrDefaultAsync();
        }

        if (inverterBattery == null)
        {
            return NotFound();
        }

        inverterBattery.AdditionalTimeForBatteryChargingPercentage = inverterBattery.AdditionalTimeForBatteryChargingPercentage * 100;

        return Ok(inverterBattery);
    }
    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateInverterBattery(int id, InverterBatteryViewModel inverterBattery)
    {
        if (id != inverterBattery.Id)
        {
            return BadRequest();
        }
        var companyInverters = await _dbContext.Inverter.Where(x => x.CompanyId == _userInfo.SelectedCompanyId).ToListAsync();

        if (! await _dbContext.InverterBattery.AnyAsync(x => x.Id == id && companyInverters.Select(y => y.Id).ToList().Contains(x.InverterId)))
        {
            return BadRequest();
        }

        if (inverterBattery.AdditionalTimeForBatteryChargingPercentage > 50)
        {
            return BadRequest("Charging additional % can\'t be more than 50%");
        }


        if (inverterBattery.AdditionalTimeForBatteryChargingPercentage < 0)
        {
            return BadRequest("Charging additional % can\'t be less than 0%");
        }

        if (inverterBattery.MinLevel < 5)
        {
            return BadRequest("Battery minimum level can\'t be less than 5%");
        }

        if (inverterBattery.MaxLevel < 85)
        {
            return BadRequest("Battery maximum level can\'t be less than 85%");
        }

        if (inverterBattery.CalculateBatterSocFromVolts && inverterBattery.BatteryVoltsMin < 150)
        {
            return BadRequest("Battery minimum volts can\'t be less than 150V");
        }

        if (inverterBattery.CalculateBatterSocFromVolts && inverterBattery.BatteryVoltsMax > 850)
        {
            return BadRequest("Battery maximum volts can\'t be more than 850V");
        }

        if (inverterBattery.CalculateBatterSocFromVolts && inverterBattery.BatteryVoltsMin > inverterBattery.BatteryVoltsMax) 
        {
            return BadRequest("Battery minimum volts can\'t be more than maximum volts");
        }

        if (inverterBattery.BatteryMinLevelWithConsumeMax < 0)
        {
            return BadRequest("Battery minimum level with consume max can\'t be less than 0%");
        }

        if (inverterBattery.BatteryMinLevelWithConsumeMax + inverterBattery.MinLevel > inverterBattery.MaxLevel) 
        {
            return BadRequest("Battery minimum level with consume max can\'t be more than maximum level");
        }

     

        var dbInverterBattery = await _dbContext.InverterBattery.FindAsync(id);

        dbInverterBattery.MinLevel = inverterBattery.MinLevel;
        dbInverterBattery.MaxLevel = inverterBattery.MaxLevel;
        dbInverterBattery.CapacityKWh = inverterBattery.CapacityKWh;
        dbInverterBattery.ChargingPowerFromGridKWh = inverterBattery.ChargingPowerFromGridKWh;
        dbInverterBattery.ChargingPowerFromSolarKWh = inverterBattery.ChargingPowerFromSolarKWh;
        dbInverterBattery.ConsiderBadWeather75PercentFactor = inverterBattery.ConsiderBadWeather75PercentFactor;
        dbInverterBattery.LoadBatteryTo95PercentPrice = inverterBattery.LoadBatteryTo95PercentPrice;
        dbInverterBattery.LoadBatteryTo95PercentEnabled = inverterBattery.LoadBatteryTo95PercentEnabled;
        dbInverterBattery.AdditionalTimeForBatteryChargingPercentage = inverterBattery.AdditionalTimeForBatteryChargingPercentage / 100;
        dbInverterBattery.NumberOfBatteries = inverterBattery.NumberOfBatteries;
        dbInverterBattery.DischargingPowerToGridKWh = inverterBattery.DischargingPowerToGridKWh;
        dbInverterBattery.CalculateBatterSocFromVolts = inverterBattery.CalculateBatterSocFromVolts;
        dbInverterBattery.BatteryVoltsMin = inverterBattery.BatteryVoltsMin;
        dbInverterBattery.BatteryVoltsMax = inverterBattery.BatteryVoltsMax;
        dbInverterBattery.BatteryMinLevelWithConsumeMax = inverterBattery.BatteryMinLevelWithConsumeMax;
        dbInverterBattery.AllowPurchasingFromGridInSummer = inverterBattery.AllowPurchasingFromGridInSummer;
        dbInverterBattery.ConsiderRemainingBatteryOnPurchase = inverterBattery.ConsiderRemainingBatteryOnPurchase;
        dbInverterBattery.Enabled = inverterBattery.Enabled;

        if (_userInfo.IsAdmin)
        {
            dbInverterBattery.HzMarketDischargeMinPrice = inverterBattery.HzMarketDischargeMinPrice;
            dbInverterBattery.HzMarketMinBatteryLevelOnDischargeCommand = inverterBattery.HzMarketMinBatteryLevelOnDischargeCommand;
        }
        else
        {
            dbInverterBattery.HzMarketDischargeMinPrice = 0;
            dbInverterBattery.HzMarketMinBatteryLevelOnDischargeCommand = 0;
        }

        if (dbInverterBattery.UseHzMarket)
        {
            dbInverterBattery.ClientUseHzMarket = inverterBattery.ClientUseHzMarket;
        }

        _dbContext.Entry(dbInverterBattery).State = EntityState.Modified;

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
            }
            await _mqttSystemMessageService.PublishSystemMesasge(MqttSystemMessageService.MessagePayLoad.Refresh);
        }

        return Ok(dbInverterBattery);

    }

    [HttpPost, Authorize]
    public async Task<IActionResult> PostInverterBattery([FromBody] InverterBatteryViewModel inverterBatteryView)
    {
        var inverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.CompanyId == _userInfo.SelectedCompanyId);

        if (inverterBatteryView.AdditionalTimeForBatteryChargingPercentage > 50)
        {
            return BadRequest("Charging additional % can\'t be more than 50%");
        }

        if (inverterBatteryView.AdditionalTimeForBatteryChargingPercentage < 0)
        {
            return BadRequest("Charging additional % can\'t be less than 0%");
        }

        if (inverterBatteryView.CalculateBatterSocFromVolts && inverterBatteryView.BatteryVoltsMin < 150)
        {
           return BadRequest("Battery minimum volts can\'t be less than 150V");
        }

        if (inverterBatteryView.CalculateBatterSocFromVolts && inverterBatteryView.BatteryVoltsMax > 850)
        {
            return BadRequest("Battery maximum volts can\'t be more than 850V");
        }

        if (inverterBatteryView.CalculateBatterSocFromVolts && inverterBatteryView.BatteryVoltsMin > inverterBatteryView.BatteryVoltsMax)
        {
            return BadRequest("Battery minimum volts can\'t be more than maximum volts");
        }

        if (inverterBatteryView.BatteryMinLevelWithConsumeMax < 0)
        {
            return BadRequest("Battery minimum level with consume max can\'t be less than 0%");
        }

        if (inverterBatteryView.BatteryMinLevelWithConsumeMax + inverterBatteryView.MinLevel > inverterBatteryView.MaxLevel)
        {
            return BadRequest("Battery minimum level with consume max can\'t be more than maximum level");
        }

        InverterBattery inverterBattery = new InverterBattery
        {
             InverterId = inverter.Id,
             CapacityKWh = inverterBatteryView.CapacityKWh,
             MaxLevel = inverterBatteryView.MaxLevel,
             MinLevel = inverterBatteryView.MinLevel,
             ConsiderBadWeather75PercentFactor = inverterBatteryView.ConsiderBadWeather75PercentFactor,
             LoadBatteryTo95PercentPrice = inverterBatteryView.LoadBatteryTo95PercentPrice,
             LoadBatteryTo95PercentEnabled = inverterBatteryView.LoadBatteryTo95PercentEnabled,
             ChargingPowerFromGridKWh = inverterBatteryView.ChargingPowerFromGridKWh,
             ChargingPowerFromSolarKWh = inverterBatteryView.ChargingPowerFromSolarKWh,
             AdditionalTimeForBatteryChargingPercentage = inverterBatteryView.AdditionalTimeForBatteryChargingPercentage / 100,
             NumberOfBatteries = inverterBatteryView.NumberOfBatteries,
             DischargingPowerToGridKWh = inverterBatteryView.DischargingPowerToGridKWh,
             CalculateBatterSocFromVolts = inverterBatteryView.CalculateBatterSocFromVolts,
             BatteryVoltsMin = inverterBatteryView.BatteryVoltsMin,
             BatteryVoltsMax = inverterBatteryView.BatteryVoltsMax,
             BatteryMinLevelWithConsumeMax = inverterBatteryView.BatteryMinLevelWithConsumeMax,
             AllowPurchasingFromGridInSummer = inverterBatteryView.AllowPurchasingFromGridInSummer,
             ConsiderRemainingBatteryOnPurchase = inverterBatteryView.ConsiderRemainingBatteryOnPurchase,
             Enabled = inverterBatteryView.Enabled
        };

        if (_userInfo.IsAdmin)
        {
            inverterBattery.HzMarketDischargeMinPrice = inverterBatteryView.HzMarketDischargeMinPrice;
            inverterBattery.HzMarketMinBatteryLevelOnDischargeCommand = inverterBatteryView.HzMarketMinBatteryLevelOnDischargeCommand;
        }

        if (inverterBattery.NumberOfBatteries == 0)
        {
            inverterBattery.NumberOfBatteries = 1;
        }


        _dbContext.InverterBattery.Add(inverterBattery);

        await _dbContext.SaveChangesAsync();

        if (!Debugger.IsAttached)
        {
            var authRes = await _inverterApiService.GetToken();

            if (authRes != null)
            {
                await _inverterApiService.RefreshInverterData(authRes.token);
            }
        }



        return Ok(inverterBattery);
    }
}
