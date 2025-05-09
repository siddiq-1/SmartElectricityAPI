using Newtonsoft.Json;
using SmartElectricityAPI.Models.FuseboxV2;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Database;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.Interfaces;
using MQTTnet.Client;
using SmartElectricityAPI.Processors;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Models;
using MQTTnet.Server;
using SmartElectricityAPI.Models.Fusebox;
using Polly;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class FuseBoxMessageProcessor
{
    FuseBoxMessageHeaderV2 fuseBoxMessageHeaderV2;
    FuseBoxChannels fuseBoxChannels;

    private IMqttClient _mqttClient;
    private IMqttLogger _mqttLogger;
    private RedisCacheService _redisCacheService;
    private List<Inverter> _invertersWithRegisteredInverter;
    private Inverter _selectedInvertersWithRegisteredInverter;
    private readonly string uploadPath = Path.Combine("/app/logs/fusebox");

    public async Task Process(string _message, string _topic, IMqttClient mqttClient, IMqttLogger mqttLogger, RedisCacheService redisCacheService, List<Inverter> invertersWithRegisteredInverter)
    {
     
        _mqttClient = mqttClient;
        _mqttLogger = mqttLogger;
        _redisCacheService = redisCacheService;
        _invertersWithRegisteredInverter = invertersWithRegisteredInverter;

        fuseBoxChannels = (FuseBoxChannels)Enum.Parse(typeof(FuseBoxChannels), _topic);

        SaveMessageToFile(_message);

        Deserialize(_message);

        _selectedInvertersWithRegisteredInverter = _invertersWithRegisteredInverter.FirstOrDefault(x => x.RegisteredInverter.Name == fuseBoxMessageHeaderV2.meta.device_id);

        await PerformMessageAction();       
    }
    private void SaveMessageToFile(string message)
    {

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        string logFile = Path.Combine(uploadPath, $"log_fusebox_main_app_{DateTime.UtcNow:yyyy-MM-dd}.txt");
        string logMessage = $"{fuseBoxChannels}, {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}, {message}\n";

        File.AppendAllText(logFile, logMessage);
    }

    private void Deserialize(string message)
    {
        try
        {
            fuseBoxMessageHeaderV2 = JsonConvert.DeserializeObject<FuseBoxMessageHeaderV2>(message);
          
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"Error deserializing message: {ex.Message}");
        }
    }

    private async Task PerformMessageAction()
    {
        switch (fuseBoxChannels)
        {
            case FuseBoxChannels.FUSEBOX_SCHED_REG_RFP:
                await ActionForRFP();
                break;
                
            case FuseBoxChannels.FUSEBOX_SCHED_REG_IP:

                if (await IsFuseBoxTransactionOpen())
                {
                    await ActionForIP();
                }              
                break;

            default:
                break;
        }
    }

    private async Task<bool> IsFuseBoxTransactionOpen()
    {
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var fuseboxSchedRegMsg = await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader)
            .Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
                && x.FuseBoxMessageHeader.m_id == fuseBoxMessageHeaderV2.meta.m_id
                && x.FuseBoxMessageHeader.device_id == fuseBoxMessageHeaderV2.meta.device_id
                && x.actualStart != null
                && x.actualEnd == null)
            .FirstOrDefaultAsync();

            return fuseboxSchedRegMsg != null;
        }     
    }

    private async Task ActionForRFP()
    {
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            if (fuseBoxMessageHeaderV2.body.start == null && fuseBoxMessageHeaderV2.body.cancel == true)
            {
                await PerformButtonChange(true);

                var ActionTypeCommandForCurrentHour = await FindCurrentHourActionTypeCommand();

                var InverterTypeCompanyAction = await dbContext.InverterTypeCompanyActions.Where(x => x.CompanyId == _selectedInvertersWithRegisteredInverter.CompanyId && x.ActionTypeCommand == ActionTypeCommandForCurrentHour).FirstOrDefaultAsync();

                var company = await dbContext.Company.FirstOrDefaultAsync(x => x.Id == _selectedInvertersWithRegisteredInverter.CompanyId);
                var inverterBatteryButtonProcessor = new InverterBatteryButtonProcessor(company, InverterTypeCompanyAction.Id, _redisCacheService, _mqttLogger);
                await inverterBatteryButtonProcessor.Process();

                await LogEndTimeForCancel();
            }

            if (fuseBoxMessageHeaderV2.body.start != null && fuseBoxMessageHeaderV2.body.cancel == false)
            {
                var currentDateTime = DateTime.Now;
                var inverterBattery = await dbContext.InverterBattery.FirstOrDefaultAsync(x => x.InverterId == _selectedInvertersWithRegisteredInverter.Id);
                var company = await dbContext.Company.FirstOrDefaultAsync(x => x.Id == _selectedInvertersWithRegisteredInverter.CompanyId);

                var currentHourPrice = await dbContext.SpotPrice
                    .Where(x => x.DateTime.Year == currentDateTime.Year
                        && x.DateTime.Month == currentDateTime.Month
                        && x.DateTime.Day == currentDateTime.Day
                        && x.DateTime.Hour == currentDateTime.Hour
                        && x.RegionId == company.RegionId)
                    .Join(dbContext.InverterCompanyHours
                        .Where(ich => ich.CompanyId == _selectedInvertersWithRegisteredInverter.CompanyId
                            && ich.InverterId == _selectedInvertersWithRegisteredInverter.Id),
                        sp => sp.Id,
                        ich => ich.SpotPriceId,
                        (sp, ich) => new {
                            SpotPrice = sp,
                            InverterCompanyHour = ich

                        })
                    .FirstOrDefaultAsync();

                var last5StateTransactions = await _redisCacheService.GetKeyValue<SofarState>(_selectedInvertersWithRegisteredInverter.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
                last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(5).ToList();


                if (await ValidateBatteryAsync(inverterBattery, dbContext, company, last5StateTransactions))
                {
                    await PerformButtonChange();

                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(_mqttLogger, _redisCacheService);

                    if (_selectedInvertersWithRegisteredInverter.UseInverterSelfUse)
                    {
                        await batteryButtonActions.ProcessInverterModeControl(_selectedInvertersWithRegisteredInverter, dbContext, ActionTypeCommand.PassiveMode);
                    }

                    ActionType actionType = fuseBoxMessageHeaderV2.body.pow_set.BipolarSetpoint >= 0 ? ActionType.Charge : ActionType.Discharge;

                    string payload = "0";

                    if (actionType == ActionType.Charge)
                    {
                        payload = ((Math.Abs(fuseBoxMessageHeaderV2.body.pow_set.BipolarSetpoint) + 0) / _selectedInvertersWithRegisteredInverter.NumberOfInverters).ToString();
                    }
                    else
                    {
                        payload = ((Math.Abs(fuseBoxMessageHeaderV2.body.pow_set.BipolarSetpoint) - 0) / _selectedInvertersWithRegisteredInverter.NumberOfInverters).ToString();
                    }

                    await SendMQttToInverter(actionType, payload);
                    await LogEndTimeForOtherOpenTasks();

                    await LogStartTime();
                }
            }
        }
    }

    private async Task<bool> IsValidForDischarge(InverterBattery inverterBattery, MySQLDBContext dbContext, Company company, List<SofarState> last5StateTransactions)
    {
        var currentDateTime = DateTime.Now;

        var currentHourPrice = await dbContext.SpotPrice
                .Where(x => x.DateTime.Year == currentDateTime.Year
                    && x.DateTime.Month == currentDateTime.Month
                    && x.DateTime.Day == currentDateTime.Day
                    && x.DateTime.Hour == currentDateTime.Hour
                    && x.RegionId == company.RegionId)
                .Join(dbContext.InverterCompanyHours
                    .Where(ich => ich.CompanyId == _selectedInvertersWithRegisteredInverter.CompanyId
                        && ich.InverterId == _selectedInvertersWithRegisteredInverter.Id),
                    sp => sp.Id,
                    ich => ich.SpotPriceId,
                    (sp, ich) => new {
                        SpotPrice = sp,
                        InverterCompanyHour = ich

                    })
                .FirstOrDefaultAsync();

        if (fuseBoxMessageHeaderV2.body.pow_set.BipolarSetpoint < 0
        && currentHourPrice != null
                    && inverterBattery != null
                    && inverterBattery.HzMarketDischargeMinPrice != null
                    && inverterBattery.HzMarketMinBatteryLevelOnDischargeCommand != null
                    && currentHourPrice.InverterCompanyHour.CostWithPurchaseMargin >= inverterBattery.HzMarketDischargeMinPrice / 100
                    && last5StateTransactions.FirstOrDefault()!.batterySOC >= inverterBattery.MinLevel
                    && last5StateTransactions.FirstOrDefault()!.batterySOC >= inverterBattery.HzMarketMinBatteryLevelOnDischargeCommand)
        {
            return true;
        }

        return false;

    }

    public async Task<bool> ValidateBatteryAsync(InverterBattery inverterBattery,
                                             MySQLDBContext dbContext,
                                             Company company,
                                             List<SofarState> last5StateTransactions)
    {
        if (await IsValidForDischarge(inverterBattery, dbContext, company, last5StateTransactions)
            || await IsValidForCharge(inverterBattery, last5StateTransactions)
            || fuseBoxMessageHeaderV2.body.pow_set.BipolarSetpoint == 0)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> IsValidForCharge(InverterBattery inverterBattery, List<SofarState> last5StateTransactions)
    {
        if (fuseBoxMessageHeaderV2.body.pow_set.BipolarSetpoint > 0
         && inverterBattery != null
         && last5StateTransactions.FirstOrDefault()!.batterySOC < inverterBattery.MaxLevel)
        {
            return true;
        }

        return false;
    }

    private async Task SendMQttToInverter(ActionType actionType, string payload)
    {
        using (var mySQLDBContext = await new DatabaseService().CreateDbContextAsync())
        {
          
            var registeredInverter = await mySQLDBContext.RegisteredInverter.FirstOrDefaultAsync(x => x.Id == _selectedInvertersWithRegisteredInverter.RegisteredInverterId);
            var inverterTypeCommands = await mySQLDBContext.InverterTypeCommands.Where(x => x.InverterTypeId == _selectedInvertersWithRegisteredInverter.InverterTypeId).ToListAsync();
            InverterTypeCommands mqttTopicForAntiReflux = new ();
            string antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(_selectedInvertersWithRegisteredInverter.MaxSalesPowerCapacity).ToString();

            if (actionType == ActionType.Charge)
            {
                mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == _selectedInvertersWithRegisteredInverter!.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);
            }
            if (actionType == ActionType.Discharge)
            {
                mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == _selectedInvertersWithRegisteredInverter!.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);
            }

            string antiRefluxTopic = $"{registeredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";

            await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                .Result.LogMessage(mySQLDBContext, _selectedInvertersWithRegisteredInverter.Id, Direction.Out, MqttMessageOrigin.HzMarket, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.HzMarket, true, MQttMessageType.ThreephaseAntireflux);

            var mqttTopic = $"{registeredInverter.Name}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == actionType && x.InverterTypeId == _selectedInvertersWithRegisteredInverter!.InverterTypeId).MqttTopic}";

            var payLoad = payload;

            if (!string.IsNullOrWhiteSpace(mqttTopic) && !string.IsNullOrWhiteSpace(payLoad) && _selectedInvertersWithRegisteredInverter != null)
            {
                await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(mqttTopic, payLoad).Result
                    .LogMessage(mySQLDBContext, _selectedInvertersWithRegisteredInverter.Id, Direction.Out, MqttMessageOrigin.HzMarket, mqttTopic, payLoad, ActionTypeCommand.HzMarket, true, MQttMessageType.Regular);
            }
        }
    }
    private async Task ActionForIP()
    {
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            if (!await FindActionWithStartTimeAndNotStarted())
            {
                var ActionTypeCommandForCurrentHour = await FindCurrentHourActionTypeCommand();

                var InverterTypeCompanyAction = await dbContext.InverterTypeCompanyActions.Where(x => x.CompanyId == _selectedInvertersWithRegisteredInverter.CompanyId && x.ActionTypeCommand == ActionTypeCommandForCurrentHour).FirstOrDefaultAsync();

                var company = await dbContext.Company.FirstOrDefaultAsync(x => x.Id == _selectedInvertersWithRegisteredInverter.CompanyId);
                var inverterBatteryButtonProcessor = new InverterBatteryButtonProcessor(company, InverterTypeCompanyAction.Id, _redisCacheService, _mqttLogger);
                await inverterBatteryButtonProcessor.Process();
            }

            await LogEndTime();
        }
    }

    private async Task<bool> FindActionWithStartTimeAndNotStarted()
    {
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            return await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader)
                .Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
                && x.FuseBoxMessageHeader.device_id == fuseBoxMessageHeaderV2.meta.device_id
                && x.actualStart == null && x.start == fuseBoxMessageHeaderV2.body.end).AnyAsync();
        }
    }

    private async Task PerformButtonChange(bool _forceCurrentHourButton = false)
    {
        if (_forceCurrentHourButton)
        {
            using (var mySQLDBContexForButtons = await new DatabaseService().CreateDbContextAsync())
            {
                DateTime currentDateTime = DateTime.Now;

                var inverterBattery = await mySQLDBContexForButtons.InverterBattery.FirstOrDefaultAsync(x => x.InverterId == _selectedInvertersWithRegisteredInverter.Id);
                var company = await mySQLDBContexForButtons.Company.FirstOrDefaultAsync(x => x.Id == _selectedInvertersWithRegisteredInverter.CompanyId);

                var currentDateTimeSpotPrice = await mySQLDBContexForButtons.SpotPrice
                    .Where(x => x.DateTime.Year == currentDateTime.Year
                     && x.DateTime.Month == currentDateTime.Month
                     && x.DateTime.Day == currentDateTime.Day
                     && x.DateTime.Hour == currentDateTime.Hour
                     && x.RegionId == company.RegionId)
                    .Join(mySQLDBContexForButtons.BatteryControlHours
                            .Where(bh => bh.InverterBatteryId == inverterBattery.Id), 
                        spotPrice => spotPrice.Id,
                        batteryHours => batteryHours.SpotPriceMaxId,
                        (spotPrice, batteryHours) => new
                        {
                            SpotPrice = spotPrice,
                            BatteryHours = batteryHours
                        }).FirstOrDefaultAsync();

                await ChangeButtonToCommand(currentDateTimeSpotPrice.BatteryHours.ActionTypeCommand);
            }
        }
        else
        {
            if (fuseBoxChannels == FuseBoxChannels.FUSEBOX_SCHED_REG_RFP || IsFuseBoxTransactionOpen().Result)
            {
                await ChangeButtonToCommand(ActionTypeCommand.HzMarket);
            }
        }
    }

    private async Task<ActionTypeCommand> FindCurrentHourActionTypeCommand()
    {
        using (var mySQLDBContexForButtons = await new DatabaseService().CreateDbContextAsync())
        {
            DateTime currentDateTime = DateTime.Now;

            var inverterBattery =  await mySQLDBContexForButtons.InverterBattery.FirstOrDefaultAsync(x => x.InverterId == _selectedInvertersWithRegisteredInverter.Id);
            var company = await mySQLDBContexForButtons.Company.FirstOrDefaultAsync(x => x.Id == _selectedInvertersWithRegisteredInverter.CompanyId);

            var currentDateTimeSpotPrice = await mySQLDBContexForButtons.SpotPrice
                .Where(x => x.DateTime.Year == currentDateTime.Year
                 && x.DateTime.Month == currentDateTime.Month
                 && x.DateTime.Day == currentDateTime.Day
                 && x.DateTime.Hour == currentDateTime.Hour
                 && x.RegionId == company.RegionId)
                .Join(mySQLDBContexForButtons.BatteryControlHours
                        .Where(bh => bh.InverterBatteryId == inverterBattery.Id),
                    spotPrice => spotPrice.Id,
                    batteryHours => batteryHours.SpotPriceMaxId,
                    (spotPrice, batteryHours) => new
                    {
                        SpotPrice = spotPrice,
                        BatteryHours = batteryHours
                    }).FirstOrDefaultAsync();

            return currentDateTimeSpotPrice.BatteryHours.ActionTypeCommand;
        }
    }

    private async Task ChangeButtonToCommand(ActionTypeCommand actionTypeCommand)
    {
        using (var mySQLDBContexForButtons = await new DatabaseService().CreateDbContextAsync())
        {

            var companyInverterActions = await mySQLDBContexForButtons.InverterTypeCompanyActions.Where(x =>
            x.CompanyId == _selectedInvertersWithRegisteredInverter.CompanyId).ToListAsync();

            foreach (var action in companyInverterActions)
            {
                if (action.ActionTypeCommand == actionTypeCommand)
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

            mySQLDBContexForButtons.SaveChanges();
        }
    }

    private async Task LogEndTime()
    {
      
        long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var fuseboxSchedRegMsg = await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader).
                Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
                && x.FuseBoxMessageHeader.device_id == fuseBoxMessageHeaderV2.meta.device_id
                && x.actualStart != null
                && x.actualEnd == null
                && x.cancel == false
                && x.end <= currentTime).ToListAsync();

            if (fuseboxSchedRegMsg != null && fuseboxSchedRegMsg.Count > 0)
            {
                foreach (var item in fuseboxSchedRegMsg)
                {
                    item.actualEnd = currentTime;
                  //  Console.WriteLine($"LogEndTime for: {item.FuseBoxMessageHeader.Id} at starttime: {item.start} at endtime: {item.end}");
                }
               

                await dbContext.SaveChangesAsync();
            }
        }       
    }

    private async Task LogEndTimeForCancel()
    {
        long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var fuseboxSchedRegMsg = await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader).
                Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
                && x.FuseBoxMessageHeader.device_id == fuseBoxMessageHeaderV2.meta.device_id
                && x.actualStart != null
                && x.actualEnd == null
                && x.cancel == false).ToListAsync();

            if (fuseboxSchedRegMsg != null && fuseboxSchedRegMsg.Count > 0)
            {
                foreach (var item in fuseboxSchedRegMsg)
                {
                    item.actualEnd = currentTime;
                  //  Console.WriteLine($"LogEndTimeForCancel for: {item.FuseBoxMessageHeader.Id} at starttime: {item.start} at endtime: {item.end}");
                }


                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task LogEndTimeForOtherOpenTasks()
    {
        long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();

        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var fuseboxSchedRegMsg = await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader).
                Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
                && x.FuseBoxMessageHeader.device_id == fuseBoxMessageHeaderV2.meta.device_id
                && x.FuseBoxMessageHeader.m_id != fuseBoxMessageHeaderV2.meta.m_id
                && x.actualStart != null
                && x.actualEnd  == null
                && x.end <= currentTime
                && x.FuseBoxMessageHeader.m_id != fuseBoxMessageHeaderV2.meta.m_id).ToListAsync();

            if (fuseboxSchedRegMsg != null && fuseboxSchedRegMsg.Count > 0)
            {
                foreach (var item in fuseboxSchedRegMsg)
                {
                    if (item.actualStart == null)
                    {
                        item.actualStart = currentTime;
                    }

                    item.actualEnd = currentTime;
                 //   Console.WriteLine($"LogEndTimeForOtherOpenTasks for: {item.FuseBoxMessageHeader.Id} at starttime: {item.start} at endtime: {item.end}");
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task LogStartTime()
    {
        using (var dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var fuseboxSchedRegMsg = await dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader).
            Where(x => x.FuseBoxMessageHeader.m_type == FuseBoxMessageType.sched_reg
            && x.FuseBoxMessageHeader.device_id == fuseBoxMessageHeaderV2.meta.device_id
            && x.FuseBoxMessageHeader.m_id == fuseBoxMessageHeaderV2.meta.m_id
            && x.actualStart == null
            && x.cancel == false).FirstOrDefaultAsync();

            if (fuseboxSchedRegMsg != null)
            {
                fuseboxSchedRegMsg!.actualStart = DateTimeOffset.Now.ToUnixTimeSeconds();

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
