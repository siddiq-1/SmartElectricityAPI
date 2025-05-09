using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Migrations;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ViewModel;
using SmartElectricityAPI.Services;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class DeviceController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;

    public DeviceController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }

    [HttpGet, Route("CompanyDevices"), Authorize]
    public async Task<ActionResult> GetCompanyDevices()
    {
        if (_userInfo.SelectedCompanyId != 0)
        {
            var devices = await _dbContext.Device.Where(x => x.CompanyId == _userInfo.SelectedCompanyId).ToListAsync();

            if (devices != null && devices.Count > 0)
                return Ok(devices);
            else
                return NotFound();
        }

        return BadRequest();

    }

    [HttpGet("DeviceSwitches/{deviceId}"), Authorize]
    public async Task<ActionResult> GetDeviceSwitches(int deviceId)
    {
        if (_userInfo.SelectedCompanyId != 0)
        {
            var companyDevices = await _dbContext.Device.FirstOrDefaultAsync(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Id == deviceId);
            if (companyDevices == null)
            {
                return NotFound();
            }

            var deviceSwitches = await _dbContext.Switch
                .Where(x => x.DeviceId == deviceId)
                .ToListAsync();


            if (deviceSwitches == null || deviceSwitches.Count == 0)
            {
                return NotFound();
            }

            var firstNoneActionTypeSwitch = deviceSwitches
                .FirstOrDefault(x => x.DeviceActionType == DeviceActionType.None);

            var otherActionTypeSwitches = deviceSwitches
                .Where(x => x.DeviceActionType != DeviceActionType.None)
                .ToList();

            var result = new List<Switch>();
            if (firstNoneActionTypeSwitch != null)
            {
                result.Add(firstNoneActionTypeSwitch);
            }
            result.AddRange(otherActionTypeSwitches);

            if (result.Count > 0)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        return BadRequest();

    }


    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<Device>> GetDevice(int id)
    {
        var device = new Device();

        if (await _dbContext.Device.AnyAsync(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Id == id))
        {
            device = await _dbContext.Device.FirstOrDefaultAsync(x => x.Id == id);
        }

        if (device == null)
        {
            return NotFound();
        }

        return device;
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> PostDevice([FromBody] DeviceViewModel deviceViewModel)
    {
        if (!_userInfo.IsAdmin
            && _userInfo.UserLevel != Level.Moderator)
        {
            return BadRequest("You are not allowed to create devices");
        }

        if (await _dbContext.Device.AnyAsync(x => x.Name == deviceViewModel.Name && x.CompanyId == _userInfo.SelectedCompanyId))
        {
            return BadRequest("Device with this name already exists");
        }

        var device = new Device
        {
            Name = deviceViewModel.Name,
            FuseboxForcedOff = deviceViewModel.FuseboxForcedOff,
            FuseboxForcedOn = deviceViewModel.FuseboxForcedOn,
            MaxStopHoursIn24h = deviceViewModel.MaxStopHoursIn24h,
            MaxStopHoursConsecutive = deviceViewModel.MaxStopHoursConsecutive,
            MaxForcedOnHoursIn24h = deviceViewModel.MaxForcedOnHoursIn24h,
            ForcedOnPercentageForComingHourToEnable = deviceViewModel.ForcedOnPercentageForComingHourToEnable,
            ForcedOn = deviceViewModel.ForcedOn,
            ForcedOff = deviceViewModel.ForcedOff,
            MediumOn = deviceViewModel.MediumOn,
            TemperatureInStandardMode = deviceViewModel.TemperatureInStandardMode,
            TemperatureInForcedOnMode = deviceViewModel.TemperatureInForcedOnMode,
            FirstHourPercentageKwPriceRequirementBeforeHeating = deviceViewModel.FirstHourPercentageKwPriceRequirementBeforeHeating,
            CompanyId = (int)_userInfo.SelectedCompanyId,
            AutoModeEnabled = deviceViewModel.AutoModeEnabled,
        };

        _dbContext.Device.Add(device);

        await _dbContext.SaveChangesAsync();

        var sensor = await _dbContext.Sensor.FirstOrDefaultAsync(x => x.Id == deviceViewModel.sensorViewModel!.Id);

        if (sensor != null)
        {
            sensor.DeviceId = device.Id;
            sensor.DeviceActionType = deviceViewModel.sensorViewModel.DeviceActionType;
            sensor.Description = deviceViewModel.sensorViewModel.Description;
            sensor.CompanyId = device.CompanyId;

            _dbContext.Entry(sensor).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            var sensorSwitch = await _dbContext.Switch.Where(x => x.SensorId == sensor.Id).ToListAsync();

            if (sensorSwitch.Count > 0)
            {
                foreach (var item in sensorSwitch)
                {
                    item.DeviceId = device.Id;
                    item.CompanyId = device.CompanyId;
                }

                await _dbContext.SaveChangesAsync();
            }
        }

        return Ok(device);
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateDevice(int id, DeviceViewModel deviceViewModel)
    {
        if (id != deviceViewModel.Id)
        {
            return BadRequest();
        }


        if (!_userInfo.IsAdmin && deviceViewModel.CompanyId != _userInfo.SelectedCompanyId)
        {
            return BadRequest();
        }

        if (await _dbContext.Device.AnyAsync(x => x.Name == deviceViewModel.Name
        && x.CompanyId == deviceViewModel.CompanyId
        && x.Id != deviceViewModel.Id))
        {
            return BadRequest("Device with this name already exists");
        }

        var dbDevice = await _dbContext.Device.FindAsync(id);

        if (dbDevice != null)
        {
            dbDevice.Name = deviceViewModel.Name;
            dbDevice.FuseboxForcedOff = deviceViewModel.FuseboxForcedOff;
            dbDevice.FuseboxForcedOn = deviceViewModel.FuseboxForcedOn;
            dbDevice.MaxStopHoursIn24h = deviceViewModel.MaxStopHoursIn24h;
            dbDevice.MaxStopHoursConsecutive = deviceViewModel.MaxStopHoursConsecutive;
            dbDevice.MaxForcedOnHoursIn24h = deviceViewModel.MaxForcedOnHoursIn24h;
            dbDevice.ForcedOnPercentageForComingHourToEnable = deviceViewModel.ForcedOnPercentageForComingHourToEnable;
            dbDevice.ForcedOn = deviceViewModel.ForcedOn;
            dbDevice.ForcedOff = deviceViewModel.ForcedOff;
            dbDevice.MediumOn = deviceViewModel.MediumOn;
            dbDevice.TemperatureInStandardMode = deviceViewModel.TemperatureInStandardMode;
            dbDevice.TemperatureInForcedOnMode = deviceViewModel.TemperatureInForcedOnMode;
            dbDevice.FirstHourPercentageKwPriceRequirementBeforeHeating = deviceViewModel.FirstHourPercentageKwPriceRequirementBeforeHeating;
            dbDevice.AutoModeEnabled = deviceViewModel.AutoModeEnabled;

            _dbContext.Entry(dbDevice).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                return BadRequest();

            }

            return Ok(dbDevice);
        }

        else
        {
            return NotFound();
        }
    }
}
