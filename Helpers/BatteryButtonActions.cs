using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models;
using MQTTnet;
using MQTTnet.Client;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Services;
using MQTTnet.Adapter;
using MQTTnet.Exceptions;
using SmartElectricityAPI.Dto;
using Polly;

namespace SmartElectricityAPI.Helpers;

public class BatteryButtonActions
{

    private readonly IMqttLogger _mqttLogger;
    private readonly RedisCacheService _redisCacheService;
    //private readonly IConfiguration _configuration;
    private IMqttClient mqttClient;
    private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1, 1);

    public BatteryButtonActions(IMqttLogger mqttLogger, RedisCacheService redisCacheService, IMqttClient existingMqttClient = null)
    {
        _mqttLogger = mqttLogger;
        _redisCacheService = redisCacheService;
        this.mqttClient = existingMqttClient;

  
    }

    private async Task EnsureConnected()
    {
        if (mqttClient?.IsConnected == true) return;

        await connectionLock.WaitAsync();
        try
        {
            // Double-check pattern
            if (mqttClient?.IsConnected == true) return;

            await ConnectMqttClient();
        }
        finally
        {
            connectionLock.Release();
        }
    }

    private async Task ConnectMqttClient()
    {
        var retryPolicy = Policy
            .Handle<MqttConnectingFailedException>()
            .Or<MqttCommunicationException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    
                }
            );

        try
        {
            if (mqttClient == null)
            {
                var mqttFactory = new MqttFactory();
                mqttClient = mqttFactory.CreateMqttClient();
            }

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(CredentialHelpers.MqttServerBasicSettings().Server, CredentialHelpers.MqttServerBasicSettings().Port)
                .WithCredentials(CredentialHelpers.MqttServerBasicSettings().Username, CredentialHelpers.MqttServerBasicSettings().Password)
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                .WithTimeout(TimeSpan.FromSeconds(10))
                .Build();

            await retryPolicy.ExecuteAsync(async () =>
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None)
            );

            mqttClient.DisconnectedAsync += async e =>
            {
             
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await EnsureConnected();
                }
                catch (Exception ex)
                {
                
                }
            };
        }
        catch (Exception ex)
        {
   
            throw;
        }
    }


    public async Task<string> ProcessSelfUse(Inverter inverterIdWithName, InverterBattery inverterBattery, SpotPrice spotPrice, MySQLDBContext mySQLDBContextLocal, bool forceSendCommand = false)
    {
        string payLoad = "0";



        //Send antireflux command
        var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
        x.InverterTypeId == inverterIdWithName.InverterTypeId
        && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

        string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";

        var mqttLogFromRedis = await _redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

        var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                x.InverterId == inverterIdWithName.Id
                                                && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

        bool shouldDispatchCommand = true;

        if (lastLoqRecord != null
            && lastLoqRecord.InverterId == inverterIdWithName.Id
            && lastLoqRecord.Topic == antiRefluxTopic
            && lastLoqRecord.Payload == "0"
            && lastLoqRecord.MQttMessageType == MQttMessageType.ThreephaseAntireflux
            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
        {
            shouldDispatchCommand = false;
        }

        if (forceSendCommand)
        {
            shouldDispatchCommand = true;
        }

        if (shouldDispatchCommand)
        {
            //Update database to antireflux off for this hour
            var antiRefluxRecord = await mySQLDBContextLocal.InverterCompanyHours.FirstOrDefaultAsync(x =>
            x.CompanyId == inverterBattery!.Inverter.CompanyId
            && x.SpotPriceId == spotPrice.Id);

            if (antiRefluxRecord != null)
            {
                antiRefluxRecord!.ActionType = ActionType.ThreePhaseAntiRefluxOff;
            }
           

            await mySQLDBContextLocal.SaveChangesAsync();
        }
        await EnsureConnected();
        await new MqttClientService(mqttClient, _mqttLogger, shouldDispatchCommand).PublishMessages(antiRefluxTopic, "0")
        .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, "0", ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommand, MQttMessageType.ThreephaseAntireflux);



        var last5StateTransactions = await _redisCacheService.GetKeyValue<SofarState>(inverterIdWithName.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
        last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(5).ToList();
        var averageBatteryProduction = 0;
        var averageInverterConsumption = 0;
        if (last5StateTransactions != null && last5StateTransactions.Count > 0)
        {
            averageBatteryProduction = last5StateTransactions.Sum(x => x.solarPV) / last5StateTransactions.Count;
            averageInverterConsumption = last5StateTransactions.Sum(x => x.consumption) / last5StateTransactions.Count;
        }
        //TODO: Lisada juurde ka kontroll, et kui päike ületab
        if (last5StateTransactions != null
            && last5StateTransactions.Count > 0
            && inverterBattery != null
            && (last5StateTransactions.FirstOrDefault()!.batterySOC > inverterBattery.MinLevel
            || averageBatteryProduction > averageInverterConsumption))
        {
            payLoad = ((Convert.ToInt32(inverterBattery!.Inverter.MaxPower) * 1000) / inverterIdWithName.NumberOfInverters).ToString();
        }            

        return payLoad;
    }

    public async Task<(string, ActionType)> ProcessSellRemainingSunNoCharging(DateTime previousHourDateTime, SpotPrice currentSpotPrice, SpotPrice spotPrice, Inverter inverterIdWithName, InverterBattery inverterBattery, Company companies, MySQLDBContext mySQLDBContextLocal, bool checkPreviousHour = true, bool forceSendCommand = false, ActionTypeCommand ActionTypeCommandOrigin = ActionTypeCommand.None, int averageBatteryProduction = 0)
    {
        if (checkPreviousHour)
        {
            var batteryDataForBatteryIdForPrevHour = await mySQLDBContextLocal.SpotPrice
              .Where(x => x.DateTime.Year == previousHourDateTime.Year
                      && x.DateTime.Month == previousHourDateTime.Month
                      && x.DateTime.Day == previousHourDateTime.Day
                      && x.DateTime.Hour == previousHourDateTime.Hour)

             .Join(
                 mySQLDBContextLocal.BatteryControlHours,
                 spotPrice => spotPrice.Id,
                 batteryHours => batteryHours.SpotPriceMaxId,
                 (spotPrice, batteryHours) => new
                 {
                     SpotPrice = spotPrice,
                     BatteryHours = batteryHours
                 })
             .Where(x => x.BatteryHours.InverterBatteryId == inverterBattery.Id)
             .FirstOrDefaultAsync();

            if (batteryDataForBatteryIdForPrevHour == null
                || batteryDataForBatteryIdForPrevHour.BatteryHours.ActionTypeCommand != ActionTypeCommand.SellRemainingSunNoCharging)
            {
                var brokerPurchaseMargin = companies.BrokerPurchaseMargin;
                var actionToSend = currentSpotPrice.PriceNoTax > brokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                if (actionToSend == ActionType.ThreePhaseAntiRefluxOn)
                {
                    //Send antireflux command
                    var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                    x.InverterTypeId == inverterIdWithName.InverterTypeId
                    && x.ActionType == actionToSend);

                    string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                    string antiRefluxPayload = actionToSend == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterBattery.Inverter.MaxSalesPowerCapacity).ToString() : "0";

                    var mqttLogFromRedis = await _redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                    x.InverterId == inverterIdWithName.Id
                                                                    && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                    DateTime currentDateTime = DateTime.Now;

                    bool shouldDispatchCommand = true;

                    if (lastLoqRecord != null
                        && lastLoqRecord.InverterId == inverterIdWithName.Id
                        && lastLoqRecord.Topic == antiRefluxTopic
                        && lastLoqRecord.Payload == antiRefluxPayload
                        && lastLoqRecord.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120)
                        && lastLoqRecord.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommand = false;
                    }

                    if (forceSendCommand)
                    {
                        shouldDispatchCommand = true;
                    }

                    if (shouldDispatchCommand)
                    {
                        //Update database antireflux
                        var antiRefluxRecord = await mySQLDBContextLocal.InverterCompanyHours.FirstOrDefaultAsync(x =>
                        x.CompanyId == inverterBattery!.Inverter.CompanyId
                        && x.SpotPriceId == spotPrice.Id);

                        if (antiRefluxRecord != null)
                        {
                            antiRefluxRecord!.ActionType = actionToSend;
                        }                     
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, _mqttLogger, shouldDispatchCommand).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                    .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommand, MQttMessageType.ThreephaseAntireflux);
                }
                else if (actionToSend == ActionType.ThreePhaseAntiRefluxOff
                    &&  ActionTypeCommandOrigin == ActionTypeCommand.ChargeWithRemainingSun)
                {
                    string chargePayload = "0";

                    if (averageBatteryProduction >= 50)
                    {
                        chargePayload = "100";
                    }

                    return (chargePayload, ActionType.Charge);
                }
                else if (actionToSend == ActionType.ThreePhaseAntiRefluxOff)
                {
                    //Send antireflux command
                    var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                    x.InverterTypeId == inverterIdWithName.InverterTypeId
                    && x.ActionType == actionToSend);

                    string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, "0")
                    .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, "0", ActionTypeCommand.SelfUse, true, MQttMessageType.ThreephaseAntireflux);

                }
                return ("0", ActionType.None);
            }
            else
            {
                var brokerPurchaseMargin = companies.BrokerPurchaseMargin;
                var actionToSend = currentSpotPrice.PriceNoTax > brokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                    x.InverterTypeId == inverterIdWithName.InverterTypeId
                    && x.ActionType == actionToSend);

                string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                string antiRefluxPayload = actionToSend == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterBattery.Inverter.MaxSalesPowerCapacity).ToString() : "0";

                if (actionToSend == ActionType.ThreePhaseAntiRefluxOn)
                {
                    //Send antireflux command

                    var mqttLogFromRedis = await _redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                        x.InverterId == inverterIdWithName.Id
                                                        && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                    bool shouldDispatchCommand = true;

                    if (lastLoqRecord != null
                        && lastLoqRecord.InverterId == inverterIdWithName.Id
                        && lastLoqRecord.Topic == antiRefluxTopic
                        && lastLoqRecord.Payload == antiRefluxPayload
                        && lastLoqRecord.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                    {
                        shouldDispatchCommand = false;
                    }

                    if (forceSendCommand)
                    {
                        shouldDispatchCommand = true;
                    }

                    if (shouldDispatchCommand)
                    {
                        //Update database antireflux
                        var antiRefluxRecord = await mySQLDBContextLocal.InverterCompanyHours.FirstOrDefaultAsync(x =>
                        x.CompanyId == inverterBattery!.Inverter.CompanyId
                        && x.SpotPriceId == spotPrice.Id);

                        if (antiRefluxRecord != null)
                        {
                            antiRefluxRecord!.ActionType = actionToSend;
                        }



                        await mySQLDBContextLocal.SaveChangesAsync();
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, _mqttLogger, shouldDispatchCommand).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                            .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommand, MQttMessageType.ThreephaseAntireflux);

                }
                else if (actionToSend == ActionType.ThreePhaseAntiRefluxOff
                        && ActionTypeCommandOrigin == ActionTypeCommand.ChargeWithRemainingSun)
                {
                    string chargePayload = "0";

                    if (averageBatteryProduction >= 50)
                    {
                        chargePayload = "100";
                    }

                    return (chargePayload, ActionType.Charge);
                }
                else
                {
                    if (forceSendCommand)
                    {
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                        .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, forceSendCommand, MQttMessageType.ThreephaseAntireflux);
                    }

                }

                return ("0", ActionType.None);
            }
        }
        else
        {
            var brokerPurchaseMargin = companies.BrokerPurchaseMargin;
            var actionToSend = currentSpotPrice.PriceNoTax > brokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

            var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                x.InverterTypeId == inverterIdWithName.InverterTypeId
                && x.ActionType == actionToSend);

            string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
            string antiRefluxPayload = actionToSend == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterBattery.Inverter.MaxSalesPowerCapacity).ToString() : "0";

            if (actionToSend == ActionType.ThreePhaseAntiRefluxOn)
            {
                //Send antireflux command

                var mqttLogFromRedis = await _redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                    x.InverterId == inverterIdWithName.Id
                                                    && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                bool shouldDispatchCommand = true;

                if (lastLoqRecord != null
                    && lastLoqRecord.InverterId == inverterIdWithName.Id
                    && lastLoqRecord.Topic == antiRefluxTopic
                    && lastLoqRecord.Payload == antiRefluxPayload
                    && lastLoqRecord.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                {
                    shouldDispatchCommand = false;
                }

                if (forceSendCommand)
                {
                    shouldDispatchCommand = true;
                }

                if (shouldDispatchCommand)
                {
                    //Update database antireflux
                    var antiRefluxRecord = await mySQLDBContextLocal.InverterCompanyHours.FirstOrDefaultAsync(x =>
                    x.CompanyId == inverterBattery!.Inverter.CompanyId
                    && x.SpotPriceId == spotPrice.Id);

                    if (antiRefluxRecord != null)
                    {
                        antiRefluxRecord!.ActionType = actionToSend;
                    }

                    await mySQLDBContextLocal.SaveChangesAsync();
                }
                await EnsureConnected();
                await new MqttClientService(mqttClient, _mqttLogger, shouldDispatchCommand).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                        .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommand, MQttMessageType.ThreephaseAntireflux);
            }
            else if (actionToSend == ActionType.ThreePhaseAntiRefluxOff
                    && ActionTypeCommandOrigin == ActionTypeCommand.ChargeWithRemainingSun)
            {
                string chargePayload = "0";

                if (averageBatteryProduction >= 50)
                {
                    chargePayload = "100";
                }

                return (chargePayload, ActionType.Charge);
            }
            else
            {
                if (forceSendCommand)
                {
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                    .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, forceSendCommand, MQttMessageType.ThreephaseAntireflux);
                }
            }

            return ("0", ActionType.None);
        }
    }

    public async Task ProcessInverterModeControl(Inverter inverter, MySQLDBContext mySqldbContext, ActionTypeCommand actionTypeCommand, bool sendAntiReflux = false)
    {
        var modeControlTopicCommand = await mySqldbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
            x.InverterTypeId == inverter.InverterTypeId
            && x.ActionType == ActionType.ModeControl);

        var company = await mySqldbContext.Company.FirstOrDefaultAsync(x => x.Id == inverter.CompanyId);

        var modeControlTopic = $"{inverter!.RegisteredInverter.Name}{modeControlTopicCommand!.MqttTopic}";

        var mqttLogFromRedisModeControl =await _redisCacheService.GetKeyValue<Models.MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverter.Id), Constants.MqttLogUnixOffset);
        var modeControlPayload = "";

        switch (actionTypeCommand)
        {
            case ActionTypeCommand.PassiveMode:
                modeControlPayload = ((int)SofarInverterModes.Passive).ToString();
                break;
            case ActionTypeCommand.InverterSelfUse:
                modeControlPayload = ((int)SofarInverterModes.SelfUse).ToString();
                break;
        }

        
        var lastLoqRecordModeControl = mqttLogFromRedisModeControl.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
            x.InverterId == inverter.Id
             && x.Topic == modeControlTopic
            && x.Direction == Direction.Out);

        bool shouldDispatchCommandModeControl = true;

        if (lastLoqRecordModeControl != null
            && lastLoqRecordModeControl.Topic == modeControlTopic
            && lastLoqRecordModeControl.Payload == modeControlPayload
            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordModeControl.CreatedAt!, 120))
        {
            shouldDispatchCommandModeControl = false;
        }

        if (sendAntiReflux)
        {

            DateTime currentDateTime = DateTime.Now; 
            var currentHourSpotPrice = await mySqldbContext.SpotPrice
             .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                     && x.DateTime.Month == currentDateTime.Month
                     && x.DateTime.Day == currentDateTime.Day
                     && x.DateTime.Hour == currentDateTime.Hour
                     && x.RegionId == company.RegionId);

            var antirefluxActionToSend = currentHourSpotPrice.PriceNoTax > company.BrokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

            var mqttTopicForAntiReflux = await mySqldbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                        x.InverterTypeId == inverter.InverterTypeId
                        && x.ActionType == antirefluxActionToSend);

            string antiRefluxTopic = $"{inverter!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
            string antiRefluxPayload = antirefluxActionToSend == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(inverter.MaxSalesPowerCapacity).ToString() : "0";

            await EnsureConnected();
            await new MqttClientService(mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                .Result.LogMessage(mySqldbContext, inverter.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);
        }
        await EnsureConnected();
        await new MqttClientService(mqttClient, _mqttLogger, shouldDispatchCommandModeControl).PublishMessages(modeControlTopic, modeControlPayload)
            .Result.LogMessage(mySqldbContext, inverter.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, modeControlTopic, modeControlPayload, actionTypeCommand, shouldDispatchCommandModeControl, MQttMessageType.ModeControl);


        
    }

}
