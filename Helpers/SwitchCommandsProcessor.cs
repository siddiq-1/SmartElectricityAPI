using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet.Server;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Migrations;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using System.Diagnostics;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace SmartElectricityAPI.Helpers;

public class SwitchCommandsProcessor
{
    private RedisCacheService redisCacheService;
    private readonly IMqttLogger _mqttLogger;
    private IMqttClient _mqttClient;
    private MySQLDBContext _dbContext;
    private readonly DateTime dateTime;

    public SwitchCommandsProcessor(RedisCacheService redisCacheService, IMqttLogger mqttLogger, IMqttClient mqttClient, MySQLDBContext dbContext, DateTime dateTime)
    {
        this.redisCacheService = redisCacheService;
        _mqttLogger = mqttLogger;
        _mqttClient = mqttClient;
        _dbContext = dbContext;
        this.dateTime = dateTime;
    }

    public async Task ProcessSwitchCommands()
    {
        using (var mySQLDBContextLocal = await new DatabaseService().CreateDbContextAsync())
        {
            var entitiesList = mySQLDBContextLocal.ChangeTracker.Entries().ToList();

            foreach (var entity in entitiesList)
            {
                entity.Reload();
            }

            var distinctRegions = await mySQLDBContextLocal.Company.Select(x => x.RegionId).Distinct().ToListAsync();
            var usedRegions = await mySQLDBContextLocal.Region.Where(x => distinctRegions.Contains(x.Id)).ToListAsync();
            var listOfSwitches = await mySQLDBContextLocal.Switch.ToListAsync();
            var listOfSensors = await mySQLDBContextLocal.Sensor.ToListAsync();
            var listOfDevices = await mySQLDBContextLocal.Device.ToListAsync();
            var switchModelParameters = await mySQLDBContextLocal.SwitchModelParameters.ToListAsync();
            
            foreach (var region in usedRegions)
                {
                    DateTime signalTime = dateTime.AddHours(-region.OffsetHoursFromEstonianTime);

                var joinedData = await mySQLDBContextLocal.SpotPrice
                    .Where(x => x.DateTime.Year == signalTime.Year
                                && x.DateTime.Month == signalTime.Month
                                && x.DateTime.Day == signalTime.Day
                                && x.DateTime.Hour == signalTime.Hour
                                && x.RegionId == region.Id)
                    .Join(
                        mySQLDBContextLocal.DeviceCompanyHours,
                        spotPrice => spotPrice.Id,
                        deviceCompanyHours => deviceCompanyHours.SpotPriceId,
                        (spotPrice, deviceCompanyHours) => new
                        {
                            SpotPrice = spotPrice,
                            deviceCompanyHours
                        })
                    .Join(
                        mySQLDBContextLocal.Device,
                        joined => joined.deviceCompanyHours.DeviceId,
                        device => device.Id,
                        (joined, device) => new
                        {
                            joined.SpotPrice,
                            joined.deviceCompanyHours,
                            Device = device
                        })
                    .Where(x => x.deviceCompanyHours.IsProcessed == false && x.Device.AutoModeEnabled)
                  //    .Where(x => x.Device.AutoModeEnabled)
                    .OrderBy(x => x.deviceCompanyHours.CompanyId)
                    .ThenBy(x => x.deviceCompanyHours.DeviceId)
                    .ToListAsync();


                foreach (var item in joinedData)
                    {
                        var relatedSensors = listOfSensors.Where(s => s.DeviceId == item.deviceCompanyHours.DeviceId).ToList();


                    if (item.deviceCompanyHours.DeviceActionType == DeviceActionType.None)
                    {
                        foreach (var sensor in relatedSensors)
                        {
                            var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == sensor.DeviceId && x.DeviceActionType == DeviceActionType.None).ToListAsync();

                            foreach (var switches in relatedSwitches)
                            {
                                var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == switches.SwitchModelId && x.DeviceActionType == switches.DeviceActionType);

                                if (selectedParameters != null)
                                {
                                    await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(sensor.Topic, selectedParameters.Payload);

                                    switches.ActionState = true;
                                    _dbContext.Entry(switches).State = EntityState.Modified;
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                        }

                        foreach (var sensor in relatedSensors)
                        {
                            var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == sensor.DeviceId && x.DeviceActionType != DeviceActionType.None).ToListAsync();

                            relatedSwitches.ForEach(x => x.ActionState = false);

                            foreach (var relatedSwitch in relatedSwitches)
                            {
                                _dbContext.Entry(relatedSwitch).State = EntityState.Modified;
                            }

                            await _dbContext.SaveChangesAsync();
                        }
                    }



                    if (item.deviceCompanyHours.DeviceActionType != DeviceActionType.None)
                    {                
                        var toggleNoneSensors = listOfSensors.Where(s => s.DeviceId == item.deviceCompanyHours.DeviceId).ToList();

                        foreach (var sensorActive in toggleNoneSensors)
                        {
                            var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == sensorActive.DeviceId && x.DeviceActionType == DeviceActionType.None).ToListAsync();

                            foreach (var switches in relatedSwitches)
                            {
                                var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == switches.SwitchModelId && x.DeviceActionType == switches.DeviceActionType);

                                if (selectedParameters != null)
                                {
                                    await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(sensorActive.Topic, selectedParameters.Payload);

                                    switches.ActionState = false;
                                    _dbContext.Entry(switches).State = EntityState.Modified;
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                        }

                        var toggleActionSensors = relatedSensors.Where(x =>  x.DeviceActionType == item.deviceCompanyHours.DeviceActionType).ToList();

                        foreach (var actionSensor in toggleActionSensors)
                        {
                            var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == actionSensor.DeviceId && x.DeviceActionType == item.deviceCompanyHours.DeviceActionType).ToListAsync();

                            foreach (var switches in relatedSwitches)
                            {
                                var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == switches.SwitchModelId && x.DeviceActionType == switches.DeviceActionType);

                                if (selectedParameters != null)
                                {
                                    await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(actionSensor.Topic, selectedParameters.Payload);

                                    switches.ActionState = true;
                                    _dbContext.Entry(switches).State = EntityState.Modified;
                                    await _dbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }



                    /*
                    foreach (var sensor in relatedSensors.Where(x => x.DeviceActionType != item.deviceCompanyHours.DeviceActionType).ToList())
                    {
                        var toggleSwitch = listOfSwitches.FirstOrDefault(x => x.SensorId == sensor.Id && x.DeviceActionType == DeviceActionType.None);

                        if (toggleSwitch != null)
                        {
                            var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == toggleSwitch.SwitchModelId && x.DeviceActionType == toggleSwitch.DeviceActionType);

                            if (selectedParameters != null)
                            {
                                await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(sensor.Topic, selectedParameters.Payload);

                                toggleSwitch.ActionState = true;
                                mySQLDBContextLocal.Entry(toggleSwitch).State = EntityState.Modified;
                                await mySQLDBContextLocal.SaveChangesAsync();
                            }                               
                        }                    
                    }

                    foreach (var sensor in relatedSensors)
                    {
                        var toggleSwitches = listOfSwitches.Where(x => x.SensorId == sensor.Id && x.DeviceActionType != DeviceActionType.None).ToList();
                        toggleSwitches.ForEach(x => x.ActionState = false);

                        foreach (var toggleSwitch in toggleSwitches)
                        {
                            mySQLDBContextLocal.Entry(toggleSwitch).State = EntityState.Modified;
                        }

                        await mySQLDBContextLocal.SaveChangesAsync();
                    }


                    foreach (var sensor in relatedSensors.Where(x=> x.DeviceActionType == item.deviceCompanyHours.DeviceActionType).ToList())
                    {
                        var toggleSwitch = listOfSwitches.FirstOrDefault(x => x.SensorId == sensor.Id && x.DeviceActionType == sensor.DeviceActionType);

                        if (toggleSwitch != null)
                        {

                            var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == toggleSwitch.SwitchModelId && x.DeviceActionType == sensor.DeviceActionType);

                            if (selectedParameters != null)
                            {
                                await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(sensor.Topic, selectedParameters.Payload);


                                toggleSwitch.ActionState = true;
                                mySQLDBContextLocal.Entry(toggleSwitch).State = EntityState.Modified;
                                await mySQLDBContextLocal.SaveChangesAsync();
                            }                                
                        }

                    }
                    */

                    var selectedDeviceControlHour = await mySQLDBContextLocal.DeviceCompanyHours.FirstOrDefaultAsync(x => x.Id == item.deviceCompanyHours.Id);

                        if (selectedDeviceControlHour != null)
                        {
                            selectedDeviceControlHour.IsProcessed = true;
                        }

                        mySQLDBContextLocal.SaveChanges();                    
                    }
                }         
        }
    }
}
