using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Processors;
using SmartElectricityAPI.Services;
using MQTTnet.Server;
using Polly;
using System.Security.Authentication;
using SmartElectricityAPI.Helpers;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SwitchActionController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    private readonly IMqttLogger _mqttLogger;
    private readonly IMapper _mapper;
    private readonly RedisCacheService redisCacheService;

    public SwitchActionController(MySQLDBContext context, IUserInfo userInfo, IMqttLogger mqttLogger, IMapper mapper)
    {
        _dbContext = context;
        _userInfo = userInfo;
        _mqttLogger = mqttLogger;
        _mapper = mapper;
        redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());
    }
    [HttpGet("SendCommand/{id}"), Authorize]
    public async Task<ActionResult> SendSwitchAction(int id)
    {
        var switchAction = _dbContext.Switch.FirstOrDefault(x => x.Id == id && x.CompanyId == _userInfo.SelectedCompanyId);

        if (switchAction == null)
        {
            return NotFound();
        }
        else
        {
            var mqttFactory = new MqttFactory();

            IMqttClient mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder().
                WithTcpServer(CredentialHelpers.MqttServerBasicSSLSettings().Server, CredentialHelpers.MqttServerBasicSSLSettings().Port)
                .WithTlsOptions(o =>
                {
                    o.Build().UseTls = true;
                    o.WithSslProtocols(SslProtocols.Tls12)
                       .WithAllowUntrustedCertificates(true)
            .WithIgnoreCertificateChainErrors(true)
            .WithIgnoreCertificateRevocationErrors(true);
                    o.WithCertificateValidationHandler(
                    eventArgs =>
                    {
                        eventArgs.Certificate.Subject.ToString();
                        eventArgs.Certificate.GetExpirationDateString().ToString();
                        eventArgs.Chain.ChainPolicy.RevocationMode.ToString();
                        eventArgs.Chain.ChainStatus.ToString();
                        eventArgs.SslPolicyErrors.ToString();
                        return true;
                    });
                })
                .WithCredentials(CredentialHelpers.MqttServerBasicSSLSettings().Username, CredentialHelpers.MqttServerBasicSSLSettings().Password)
                .Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            if (mqttClient.IsConnected)
            {
                var switchModelParameters = await _dbContext.SwitchModelParameters.ToListAsync();

                var relatedSenors = await _dbContext.Sensor.Where(x => x.DeviceId == switchAction.DeviceId).ToListAsync();

                if (switchAction.DeviceActionType == Enums.DeviceActionType.None)
                {                
                    foreach (var sensor in relatedSenors)
                    {
                        var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == sensor.DeviceId && x.DeviceActionType == Enums.DeviceActionType.None).ToListAsync();

                        foreach (var switches in relatedSwitches)
                        {
                            var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == switches.SwitchModelId && x.DeviceActionType == switches.DeviceActionType);

                            if (selectedParameters != null)
                            {
                                await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(sensor.Topic, selectedParameters.Payload);

                                switches.ActionState = true;
                                _dbContext.Entry(switches).State = EntityState.Modified;
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }

                    foreach (var sensor in relatedSenors)
                    {
                        var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == sensor.DeviceId && x.DeviceActionType != Enums.DeviceActionType.None).ToListAsync();

                        relatedSwitches.ForEach(x => x.ActionState = false);

                        foreach (var relatedSwitch in relatedSwitches)
                        {
                            _dbContext.Entry(relatedSwitch).State = EntityState.Modified;
                        }

                        await _dbContext.SaveChangesAsync();
                    }
                }

                if (switchAction.DeviceActionType != Enums.DeviceActionType.None)
                {
                    var toggleNoneSensors = relatedSenors.Where(x => x.Id == switchAction.SensorId).ToList();

                    foreach (var sensorAction in toggleNoneSensors)
                    {
                        var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == sensorAction.DeviceId && x.DeviceActionType == Enums.DeviceActionType.None).ToListAsync();

                        foreach (var switches in relatedSwitches)
                        {
                            var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == switches.SwitchModelId && x.DeviceActionType == switches.DeviceActionType);

                            if (selectedParameters != null)
                            {
                                await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(sensorAction.Topic, selectedParameters.Payload);

                                switches.ActionState = false;
                                _dbContext.Entry(switches).State = EntityState.Modified;
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }

                    var toggleActionSensors = relatedSenors.Where(x => x.Id == switchAction.SensorId && x.DeviceActionType == switchAction.DeviceActionType).ToList();

                    foreach (var actionSensor in toggleActionSensors)
                    {
                        var relatedSwitches = await _dbContext.Switch.Where(x => x.DeviceId == actionSensor.DeviceId && x.DeviceActionType == switchAction.DeviceActionType).ToListAsync();

                        foreach (var switches in relatedSwitches)
                        {
                            var selectedParameters = switchModelParameters.FirstOrDefault(x => x.SwitchModelId == switches.SwitchModelId && x.DeviceActionType == switches.DeviceActionType);

                            if (selectedParameters != null)
                            {
                                await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(actionSensor.Topic, selectedParameters.Payload);

                                switches.ActionState = true;
                                _dbContext.Entry(switches).State = EntityState.Modified;
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
        }

        return Ok();
    }
}
