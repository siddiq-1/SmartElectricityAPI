using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;

namespace SmartElectricityAPI.Helpers;

public class BatteryCommandsProcessor
{
    private RedisCacheService redisCacheService;
    private readonly IMqttLogger _mqttLogger;
    private IMqttClient _mqttClient;
    private MySQLDBContext _dbContext;
    private List<InverterTypeCommands> InverterTypeCommands;
    private readonly DateTime dateTime;
    public BatteryCommandsProcessor(RedisCacheService redisCacheService, IMqttLogger mqttLogger, IMqttClient mqttClient, MySQLDBContext dbContext, List<InverterTypeCommands> inverterTypeCommands, DateTime dateTime)
    {
        this.redisCacheService = redisCacheService;
        _mqttLogger = mqttLogger;
        _mqttClient = mqttClient;
        _dbContext = dbContext;
        InverterTypeCommands = inverterTypeCommands;
        this.dateTime = dateTime;
    }
    public async Task ProcessBatteryCommands()
    {
        using (var mySQLDBContextLocal = await new DatabaseService().CreateDbContextAsync())
        {
            var entitiesList = mySQLDBContextLocal.ChangeTracker.Entries().ToList();

            foreach (var entity in entitiesList)
            {
                entity.Reload();
            }

            var listOfInverterBatteries = await mySQLDBContextLocal.InverterBattery.Include(x => x.Inverter).ToListAsync();
            var listOfInverters = await mySQLDBContextLocal.Inverter.Include(x => x.RegisteredInverter).ToListAsync();
            var inverterTypeCompanyActions = await mySQLDBContextLocal.InverterTypeCompanyActions.ToListAsync();
            var inverterTypes = await mySQLDBContextLocal.InverterType.ToListAsync();
            var companies = await mySQLDBContextLocal.Company.ToListAsync();
            var inverterTypeCommands = await mySQLDBContextLocal.InverterTypeCommands.ToListAsync();
            var distinctRegions = await mySQLDBContextLocal.Company.Select(x => x.RegionId).Distinct().ToListAsync();
            var usedRegions = await mySQLDBContextLocal.Region.Where(x => distinctRegions.Contains(x.Id)).ToListAsync();
            var registeredInverters = await  mySQLDBContextLocal.RegisteredInverter.ToListAsync();

            foreach (var region in usedRegions)
            {
                DateTime signalTime = dateTime.AddHours(-region.OffsetHoursFromEstonianTime);

                var joinedData = await mySQLDBContextLocal.SpotPrice
                .Where(x => x.DateTime.Year == signalTime.Year
                 && x.DateTime.Month == signalTime.Month
                 && x.DateTime.Day == signalTime.Day
                 && x.DateTime.Hour == signalTime.Hour
                 && x.RegionId == region.Id
                 )
        .Join(
            mySQLDBContextLocal.BatteryControlHours,
            spotPrice => spotPrice.Id,
            batteryHours => batteryHours.SpotPriceMaxId,
            (spotPrice, batteryHours) => new
            {
                SpotPrice = spotPrice,
                BatteryHours = batteryHours
            }).Where(x => x.BatteryHours.IsProcessed == false)
        .ToListAsync();

                foreach (var item in joinedData)
                {
                    var listOfActions = await mySQLDBContextLocal.InverterTypeActions.Where(
                        x => x.ActionTypeCommand == item.BatteryHours.ActionTypeCommand)
                        .Join(mySQLDBContextLocal.InverterTypeCommands,
                        InverterTypeActions => new { InverterTypeActions.ActionType, InverterTypeActions.InverterTypeId },
                        InverterTypeCommands => new { InverterTypeCommands.ActionType, InverterTypeCommands.InverterTypeId },
                        (InverterTypeActions, InverterTypeCommands) => new
                        {
                            InverterTypeActions,
                            InverterTypeCommands
                        }).ToListAsync();


                    foreach (var singleAction in listOfActions)
                    {
                        bool skipSendingMQttCommand = false;

                        var inverterBattery = listOfInverterBatteries.Where(x => x.Id == item.BatteryHours.InverterBatteryId).FirstOrDefault();

                        var inverterIdWithName = listOfInverters.Where(x => x.Id == inverterBattery.InverterId && x.RegisteredInverterId != null).FirstOrDefault();

                        var selectedRegisteredInverter = registeredInverters.FirstOrDefault(x => x.Id == inverterIdWithName!.RegisteredInverterId);

                        var inverterTypeCurrentAction = inverterTypeCompanyActions.FirstOrDefault(x =>
                        x.InverterId == inverterIdWithName!.Id
                        && x.CompanyId == inverterIdWithName.CompanyId
                        && x.InverterTypeId == inverterIdWithName.InverterTypeId
                        && x.ActionTypeCommand == ActionTypeCommand.AutoMode);


                        var inverterTypeCurrentActionActive = inverterTypeCompanyActions.FirstOrDefault(x =>
                        x.InverterId == inverterIdWithName!.Id
                        && x.CompanyId == inverterIdWithName.CompanyId
                        && x.InverterTypeId == inverterIdWithName.InverterTypeId
                        && x.ActionTypeCommand != ActionTypeCommand.AutoMode
                        && x.ActionState == true);


                        if (inverterBattery != null &&!inverterBattery.Enabled)
                        {
                            continue;
                        }

                        ActionTypeCommand actionTypeCommandToUse = singleAction.InverterTypeActions.ActionTypeCommand;

                        var last5StateTransactions = await redisCacheService.GetKeyValue<SofarState>(selectedRegisteredInverter.Id.ToString(), Constants.SofarStateUnixOffset);
                        last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(2).ToList();

                        var averageBatteryProduction = 0;
                        var averageInverterConsumption = 0;
                        if (last5StateTransactions != null && last5StateTransactions.Count > 0)
                        {
                            averageBatteryProduction = last5StateTransactions.Sum(x => x.solarPV) / last5StateTransactions.Count;
                            averageInverterConsumption = last5StateTransactions.Sum(x => x.consumption) / last5StateTransactions.Count;
                        }
       

                        if (inverterTypeCurrentActionActive != null
                            && inverterTypeCurrentActionActive.ActionTypeCommand == ActionTypeCommand.HzMarket
                            || selectedRegisteredInverter != null && await HasHzMarketActionForThisHour(selectedRegisteredInverter))
                        {
                            continue;
                        }
                        if (actionTypeCommandToUse == ActionTypeCommand.ChargeWithRemainingSun
                            || actionTypeCommandToUse == ActionTypeCommand.ChargeMax)
                        {                         

                            if (last5StateTransactions.Count > 0 && last5StateTransactions.Average(x => x.batterySOC) >= inverterBattery.MaxLevel)
                            {

                                if (averageBatteryProduction > averageInverterConsumption && actionTypeCommandToUse == ActionTypeCommand.ChargeWithRemainingSun)
                                {
                                    actionTypeCommandToUse = ActionTypeCommand.SellRemainingSunNoCharging;
                                }
                                else
                                {
                                    actionTypeCommandToUse = ActionTypeCommand.ChargeWithRemainingSun;
                                }

                            }
                        }

                        if (inverterIdWithName != null && inverterTypeCurrentAction != null && inverterTypeCurrentAction.ActionState)
                        {
                            string topic = $"{inverterIdWithName!.RegisteredInverter.Name}{singleAction.InverterTypeCommands.MqttTopic}";
                            string payload = "0";
                            ActionType returnedActionTypeToUse = ActionType.None;

                            if (actionTypeCommandToUse == ActionTypeCommand.SelfUse)
                            {
                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);

                                if (inverterIdWithName.UseInverterSelfUse)
                                {
                                    if (last5StateTransactions.Count == 0
                                        ||
                                        (last5StateTransactions.Count > 0
                                        && last5StateTransactions.Average(x => x.batterySOC) > inverterBattery!.MinLevel
                                        || averageBatteryProduction > averageInverterConsumption))
                                    {
                                        await batteryButtonActions.ProcessInverterModeControl(inverterIdWithName, mySQLDBContextLocal, ActionTypeCommand.PassiveMode);
                                    }
                                    
                                }

                                var inverterTypeCurrentActionButton = inverterTypeCompanyActions.FirstOrDefault(x =>
                                    x.InverterId == inverterIdWithName!.Id
                                    && x.CompanyId == inverterIdWithName.CompanyId
                                    && x.InverterTypeId == inverterIdWithName.InverterTypeId
                                    && x.ActionTypeCommand != ActionTypeCommand.AutoMode
                                    && x.ActionState);


                                if (inverterTypeCurrentActionButton != null && inverterTypeCurrentActionButton.ActionTypeCommand != item.BatteryHours.ActionTypeCommand)
                                {
                                    payload = await batteryButtonActions.ProcessSelfUse(inverterIdWithName, inverterBattery, joinedData[0].SpotPrice, mySQLDBContextLocal, true);

                                    if (payload == "0")
                                    {
                                        topic = $"{inverterIdWithName!.RegisteredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == inverterIdWithName.InverterTypeId).MqttTopic}";
                                    }

          
                                    mySQLDBContextLocal.SaveChanges();
                                }
                                else
                                {
                                    skipSendingMQttCommand = true;
                                }
                            }

                            if (actionTypeCommandToUse == ActionTypeCommand.SellRemainingSunNoCharging)
                            {
                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);

                                if (inverterIdWithName.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(inverterIdWithName, mySQLDBContextLocal, ActionTypeCommand.PassiveMode);
                                }

                                var previousHourDateTime = dateTime.AddHours(-1);

                                (payload, returnedActionTypeToUse) = await batteryButtonActions.ProcessSellRemainingSunNoCharging(previousHourDateTime, item.SpotPrice, joinedData[0].SpotPrice, inverterIdWithName, inverterBattery, companies.FirstOrDefault(x => x.Id == inverterBattery.Inverter.CompanyId), mySQLDBContextLocal, false, true, ActionTypeCommand.SellRemainingSunNoCharging);



                                if (last5StateTransactions.Count > 0 && last5StateTransactions.Average(x => x.batterySOC) >= 50 && payload == "0")
                                {
                                    topic = $"{inverterIdWithName!.RegisteredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == inverterIdWithName.InverterTypeId).MqttTopic}";
                                }
                                else
                                {
                                    topic = $"{inverterIdWithName!.RegisteredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == inverterIdWithName.InverterTypeId).MqttTopic}";
                                }


                                 await mySQLDBContextLocal.SaveChangesAsync();
                            }

                            if (actionTypeCommandToUse == ActionTypeCommand.ConsumeBatteryWithMaxPower)
                            {
                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);

                                if (inverterIdWithName.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(inverterIdWithName, mySQLDBContextLocal, ActionTypeCommand.PassiveMode);
                                }

                                //Send antireflux command
                                var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                x.InverterTypeId == inverterIdWithName.InverterTypeId
                                && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                                string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                                string antiRefluxPayLoad = InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterIdWithName.MaxSalesPowerCapacity).ToString();

                                var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<Models.MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                                var lastLoqRecordAntiReflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                        x.InverterId == inverterIdWithName.Id
                                                        && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                                bool shouldDispatchCommandAntiReflux = true;

                                if (lastLoqRecordAntiReflux != null
                                    && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                                    && lastLoqRecordAntiReflux.Payload == antiRefluxPayLoad
                                    && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120))
                                {
                                    shouldDispatchCommandAntiReflux = true;
                                }
                                await new MqttClientService(_mqttClient, _mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayLoad)
                               .Result.LogMessage(_dbContext, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayLoad, inverterTypeCurrentAction.ActionTypeCommand, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);
                                payload = ((Convert.ToInt32(inverterBattery!.Inverter.MaxPower) * 1000) / inverterBattery!.Inverter.NumberOfInverters).ToString();
                            }

                            if (actionTypeCommandToUse == ActionTypeCommand.ChargeWithRemainingSun)
                            {
                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);

                                if (inverterIdWithName.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(inverterIdWithName, mySQLDBContextLocal, ActionTypeCommand.PassiveMode);
                                }

                                var inverterTypeCurrentActionButton = inverterTypeCompanyActions.FirstOrDefault(x =>
                                                                        x.InverterId == inverterIdWithName!.Id
                                                                        && x.CompanyId == inverterIdWithName.CompanyId
                                                                        && x.InverterTypeId == inverterIdWithName.InverterTypeId
                                                                        && x.ActionTypeCommand != ActionTypeCommand.AutoMode
                                                                        && x.ActionState);

                                if (inverterTypeCurrentActionButton != null && inverterTypeCurrentActionButton.ActionTypeCommand != item.BatteryHours.ActionTypeCommand)
                                {

                                    var mqttTopicForAntiRefluxRemSun = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                    x.InverterTypeId == inverterIdWithName.InverterTypeId
                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                                    string antiRefluxTopicRemSun = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiRefluxRemSun!.MqttTopic}";

                                    string antiRefluxPayLoadRemSun = "0";

                                    var selectedCompany = companies.FirstOrDefault(x => x.Id == inverterIdWithName.CompanyId);

                                    if (item.SpotPrice.PriceNoTax - selectedCompany.BrokerPurchaseMargin > 0)
                                    {
                                        antiRefluxPayLoadRemSun = InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterIdWithName.MaxSalesPowerCapacity).ToString();
                                    }                                

                                    await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopicRemSun, antiRefluxPayLoadRemSun)
                                                .Result.LogMessage(_dbContext, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopicRemSun, antiRefluxPayLoadRemSun, inverterTypeCurrentAction.ActionTypeCommand, true, MQttMessageType.ThreephaseAntireflux);

                                    topic = $"{inverterIdWithName!.RegisteredInverter.Name}{InverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == inverterIdWithName.InverterTypeId).MqttTopic}";

                                    payload = "0";
                                }
                                else
                                {
                                    skipSendingMQttCommand = true;
                                }
                            }


                            if (actionTypeCommandToUse == ActionTypeCommand.ChargeMax)
                            {
                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);

                                if (inverterIdWithName.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(inverterIdWithName, mySQLDBContextLocal, ActionTypeCommand.PassiveMode);
                                }

                                payload = (inverterIdWithName.InverterBattery.FirstOrDefault()!.ChargingPowerFromGridKWh * 1000).ToString();
                            }

                            if (actionTypeCommandToUse == ActionTypeCommand.InverterSelfUse
                                && inverterIdWithName.UseInverterSelfUse)
                            {

                                DateTime dateTimePreviousHour = DateTime.Now.AddHours(-1);

                                var previousHourSpotPrice = await mySQLDBContextLocal.SpotPrice
                                                    .Where(x => x.DateTime.Year == dateTimePreviousHour.Year
                                                     && x.DateTime.Month == dateTimePreviousHour.Month
                                                     && x.DateTime.Day == dateTimePreviousHour.Day
                                                     && x.DateTime.Hour == dateTimePreviousHour.Hour
                                                     && x.RegionId == region.Id)
                                                    .Join(mySQLDBContextLocal.BatteryControlHours
                                                            .Where(bh => bh.InverterBatteryId == item.BatteryHours.InverterBatteryId), // Add filter condition here
                                                        spotPrice => spotPrice.Id,
                                                        batteryHours => batteryHours.SpotPriceMaxId,
                                                        (spotPrice, batteryHours) => new
                                                        {
                                                            SpotPrice = spotPrice,
                                                            BatteryHours = batteryHours
                                                        }).FirstOrDefaultAsync();

                                if (previousHourSpotPrice != null && previousHourSpotPrice.BatteryHours.ActionTypeCommand == item.BatteryHours.ActionTypeCommand)
                                {
                                    var mqttLogFromRedisModeControl = await redisCacheService.GetKeyValue<Models.MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                                    var lastLoqRecordModeControl = mqttLogFromRedisModeControl.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                        x.InverterId == inverterIdWithName.Id
                                                                        && x.MQttMessageType == MQttMessageType.ModeControl
                                                                        && x.Direction == Direction.Out);

                                    if (lastLoqRecordModeControl != null
                                        && lastLoqRecordModeControl.Payload == payload)
                                    {
                                        skipSendingMQttCommand = true;
                                    }
                                }

                            }


                            var mqttLogFromRedis = await redisCacheService.GetKeyValue<Models.MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                            var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                                            x.InverterId == inverterIdWithName.Id
                                                                                            && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                                                            && x.MQttMessageType == MQttMessageType.Regular
                                                                                            && x.Direction == Direction.Out);


                            bool shouldDispatchCommand = true;

                            if (lastLoqRecord != null
                                && lastLoqRecord.Topic == topic
                                && lastLoqRecord.Payload == payload
                                && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                                && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                            {
                                shouldDispatchCommand = true;
                            }

                            if (skipSendingMQttCommand)
                            {
                                shouldDispatchCommand = false;
                            }

                            if (actionTypeCommandToUse == ActionTypeCommand.InverterSelfUse)
                            {
                                if (last5StateTransactions.Count == 0
                                  ||
                                  (last5StateTransactions.Count > 0
                                  && last5StateTransactions.Average(x => x.batterySOC) > inverterBattery!.MinLevel
                                  || averageBatteryProduction > averageInverterConsumption))
                                {
                                    var localCompany = companies.FirstOrDefault(x => x.Id == inverterIdWithName.CompanyId);
                                    var antirefluxActionToSend = joinedData[0].SpotPrice.PriceNoTax > localCompany.BrokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                                    var mqttTopicForAntiReflux = await mySQLDBContextLocal.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                        x.InverterTypeId == inverterIdWithName.InverterTypeId
                                        && x.ActionType == antirefluxActionToSend);

                                    string antiRefluxTopic = $"{inverterIdWithName!.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                                    string antiRefluxPayload = antirefluxActionToSend == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterBattery.Inverter.MaxSalesPowerCapacity).ToString() : "0";
                                    await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                                        .Result.LogMessage(_dbContext, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);
                                }
                                else
                                {
                                    payload = "0";
                                    topic = $"{inverterIdWithName!.RegisteredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == inverterIdWithName.InverterTypeId).MqttTopic}";
                                }
                                    //Samal loogika Kui aku on üle miinimumi või päike ületab maja tarbimise siis läheb allolev blokk töösse
                                    //Muul juhul passive mode ja charge 0. Peaks kontrollima kas on vaja passive reziimi viia, kui eelmine oli juba inverterselfuse siis on vaja

                            }


                            await new MqttClientService(_mqttClient, _mqttLogger, shouldDispatchCommand).PublishMessages(topic, payload)
                             .Result.LogMessage(_dbContext, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, topic, payload, inverterTypeCurrentAction.ActionTypeCommand, shouldDispatchCommand, MQttMessageType.Regular);

                            using (var mySQLDBContexForButtons = await new DatabaseService().CreateDbContextAsync())
                            {

                                var companyInverterActions = await mySQLDBContexForButtons.InverterTypeCompanyActions.Where(x =>
                                x.CompanyId == inverterIdWithName.CompanyId
                                && x.InverterId == inverterIdWithName!.Id
                                 && x.InverterTypeId == inverterIdWithName.InverterTypeId).ToListAsync();

                                foreach (var action in companyInverterActions)
                                {
                                    if (action.ActionTypeCommand == actionTypeCommandToUse)
                                    {
                                        action.ActionState = true;
                                    }
                                    else
                                    {
                                        if (action.ActionTypeCommand != ActionTypeCommand.AutoMode)
                                        {
                                            action.ActionState = false;
                                        }
                                    }
                                }



                                await mySQLDBContexForButtons.SaveChangesAsync();
                            }

                            var selectedBatterControlHours = await mySQLDBContextLocal.BatteryControlHours.FirstOrDefaultAsync(x => x.Id == item.BatteryHours.Id);

                            if (selectedBatterControlHours != null)
                            {
                                selectedBatterControlHours.IsProcessed = true;
                            }


                            //TODO: Update battery control hours

                            await mySQLDBContextLocal.SaveChangesAsync();
                        }
                        else if (inverterIdWithName != null && inverterTypeCurrentAction != null && !inverterTypeCurrentAction.ActionState)
                        {
                            string topic = $"{inverterIdWithName!.RegisteredInverter.Name}{singleAction.InverterTypeCommands.MqttTopic}";
                            string payload = "0";

                            if (actionTypeCommandToUse == ActionTypeCommand.SelfUse)
                            {
                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, redisCacheService);

                                if (inverterIdWithName.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(inverterIdWithName, mySQLDBContextLocal, ActionTypeCommand.PassiveMode);
                                }

                                DateTime dateTimePreviousHour = DateTime.Now.AddHours(-1);

                                var previousHourSpotPrice = await mySQLDBContextLocal.SpotPrice
                                                    .Where(x => x.DateTime.Year == dateTimePreviousHour.Year
                                                     && x.DateTime.Month == dateTimePreviousHour.Month
                                                     && x.DateTime.Day == dateTimePreviousHour.Day
                                                     && x.DateTime.Hour == dateTimePreviousHour.Hour
                                                     && x.RegionId == region.Id)
                                                    .Join(mySQLDBContextLocal.BatteryControlHours
                                                            .Where(bh => bh.InverterBatteryId == item.BatteryHours.InverterBatteryId), // Add filter condition here
                                                        spotPrice => spotPrice.Id,
                                                        batteryHours => batteryHours.SpotPriceMaxId,
                                                        (spotPrice, batteryHours) => new
                                                        {
                                                            SpotPrice = spotPrice,
                                                            BatteryHours = batteryHours
                                                        }).FirstOrDefaultAsync();

                                if (previousHourSpotPrice != null && previousHourSpotPrice.BatteryHours.ActionTypeCommand != item.BatteryHours.ActionTypeCommand)
                                {
                                    payload = await batteryButtonActions.ProcessSelfUse(inverterIdWithName, inverterBattery, joinedData[0].SpotPrice, mySQLDBContextLocal, true);

                                      await mySQLDBContextLocal.SaveChangesAsync();
                                }
                                else
                                {
                                    skipSendingMQttCommand = true;
                                }
                            }

                            var mqttLogFromRedis = await redisCacheService.GetKeyValue<Models.MqttMessageLog>(Constants.CacheKeys.MqttLogKey(inverterIdWithName.Id), Constants.MqttLogUnixOffset);

                            var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                        x.InverterId == inverterIdWithName.Id
                                                                        && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                                        && x.MQttMessageType == MQttMessageType.Regular
                                                                        && x.Direction == Direction.Out);

                            bool shouldDispatchCommand = true;

                            if (lastLoqRecord != null
                                && lastLoqRecord.Topic == topic
                                && lastLoqRecord.Payload == payload
                                && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                                && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                            {
                                shouldDispatchCommand = true;
                            }

                            if (skipSendingMQttCommand)
                            {
                                shouldDispatchCommand = false;
                            }

                            if (actionTypeCommandToUse != ActionTypeCommand.InverterSelfUse)
                            {

                                await new MqttClientService(_mqttClient, _mqttLogger, shouldDispatchCommand).PublishMessages(topic, payload)
                                    .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.Id, Direction.Out, MqttMessageOrigin.AutoServiceBattery, topic, payload, inverterTypeCurrentAction.ActionTypeCommand, shouldDispatchCommand, MQttMessageType.Regular);
                            }

                            var selectedBatterControlHours = await mySQLDBContextLocal.BatteryControlHours.FirstOrDefaultAsync(x => x.Id == item.BatteryHours.Id);

                            if (selectedBatterControlHours != null)
                            {
                                selectedBatterControlHours.IsProcessed = true;
                            }
                            mySQLDBContextLocal.SaveChanges();
                            //TODO: Update battery control hours
                        }
                    }
                }
            }
        }
    }

    private async Task<bool> HasHzMarketActionForThisHour(RegisteredInverter registeredInverter)
    {
        DateTime currentDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        var DateTimeOffset = new DateTimeOffset(currentDateTime);
        long currentTime = DateTimeOffset.ToUnixTimeSeconds();
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            return await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader)
                .Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
                && x.FuseBoxMessageHeader.device_id == registeredInverter.Name
                && x.actualStart == null && x.start == currentTime).AnyAsync();
        }
    }
}
