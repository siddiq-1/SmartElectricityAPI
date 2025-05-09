using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ViewModel;
using System.Collections.Immutable;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SensorController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;

    public SensorController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }

    [HttpGet("DeviceSensors/{deviceId}"), Authorize]
    public async Task<ActionResult<List<Sensor>>> GetDeviceSensors(int deviceId)
    {
        var companyDevices = await _dbContext.Device.FirstOrDefaultAsync(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Id  == deviceId);

        if (companyDevices == null)
        {
            return NotFound();
        }

        var deviceSensors = await _dbContext.Sensor.Where(x => x.DeviceId == deviceId).ToListAsync();

        if (deviceSensors != null && deviceSensors.Count > 0)
        {
            return Ok(deviceSensors);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("CompanyUnAssignedSensors"), Authorize]
    public async Task<ActionResult<List<Sensor>>> GetCompanyUnAssignedSensors()
    {

        if (_userInfo.IsAdmin || _userInfo.UserLevel == Enums.Level.Moderator)
        {
            var unAssignedSensors = await _dbContext.Sensor.Where(x => x.CompanyId == null && x.DeviceId == null).ToListAsync();

            if (unAssignedSensors != null && unAssignedSensors.Count > 0)
            {
                return Ok(unAssignedSensors);
            }
            else
            {
                return NotFound();
            }
        }
        else
        {
            return BadRequest("You are not allowed to see unassigned sensors");
        }

    }

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<Sensor>> GetSensor(int id)
    {
        if (_userInfo.IsAdmin || _userInfo.UserLevel == Enums.Level.Moderator)
        {
            var sensor = await _dbContext.Sensor.FirstOrDefaultAsync(x => x.Id == id);

            return Ok(sensor);
        }
        else
        {
            var isCompanySensor = await _dbContext.Sensor.AnyAsync(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Id == id);

            if (!isCompanySensor)
            {
                return NotFound();
            }

            var sensor = await _dbContext.Sensor.FirstOrDefaultAsync(x => x.Id == id);

            return Ok(sensor);
        }
    }

    [HttpGet("All"), Authorize]
    public async Task<ActionResult<Sensor>> GetAllSensors()
    {
        if (!_userInfo.IsAdmin)
        {
            return BadRequest();
        }
        var allSensors = await _dbContext.Sensor.Include(i => i.Company).Include(j => j.SwitchModel).ToListAsync();

        return Ok(allSensors);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> PostSensor([FromBody] SensorViewModel sensorViewModel)
    {
        if (!_userInfo.IsAdmin)
        {
            return BadRequest("You are not allowed to create sensors for other companies");
        }

        if (await _dbContext.Sensor.AnyAsync(x => x.Name == sensorViewModel.Name))
        {
            return BadRequest("Sensor with this name already exists");
        }

        var switchModelUsed = await _dbContext.SwitchModel.FirstOrDefaultAsync(x => x.Id == sensorViewModel.SwitchModelId);

        string mqttTopic = sensorViewModel.Name + switchModelUsed.MqttSuffix;

        var sensor = new Sensor
        {
            Name = sensorViewModel.Name,
            Description = sensorViewModel.Description,
            DeviceId = sensorViewModel.DeviceId,
           // CompanyId = sensorViewModel.CompanyId,
            SwitchModelId = sensorViewModel.SwitchModelId,
            DeviceActionType = Enums.DeviceActionType.Off,
            BroadcastToFusebox = sensorViewModel.BroadcastToFusebox,
            Topic = mqttTopic

        };

        _dbContext.Sensor.Add(sensor);

        await _dbContext.SaveChangesAsync();

        Switch switchNone = new Switch
        {
            SensorId = sensor.Id,
            CompanyId = sensor.CompanyId,
            DeviceId = sensor.DeviceId,
            SwitchModelId = sensor.SwitchModelId,
            DeviceActionType = Enums.DeviceActionType.None,
        };

        _dbContext.Switch.Add(switchNone);

        Switch switchToggle = new Switch
        {
            SensorId = sensor.Id,
            CompanyId = sensor.CompanyId,
            DeviceId = sensor.DeviceId,
            SwitchModelId = sensor.SwitchModelId,
            DeviceActionType = sensor.DeviceActionType,
        };

        _dbContext.Switch.Add(switchToggle);

        await _dbContext.SaveChangesAsync();

        return Ok(sensor);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateSensor(int id, SensorViewModel sensorViewModel)
    {
        if (id != sensorViewModel.Id)
        {
            return BadRequest();
        }

        if (_userInfo.UserLevel == Enums.Level.User && sensorViewModel.CompanyId != _userInfo.SelectedCompanyId)
        {
            return BadRequest();
        }

        if (await _dbContext.Device.AnyAsync(x => x.Name == sensorViewModel.Name
        && x.CompanyId == sensorViewModel.CompanyId
        && x.Id != sensorViewModel.Id))
        {
            return BadRequest("Sensor with this name already exists");
        }

        var dbSensor = await _dbContext.Sensor.FindAsync(id);
        var switchModelUsed = await _dbContext.SwitchModel.FirstOrDefaultAsync(x => x.Id == sensorViewModel.SwitchModelId);
        bool deviceActionTypeChanged = dbSensor.DeviceActionType != sensorViewModel.DeviceActionType;
        string mqttTopic = sensorViewModel.Name + switchModelUsed.MqttSuffix;
        if (dbSensor != null)
        {
            dbSensor.Name = sensorViewModel.Name;
            dbSensor.Description = sensorViewModel.Description;
            dbSensor.DeviceId = sensorViewModel.DeviceId;
            dbSensor.CompanyId = sensorViewModel.CompanyId;
            dbSensor.SwitchModelId = sensorViewModel.SwitchModelId;
            dbSensor.DeviceActionType = sensorViewModel.DeviceActionType;
            dbSensor.BroadcastToFusebox = sensorViewModel.BroadcastToFusebox;
            dbSensor.Topic = mqttTopic;

            _dbContext.Entry(dbSensor).State = EntityState.Modified;

            try
            {
                if (deviceActionTypeChanged
                     && dbSensor.DeviceActionType != Enums.DeviceActionType.None)
                {
                    var switchToggle = await _dbContext.Switch.FirstOrDefaultAsync(x => x.SensorId == dbSensor.Id && x.DeviceActionType != Enums.DeviceActionType.None);
                    switchToggle.DeviceActionType = dbSensor.DeviceActionType;
                    _dbContext.Entry(switchToggle).State = EntityState.Modified;
                }

                var sensorSwitches = await _dbContext.Switch.Where(x => x.SensorId == dbSensor.Id).ToListAsync();

                if (sensorSwitches != null && sensorSwitches.Count > 0)
                {
                    foreach (var sensorSwitch in sensorSwitches)
                    {
                        sensorSwitch.SwitchModelId = dbSensor.SwitchModelId;
                        _dbContext.Entry(sensorSwitch).State = EntityState.Modified;
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();
            }

            return Ok(dbSensor);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPut("AllDeviceSensors/{deviceId}"), Authorize]
    public async Task<IActionResult> UpdateAllDeviceSensors(int deviceId, List<SensorViewModel> sensorViewModel)
    {
        var deviceSensorsInDb = await _dbContext.Sensor.Where(x => x.DeviceId == deviceId && x.CompanyId == _userInfo.SelectedCompanyId).ToListAsync();

        if (deviceSensorsInDb == null || deviceSensorsInDb.Count == 0)
        {
            return NotFound();
        }

        if (!_userInfo.IsAdmin && sensorViewModel.Any(x=> x.CompanyId != _userInfo.SelectedCompanyId))
        {
            return BadRequest();
        }

        foreach (var sensorViewItem in sensorViewModel)
        {
            if (!deviceSensorsInDb.Any(x=> x.Id == sensorViewItem.Id))
            {
                continue;
            }

            if (await _dbContext.Device.AnyAsync(x => x.Name == sensorViewItem.Name
                    && x.CompanyId == sensorViewItem.CompanyId
                    && x.Id != sensorViewItem.Id))
            {
                return BadRequest("Sensor with this name already exists");
            }

            var dbSensor = await _dbContext.Sensor.FindAsync(sensorViewItem.Id);

            bool deviceActionTypeChanged = dbSensor.DeviceActionType != sensorViewItem.DeviceActionType;

            if (dbSensor != null)
            {
                dbSensor.Name = sensorViewItem.Name;
                dbSensor.Description = sensorViewItem.Description;
                dbSensor.DeviceId = sensorViewItem.DeviceId;
                dbSensor.CompanyId = sensorViewItem.CompanyId;
                dbSensor.SwitchModelId = sensorViewItem.SwitchModelId;
                dbSensor.DeviceActionType = sensorViewItem.DeviceActionType;
                dbSensor.BroadcastToFusebox = sensorViewItem.BroadcastToFusebox;
                dbSensor.Topic = sensorViewItem.Topic;

                _dbContext.Entry(dbSensor).State = EntityState.Modified;

                try
                {
                    if (deviceActionTypeChanged
                        && dbSensor.DeviceActionType != Enums.DeviceActionType.None)
                    {
                        var switchToggle = await _dbContext.Switch.FirstOrDefaultAsync(x => x.SensorId == dbSensor.Id && x.DeviceActionType != Enums.DeviceActionType.None);
                        if (switchToggle != null)
                        {
                            switchToggle.DeviceActionType = dbSensor.DeviceActionType;
                            _dbContext.Entry(switchToggle).State = EntityState.Modified;
                        }                 
                    }

                    var sensorSwitches = await _dbContext.Switch.Where(x => x.SensorId == dbSensor.Id).ToListAsync();

                    if (sensorSwitches != null && sensorSwitches.Count > 0)
                    {
                        foreach (var sensorSwitch in sensorSwitches)
                        {
                            sensorSwitch.SwitchModelId = dbSensor.SwitchModelId;
                            _dbContext.Entry(sensorSwitch).State = EntityState.Modified;
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return BadRequest();
                }               
            }   
        }

        return Ok();
    }
}
