using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet;
using Newtonsoft.Json;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using Polly;
using SmartElectricityAPI.Models.FuseboxV2;
using SmartElectricityAPI.Models.Fusebox;
namespace SmartElectricityAPI.Processors;

public class InverterBatteryButtonProcessor
{
    private Company _company;
    private int _inverterTypeCompanyActionId;
    private RedisCacheService _redisCacheService;
    private IMqttLogger _mqttLogger;
    public InverterBatteryButtonProcessor(Company company, int inverterTypeCompanyActionId, RedisCacheService redisCacheService, IMqttLogger mqttLogger)
    {
        _company = company;
        _inverterTypeCompanyActionId = inverterTypeCompanyActionId;
        _redisCacheService = redisCacheService;
        _mqttLogger = mqttLogger;
    }
    public async Task Process()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            if (await _dbContext.InverterTypeCompanyActions.AnyAsync(x => x.Id == _inverterTypeCompanyActionId && x.CompanyId == _company.Id))
            {
                bool modeChanged = false;
                var selectedRecord = await _dbContext.InverterTypeCompanyActions.FirstOrDefaultAsync(x => x.Id == _inverterTypeCompanyActionId);

                var inverterTypeCommands = await _dbContext.InverterTypeCommands.Where(x => x.InverterTypeId == selectedRecord.InverterTypeId).ToListAsync();
                var selectedInverter = await _dbContext.Inverter.Include(x => x.RegisteredInverter).FirstOrDefaultAsync(x => x.Id == selectedRecord.InverterId);
                var registeredInverter = await _dbContext.RegisteredInverter.FirstOrDefaultAsync(x => x.Id == selectedInverter.RegisteredInverterId);
                var inverterBattery = await _dbContext.InverterBattery.Include(x => x.Inverter).FirstOrDefaultAsync(x => x.InverterId == selectedRecord.InverterId);
                var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == _company.Id);
                var currentDateTime = DateTime.Now;


                if (selectedRecord.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun
                    || selectedRecord.ActionTypeCommand == ActionTypeCommand.ChargeMax)
                {
                    var last5StateTransactions = await _redisCacheService.GetKeyValue<SofarState>(registeredInverter.Id.ToString(), Constants.SofarStateUnixOffset);
                    last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(2).ToList();


                    if (last5StateTransactions.Count > 0 && last5StateTransactions.Average(x => x.batterySOC) >= inverterBattery.MaxLevel)
                    {
                        selectedRecord = await _dbContext.InverterTypeCompanyActions.FirstOrDefaultAsync(x => x.CompanyId == _company.Id && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);
                        modeChanged = true;
                    }
                }

                if (selectedRecord!.ActionTypeCommand != ActionTypeCommand.AutoMode)
                {
                    selectedRecord.ActionState = true;

                }
                else
                {
                    selectedRecord.ActionState = !selectedRecord.ActionState;
                }

                if (selectedRecord.ActionTypeCommand != ActionTypeCommand.HzMarket)
                {
                    await LogEndTime();
                }

                var recordsToUpdate = await _dbContext.InverterTypeCompanyActions.Where(x => x.InverterId == selectedRecord.InverterId && x.Id != selectedRecord.Id).ToListAsync();

                if (selectedRecord.ActionType != ActionType.Automode)
                {
                    foreach (var record in recordsToUpdate)
                    {
                        if (record.ActionTypeCommand != ActionTypeCommand.AutoMode)
                        {
                            record.ActionState = false;
                            _dbContext.Entry(record).State = EntityState.Modified;
                        }
                    }
                }

                if (selectedRecord.ActionType == ActionType.Automode
                    && selectedRecord.ActionState)
                {
                    var currentHourActiveCommand = await _dbContext.BatteryControlHours.Include(x => x.SpotPriceMax)
                        .Where(x => x.SpotPriceMax.Date >= DateOnly.FromDateTime(DateTime.Now))
                        .FirstOrDefaultAsync(x => x.InverterBatteryId == inverterBattery.Id);

                    var commandToUse = await _dbContext.InverterTypeCompanyActions.FirstOrDefaultAsync(x=> x.CompanyId == _company.Id && x.ActionTypeCommand == currentHourActiveCommand.ActionTypeCommand && x.InverterId == selectedRecord.InverterId);

                    var inverterBatteryButtonProcessor = new InverterBatteryButtonProcessor(company, commandToUse.Id, _redisCacheService, _mqttLogger);
                    await inverterBatteryButtonProcessor.Process();

                }

                await _dbContext.SaveChangesAsync();

                var mqttFactory = new MqttFactory();

                IMqttClient mqttClient = mqttFactory.CreateMqttClient();

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(CredentialHelpers.MqttServerBasicSettings().Server, CredentialHelpers.MqttServerBasicSettings().Port)
                    .WithCredentials(CredentialHelpers.MqttServerBasicSettings().Username, CredentialHelpers.MqttServerBasicSettings().Password)
                    .WithCleanSession().Build();
                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    if (mqttClient.IsConnected)
                    {
                        string mqttTopic = "";
                        string payLoad = "";
                        ActionType returnedActionType = ActionType.None;

                        switch (selectedRecord.ActionTypeCommand)
                        {
                            case ActionTypeCommand.ChargeMax:
                                if (selectedRecord.ActionType == ActionType.Charge)
                                {
                                    if (selectedInverter.UseInverterSelfUse)
                                    {
                                        BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                                        await batteryButtonActions.ProcessInverterModeControl(selectedInverter, _dbContext, ActionTypeCommand.PassiveMode);
                                    }

                                    var spotPrice = await Find(selectedInverter);
                                    //Send antireflux command
                                    var mqttTopicForAntiReflux = await _dbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                    x.InverterTypeId == selectedInverter.InverterTypeId
                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                                    string antiRefluxTopic = $"{registeredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                                    string antiRefluxPayLoad = "0";

                                    if (spotPrice.PriceNoTax > company!.BrokerPurchaseMargin)
                                    {
                                        antiRefluxPayLoad = InverterHelper.SofarThreePhaseAntiRefluxPayload(selectedInverter.MaxSalesPowerCapacity).ToString();
                                    }

                                    await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayLoad)
                                    .Result.LogMessage(_dbContext, selectedInverter.Id, Direction.Out, MqttMessageOrigin.ManualButtonAction, antiRefluxTopic, antiRefluxPayLoad, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);

                                    mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == selectedInverter.InverterTypeId).MqttTopic}";
                                    payLoad = $"{(inverterBattery.ChargingPowerFromGridKWh * 1000) / selectedInverter.NumberOfInverters}";
                                }
                                break;

                            case ActionTypeCommand.ConsumeBatteryWithMaxPower:
                                if (selectedRecord.ActionType == ActionType.Discharge)
                                {
                                    if (selectedInverter.UseInverterSelfUse)
                                    {
                                        BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                                        await batteryButtonActions.ProcessInverterModeControl(selectedInverter, _dbContext, ActionTypeCommand.PassiveMode);
                                    }

                                    //Send antireflux command
                                    var mqttTopicForAntiReflux = await _dbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                    x.InverterTypeId == selectedInverter.InverterTypeId
                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                                    string antiRefluxTopic = $"{registeredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                                    string antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(selectedInverter.MaxSalesPowerCapacity).ToString();
                                    await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                                        .Result.LogMessage(_dbContext, selectedInverter.Id, Direction.Out, MqttMessageOrigin.ManualButtonAction, antiRefluxTopic, InverterHelper.SofarThreePhaseAntiRefluxPayload(selectedInverter.MaxSalesPowerCapacity).ToString(), ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);

                                    mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == selectedInverter.InverterTypeId).MqttTopic}";
                                    payLoad = $"{(inverterBattery.Inverter.MaxPower * 1000) / selectedInverter.NumberOfInverters}";
                                }
                                break;

                            case ActionTypeCommand.SelfUse:
                                if (selectedRecord.ActionType == ActionType.Discharge)
                                {
                                    if (selectedInverter.UseInverterSelfUse)
                                    {
                                        BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                                        await batteryButtonActions.ProcessInverterModeControl(selectedInverter, _dbContext, ActionTypeCommand.PassiveMode);
                                    }

                                    //Send antireflux command
                                    var mqttTopicForAntiReflux = await  _dbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                    x.InverterTypeId == selectedInverter.InverterTypeId
                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);
                                    string antiRefluxTopic = $"{registeredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                                    string antiRefluxPayLoad = "0";

                                    await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayLoad)
                                    .Result.LogMessage(_dbContext, selectedInverter.Id, Direction.Out, MqttMessageOrigin.ManualButtonAction, antiRefluxTopic, antiRefluxPayLoad, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);

                                    mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == selectedInverter.InverterTypeId).MqttTopic}";
                                    payLoad = $"{(inverterBattery.Inverter.MaxPower * 1000) / selectedInverter.NumberOfInverters}";
                                }
                                break;

                            case ActionTypeCommand.SellRemainingSunNoCharging:
                                if (selectedRecord.ActionType == ActionType.Charge)
                                {
                                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                                    if (selectedInverter.UseInverterSelfUse)
                                    {
                                        await batteryButtonActions.ProcessInverterModeControl(selectedInverter, _dbContext, ActionTypeCommand.PassiveMode);
                                    }
                                    var spotPrice = await Find(selectedInverter);

                                    DateTime previousHourDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0).AddHours(-1);

                                    (payLoad, returnedActionType) = await batteryButtonActions.ProcessSellRemainingSunNoCharging(previousHourDateTime, spotPrice, spotPrice, selectedInverter, inverterBattery, company, _dbContext, false, true, ActionTypeCommand.SellRemainingSunNoCharging);

                                    var last5StateTransactions = await _redisCacheService.GetKeyValue<SofarState>(registeredInverter.Id.ToString(), Constants.SofarStateUnixOffset);
                                        last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(2).ToList();

                                    if (last5StateTransactions.Count > 0 && last5StateTransactions.Average(x => x.batterySOC) >= 50 && payLoad == "0")
                                    {
                                        mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == selectedInverter.InverterTypeId).MqttTopic}";
                                    }
                                    else
                                    {
                                        mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == selectedInverter.InverterTypeId).MqttTopic}";
                                    }
                                }
                                break;
                            case ActionTypeCommand.ChargeWithRemainingSun:

                                if (selectedInverter.UseInverterSelfUse)
                                {
                                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                                    await batteryButtonActions.ProcessInverterModeControl(selectedInverter, _dbContext, ActionTypeCommand.PassiveMode);
                                }

                                var mqttTopicForAntiRefluxRemSun = await _dbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                    x.InverterTypeId == selectedInverter.InverterTypeId
                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                                string antiRefluxTopicRemSun = $"{registeredInverter.Name}{mqttTopicForAntiRefluxRemSun!.MqttTopic}";
                                string antiRefluxPayLoadRemSun = InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterBattery.Inverter.MaxSalesPowerCapacity).ToString();

                                await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopicRemSun, antiRefluxPayLoadRemSun)
                                    .Result.LogMessage(_dbContext, selectedInverter.Id, Direction.Out, MqttMessageOrigin.ManualButtonAction, antiRefluxTopicRemSun, antiRefluxPayLoadRemSun, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);

                                mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == selectedInverter.InverterTypeId).MqttTopic}";
                                payLoad = "0";
                                break;
                            case ActionTypeCommand.InverterSelfUse:

                                if (selectedInverter.UseInverterSelfUse)
                                {
                                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                                    await batteryButtonActions.ProcessInverterModeControl(selectedInverter, _dbContext, ActionTypeCommand.InverterSelfUse, true);
                                }
                                break;
                        }

                        MqttClientPublishResult pubresults = null;

                        if (!string.IsNullOrWhiteSpace(mqttTopic) && !string.IsNullOrWhiteSpace(payLoad) && selectedRecord != null)
                        {
                            await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(mqttTopic, payLoad).Result
                                .LogMessage(_dbContext, selectedRecord.InverterId, Direction.Out, MqttMessageOrigin.ManualButtonAction, mqttTopic, payLoad, selectedRecord.ActionTypeCommand, true, MQttMessageType.Regular);
                        }

                        await mqttClient.DisconnectAsync();


                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

    }
    private async Task<SpotPrice> Find(Inverter inverter)
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var currentDateTime = DateTime.Now;

            var inverterCompanyHour = await _dbContext.SpotPrice
             .Where(x => x.DateTime.Year == currentDateTime.Year
                     && x.DateTime.Month == currentDateTime.Month
                     && x.DateTime.Day == currentDateTime.Day
                     && x.DateTime.Hour == currentDateTime.Hour)
            .Join(
                _dbContext.InverterCompanyHours,
                spotPrice => spotPrice.Id,
                offHour => offHour.SpotPriceId,
                (spotPrice, offHour) => new
                {
                    SpotPrice = spotPrice,
                    OffHour = offHour
                })
            .Where(x => x.OffHour.InverterId == inverter.Id)
            .FirstOrDefaultAsync();

            return inverterCompanyHour.SpotPrice;
        }
    }

    private async Task LogEndTime()
    {
        //TODO: Change ID to correct one
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var fuseboxSchedRegMsg = await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader).
            Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
            && x.FuseBoxMessageHeader.device_id == "f2c01etbmxa8"
            && x.actualStart != null
            && x.actualEnd == null
            && x.cancel == false).FirstOrDefaultAsync();

            if (fuseboxSchedRegMsg != null)
            {
                fuseboxSchedRegMsg!.actualEnd = DateTimeOffset.Now.ToUnixTimeSeconds();

                await dbContext.SaveChangesAsync();
            }        
        }
    }
}
