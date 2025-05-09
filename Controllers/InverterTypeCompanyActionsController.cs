using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using MQTTnet.Server;
using SmartElectricityAPI.Enums;
using AutoMapper;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.Models;
using Newtonsoft.Json;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.Infrastructure;
using System.Linq.Expressions;
using System.Security.Cryptography;
using SmartElectricityAPI.Processors;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InverterTypeCompanyActionsController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    private readonly IMqttLogger _mqttLogger;
    private readonly IMapper _mapper;
    private readonly RedisCacheService redisCacheService;


    public InverterTypeCompanyActionsController(MySQLDBContext context, IUserInfo userInfo, IMqttLogger mqttLogger, IMapper mapper)
    {
        _dbContext = context;
        _userInfo = userInfo;
        _mqttLogger = mqttLogger;
        _mapper = mapper;
        redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());

    }

    [HttpGet, Authorize]
    public async Task<ActionResult> GetInverterTypesCompanyActions()
    {
        var companyInverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.CompanyId == _userInfo.SelectedCompanyId);

        if (companyInverter == null)
        {
            return BadRequest("No inverter found for company");
        }

        var result = await _dbContext.InverterTypeCompanyActions.Where(x => x.CompanyId == _userInfo.SelectedCompanyId).Include(x=> x.InverterTypeActions).ToListAsync();

        if (result == null || result.Count == 0) return BadRequest();

        if (!companyInverter.UseInverterSelfUse)
        {
            result = result.Where(x => x.ActionTypeCommand != ActionTypeCommand.InverterSelfUse).ToList();
        }
        else
        {
            result = result.Where(x => x.ActionTypeCommand != ActionTypeCommand.SelfUse).ToList();
        }

        var mappedResult = _mapper.Map<List<InverterTypeCompanyActionsDto>>(result);

        foreach (var item in mappedResult)
        {
            if (item.ActionType ==  ActionType.Automode)
            {
                var inverterBattery = await _dbContext.InverterBattery.FirstOrDefaultAsync(x => x.InverterId == item.InverterId);

                if (inverterBattery == null)
                {
                    item.ButtonEnabled = false;
                }
                else
                {
                    var HasAutoRecords = await _dbContext.BatteryControlHours.Include(x => x.SpotPriceMax).Where(x => x.SpotPriceMax.Date >= DateOnly.FromDateTime(DateTime.Now)).AnyAsync(x => x.InverterBatteryId == inverterBattery.Id);

                    if (!HasAutoRecords)
                    {
                        item.ButtonEnabled = false;
                    }
                }
            }
        }

        return Ok(mappedResult);
    }

    [HttpGet("SendCommand/{id}"), Authorize]
    public async Task<ActionResult> SendInverterTypesCompanyActions(int id)
    {
        var company = _dbContext.Company.FirstOrDefault(x => x.Id == _userInfo.SelectedCompanyId);

        var inverterBatteryButtonProcessor = new InverterBatteryButtonProcessor(company, id, redisCacheService, _mqttLogger);
        await inverterBatteryButtonProcessor.Process();

        return Ok();
    }
}
