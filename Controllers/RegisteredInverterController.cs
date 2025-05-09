using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.BackgroundServices;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using System.Diagnostics;
using System.Linq;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RegisteredInverterController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<RegisteredInverterController> _logger;
    private readonly IUserInfo _userInfo;
    private readonly InverterApiService _inverterApiService;
    private readonly MqttSystemMessageService _mqttSystemMessageService;

    public RegisteredInverterController(MySQLDBContext context, ILogger<RegisteredInverterController> logger, IUserInfo userInfo, InverterApiService inverterApiService, MqttSystemMessageService mqttSystemMessageService)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
        _inverterApiService = inverterApiService;
        _mqttSystemMessageService = mqttSystemMessageService;
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> GetRegisteredInverters()
    {
        if (_userInfo.IsAdmin)
        {
            return Ok(_dbContext.RegisteredInverter.OrderByDescending(x => x.Id).ToList());
        }
        else
        {
            return BadRequest();
        }

    }

    [HttpPost, Authorize]
    public async Task<ActionResult<RegisteredInverter>> PostRegisteredInverter(RegisteredInverter registeredInverter)
    {
        if (_userInfo.IsAdmin)
        {
            if (RegisteredInverterNameExists(registeredInverter.Name))
            {
                return Conflict("Inverter name already exists.");
            }
            _dbContext.RegisteredInverter.Add(registeredInverter);
            await _dbContext.SaveChangesAsync();

            if (!Debugger.IsAttached)
            {
                var authRes = await _inverterApiService.GetToken();

                if (authRes != null)
                {
                    await _inverterApiService.RefreshInverterData(authRes.token);
                    await _mqttSystemMessageService.PublishSystemMesasge(MqttSystemMessageService.MessagePayLoad.Refresh);
                }
            }



            return CreatedAtAction(nameof(GetRegisteredInverter), new { id = registeredInverter.Id }, registeredInverter);
        }
        else
        {
            return BadRequest();
        }

    }

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<RegisteredInverter>> GetRegisteredInverter(int id)
    {
        var registeredInverter = _dbContext.RegisteredInverter.Where(x => x.Id == id).FirstOrDefault();

        if (registeredInverter == null)
        {
            return NotFound();
        }

        return registeredInverter;
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateRegisteredInverter(int id, RegisteredInverter registeredInverter)
    {
        if (id != registeredInverter.Id)
        {
            return BadRequest();
        }

        if (_dbContext.RegisteredInverter.Any(x => x.Name == registeredInverter.Name && x.Id != id))
        {
            return BadRequest("Inverter with that name already exists.");
        }

        _dbContext.Entry(registeredInverter).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RegisteredInverterIdExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
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



        return Ok(registeredInverter);
    }

    private bool RegisteredInverterIdExists(long id)
    {
        return (_dbContext.RegisteredInverter?.Any(x => x.Id == id)).GetValueOrDefault();
    }

    private bool RegisteredInverterNameExists(string name)
    {
        return (_dbContext.RegisteredInverter?.Any(x => x.Name == name)).GetValueOrDefault();
    }
}
