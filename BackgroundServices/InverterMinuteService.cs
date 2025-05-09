using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using System.Timers;
using Newtonsoft.Json;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Dto;
using Polly;
using MQTTnet.Adapter;
using MQTTnet.Exceptions;
using SmartElectricityAPI.Processors;

namespace SmartElectricityAPI.BackgroundServices;

public class InverterMinuteService : IHostedService, IDisposable
{
    private IMqttLogger mqttLogger;
    private RedisCacheService redisCacheService;
    private List<RegisteredInverter> registeredInverters = new List<RegisteredInverter>();
    private IMqttClient mqttClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim processLock = new SemaphoreSlim(1, 1);

    public InverterMinuteService(IMqttLogger mqttLogger, IServiceProvider serviceProvider)
    {
        this.mqttLogger = mqttLogger;

       // ConnectMqttClient().Wait();
        _serviceProvider = serviceProvider;
    }
    private async Task ConnectMqttClient()
    {
        var retryPolicy = Policy
            .Handle<MqttConnectingFailedException>()
            .Or<MqttCommunicationException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                (exception, timeSpan, retryCount, context) =>
                {
                    //mqttLogger.LogError($"Failed to connect to MQTT broker (Attempt {retryCount}): {exception.Message}");
                }
            );

        try
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(CredentialHelpers.MqttServerBasicSettings().Server, CredentialHelpers.MqttServerBasicSettings().Port)
                .WithCredentials(CredentialHelpers.MqttServerBasicSettings().Username, CredentialHelpers.MqttServerBasicSettings().Password)
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))  // Add keep-alive
                .WithTimeout(TimeSpan.FromSeconds(10))          // Add timeout
                .Build();

            await retryPolicy.ExecuteAsync(async () =>
                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None)
            );

            // Set up reconnection handler
            mqttClient.DisconnectedAsync += async e =>
            {
              //  mqttLogger.LogWarning("MQTT Client disconnected. Attempting to reconnect...");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqttClient.ConnectAsync(mqttClientOptions);
                }
                catch (Exception ex)
                {
                   // mqttLogger.LogError($"Failed to reconnect: {ex.Message}");
                }
            };
        }
        catch (Exception ex)
        {
          //  mqttLogger.LogError($"Fatal MQTT connection error: {ex.Message}");
            throw;
        }
    }


    public async void TimerElapsedEveryMinute(object sender, ElapsedEventArgs e)
    {
        var currentTime = DateTime.Now;
        int currentMinute = currentTime.Minute;
        if (!(currentMinute == 59 || currentMinute == 0 || currentMinute == 1))
        {
            await ProcessMinMaxThresoldState();

            await ProcessBatteryButtons();
        }

        if (currentMinute == 20 || currentMinute == 40 || currentMinute == 57)
        {
            await DeleteOldRecords();
        }

        if (currentTime.Hour >= 15 && currentTime.Hour <= 23)
        {
            if (currentMinute == 8 || currentMinute == 22 || currentMinute == 28 || currentMinute == 44 || currentMinute == 51)
            {
                var WeatherApiComService = new WeatherApiComService(_serviceProvider.GetRequiredService<IHttpClientFactory>(), _serviceProvider.GetRequiredService<IConfiguration>());
                CompanyPlanCalculator companyPlanCalculator = new CompanyPlanCalculator(WeatherApiComService);
                await companyPlanCalculator.CalculateForMissingInverter();
                await companyPlanCalculator.CalculateForMissingDevices();
            }
        }

        // var currentTimeCalculation = DateTime.Now;

        var nextHour = currentTime.AddMinutes(Constants.CheckInverterManualButtonsStateEveryXminute).AddSeconds(-currentTime.Second);
        // var nextHour = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second);
        var timeUntilNextHour = nextHour - currentTime;

        // Reset the timer interval
        var timer = (System.Timers.Timer)sender;
        timer.Interval = timeUntilNextHour.TotalMilliseconds;

    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await ConnectMqttClient();

        if (!Debugger.IsAttached)
        {
            Console.WriteLine("Starting service");

            using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
            {
                registeredInverters = await _dbContext.RegisteredInverter.ToListAsync();

                redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());

                var currentTime = DateTime.Now;
                var nextHour = currentTime.AddMinutes(Constants.CheckInverterManualButtonsStateEveryXminute).AddSeconds(-currentTime.Second);
                //   var nextHour = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second);
                var timeUntilNextHour = nextHour - currentTime;
                var timer = new System.Timers.Timer(timeUntilNextHour.TotalMilliseconds);

                timer.Elapsed += TimerElapsedEveryMinute;
                timer.Start();
            }
        }

    }

    private async Task EnsureConnected()
    {
        if (mqttClient?.IsConnected != true)
        {
            await ConnectMqttClient();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping service");
        if (mqttClient?.IsConnected == true)
        {
            await mqttClient.DisconnectAsync();
        }

        Dispose();
      
    }

    public void Dispose()
    {
        if (mqttClient != null)
        {
            try
            {
                if (mqttClient.IsConnected)
                {
                    mqttClient.DisconnectAsync().Wait();
                }
                mqttClient.Dispose();
                processLock.Dispose();
            }
            catch (Exception ex)
            {
             
            }
        }


    }

    private async Task DeleteOldRecords()
    {
        foreach (var item in registeredInverters)
        {
            await redisCacheService.RemoveOldRange(item.Id.ToString(), Constants.SofarStateUnixOffset);

            await redisCacheService.RemoveOldRange(Constants.CacheKeys.MqttLogKey(item.Id), Constants.MqttLogUnixOffset);
        }
    }

    private async Task ProcessBatteryButtons()
    {
        using (var _dbContext = await  new DatabaseService().CreateDbContextAsync())
        {
            var inverterBatteries = await _dbContext.InverterBattery.Where(x=> x.Enabled).Include(x => x.Inverter).ThenInclude(x => x.RegisteredInverter).ToListAsync();
            var inverterTypeCompanyActions = await _dbContext.InverterTypeCompanyActions.ToListAsync();
            var inverterTypeCommands = await _dbContext.InverterTypeCommands.ToListAsync();
            var inverterTypes = await _dbContext.InverterType.ToListAsync();
            var companies = await _dbContext.Company.ToListAsync();

            foreach (var battery in inverterBatteries)
            {
                var actionType = inverterTypeCompanyActions
                    .Where(x => x.InverterId == battery.InverterId && x.ActionState == true && x.ActionTypeCommand != ActionTypeCommand.AutoMode)
                    .Select(x => x.ActionTypeCommand)
                    .FirstOrDefault();

                var selectedCompany = companies.FirstOrDefault(x => x.Id == battery.Inverter.CompanyId);

                var selectedInverterTypeCompanyActions = inverterTypeCompanyActions.Where(x => x.InverterId == battery.InverterId).ToList();

                var task = actionType switch
                {
                    ActionTypeCommand.SellRemainingSunNoCharging => ProcessSellRemainingSunNoCharging(battery, selectedInverterTypeCompanyActions, inverterTypeCommands, selectedCompany),
                    ActionTypeCommand.SelfUse => ProcessSelfUse(battery, selectedInverterTypeCompanyActions, inverterTypeCommands, inverterTypes, selectedCompany),
                    ActionTypeCommand.ChargeMax => ProcessChargeMax(battery, selectedInverterTypeCompanyActions, inverterTypeCommands, inverterTypes),
                    ActionTypeCommand.ChargeWithRemainingSun => ProcessChargeWithRemainingSun(battery, selectedInverterTypeCompanyActions, inverterTypeCommands, selectedCompany),
                    _ => Task.CompletedTask
                };

                await task;
            }
        }
    }

    private async Task ProcessChargeMax(InverterBattery battery, List<Models.InverterTypeCompanyActions> inverterTypeCompanyActions, List<InverterTypeCommands> inverterTypeCommands, List<Models.InverterType> inverterTypes)
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
            var chargeMaxAction = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.ChargeMax);
            var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == chargeMaxAction.InverterTypeId && x.ActionType == chargeMaxAction.ActionType);
            string topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

            var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

            var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                    x.InverterId == battery.InverterId
                                                                    && x.MQttMessageType == MQttMessageType.Regular
                                                                    && x.Direction == Direction.Out);

            string payload = (battery.ChargingPowerFromGridKWh * 1000).ToString();

            bool shouldDispatchCommand = true;

            if (lastLoqRecord != null
                && lastLoqRecord.Topic == topicForMqtt
                && lastLoqRecord.Payload == payload
                && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
            //    && lastLoqRecord.ActionTypeCommand == ActionTypeCommand.ChargeMax
                && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
            {
                shouldDispatchCommand = false;
            }
            await EnsureConnected();
            await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payload)
                .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSChargeMax, topicForMqtt, payload, ActionTypeCommand.ChargeMax, shouldDispatchCommand, MQttMessageType.Regular);

            await _dbContext.SaveChangesAsync();
        }
    }
    private async Task ProcessSelfUse(InverterBattery battery, List<Models.InverterTypeCompanyActions> inverterTypeCompanyActions, List<InverterTypeCommands> inverterTypeCommands, List<Models.InverterType> inverterTypes, Company company)
    {
        if (battery.InverterId == 28)
        {

        }

        DateTime currentDateTime = DateTime.Now;

        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var last5StateTransactions = await redisCacheService.GetKeyValue<SofarState>(battery.Inverter.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
            //.OrderByDescending(x => x.CreatedAt).Take(5).ToList();

            if (last5StateTransactions.Count < 4)
            {
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var averageBatteryProduction = last5StateTransactions.Sum(x => x.solarPV) / last5StateTransactions.Count;
                var averageInverterConsumption = last5StateTransactions.Sum(x => x.consumption) / last5StateTransactions.Count;

                var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                var sellRemainingSunAction = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == Enums.ActionTypeCommand.SellRemainingSunNoCharging);
                var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == sellRemainingSunAction.InverterTypeId && x.ActionType == sellRemainingSunAction.ActionType);
                string topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                if (last5StateTransactions.FirstOrDefault()!.batterySOC >= battery.MaxLevel) // && averageInverterConsumption < averageBatteryProduction)
                {
                    string payLoad = "0";

                    var currentHourSpotPrice = await _dbContext.SpotPrice
                     .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                             && x.DateTime.Month == currentDateTime.Month
                             && x.DateTime.Day == currentDateTime.Day
                             && x.DateTime.Hour == currentDateTime.Hour
                             && x.RegionId == company.RegionId);

                    DateTime previousHourDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0).AddHours(-1);

                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService);

                    await batteryButtonActions.ProcessSellRemainingSunNoCharging(previousHourDateTime, currentHourSpotPrice, currentHourSpotPrice, battery.Inverter, battery, company, _dbContext, false, false, ActionTypeCommand.SelfUse);

                    if (currentHourSpotPrice.PriceNoTax < company.BrokerPurchaseMargin)
                    {
                        topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                        if (averageInverterConsumption >= averageBatteryProduction)
                        {
                            payLoad = ((Convert.ToInt32(battery.Inverter.MaxPower) * 1000) / battery.Inverter.NumberOfInverters).ToString();
                        }
                        else
                        {
                            topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                            payLoad = "0";
                        }

                        var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);
                        string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                        string antiRefluxPayload = "0";

                        if (currentHourSpotPrice.PriceNoTax > company!.BrokerPurchaseMargin)
                        {
                            antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString();
                        }

                        var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                     

                        var lastLoqRecordAntireflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                           x.InverterId == battery.InverterId &&
                           x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                        bool shouldDispatchCommandAntiReflux = true;

                        if (lastLoqRecordAntireflux != null
                            && lastLoqRecordAntireflux.Topic == antiRefluxTopic
                            && lastLoqRecordAntireflux.Payload == antiRefluxPayload
                            && lastLoqRecordAntireflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntireflux.CreatedAt!, 120)
                            && lastLoqRecordAntireflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                        {
                            shouldDispatchCommandAntiReflux = false;
                        }
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                            .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSelfUse, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);
                    }
                    else
                    {
                        if (averageInverterConsumption >= averageBatteryProduction)
                        {
                            //Antireflux 0
                            var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);
                            string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                            await EnsureConnected();
                            await new MqttClientService(mqttClient, mqttLogger, true).PublishMessages(antiRefluxTopic, "0")
                            .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSelfUse, antiRefluxTopic, "0", ActionTypeCommand.SelfUse, true, MQttMessageType.ThreephaseAntireflux);

                            topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                            payLoad = ((Convert.ToInt32(battery.Inverter.MaxPower) * 1000) / battery.Inverter.NumberOfInverters).ToString();
                        }
                        else
                        {
                            topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                            payLoad = "0";
                        }
                    }

                    // Fix for CS1061: Await the Task<List<MqttMessageLog>> before calling OrderByDescending.  
                    var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                           x.InverterId == battery.InverterId
                                                    && x.MQttMessageType == MQttMessageType.Regular
                                                    && x.MqttMessageOrigin == MqttMessageOrigin.BBSSelfUse
                                                    && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);


                    bool shouldDispatchCommand = true;

                    if (lastLoqRecord != null
                        && lastLoqRecord.Topic == topicForMqtt
                        && lastLoqRecord.Payload == payLoad
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120)
                        && lastLoqRecord.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payLoad)
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSelfUse, topicForMqtt, payLoad, ActionTypeCommand.SellRemainingSunNoCharging, shouldDispatchCommand, MQttMessageType.Regular);
                }
                else if (last5StateTransactions.FirstOrDefault()!.batterySOC <= battery.MinLevel)
                {
                    var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                        x.InverterId == battery.InverterId
                                    && x.MQttMessageType == MQttMessageType.Regular
                                    && x.MqttMessageOrigin == MqttMessageOrigin.BBSSelfUse
                                    && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);

                    topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";

                    bool shouldDispatchCommand = true;

                    if (lastLoqRecord != null
                        && lastLoqRecord.Topic == topicForMqtt
                        && lastLoqRecord.Payload == "0"
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120)
                        && lastLoqRecord.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, "0")
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSelfUse, topicForMqtt, "0", ActionTypeCommand.SellRemainingSunNoCharging, shouldDispatchCommand, MQttMessageType.Regular, $"{JsonConvert.SerializeObject(last5StateTransactions.FirstOrDefault())}  battery minlevel:{battery.MinLevel} registered inverter: {registeredMqttInverter}");



                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);
                    string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                    var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                     x.InverterId == battery.InverterId
                                                                     && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                    bool shouldDispatchCommand = true;

                    if (lastLoqRecord != null
                        && lastLoqRecord.Topic == antiRefluxTopic
                        && lastLoqRecord.Payload == "0"
                        && lastLoqRecord.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120)
                        && lastLoqRecord.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(antiRefluxTopic, "0")
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSelfUse, antiRefluxTopic, "0", ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommand, MQttMessageType.ThreephaseAntireflux);

                    var mqttTopic = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                    var payLoad = $"{(battery.Inverter.MaxPower * 1000) / battery.Inverter.NumberOfInverters}";

                    // Fix for CS1061: Await the Task<List<MqttMessageLog>> before calling OrderByDescending.  
                    var mqttLogFromRedisRegular = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecordRegular = mqttLogFromRedisRegular.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                       x.InverterId == battery.InverterId &&
                       x.MQttMessageType == MQttMessageType.Regular &&
                       x.Direction == Direction.Out);

                    bool shouldDispatchRegularCommand = true;

                    if (lastLoqRecordRegular != null
                        && lastLoqRecordRegular.Topic == mqttTopic
                        && lastLoqRecordRegular.Payload == payLoad
                        && lastLoqRecordRegular.MQttMessageType == MQttMessageType.Regular
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordRegular.CreatedAt!, 120)
                        && lastLoqRecordRegular.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchRegularCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchRegularCommand).PublishMessages(mqttTopic, payLoad)
                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSelfUse, mqttTopic, payLoad, ActionTypeCommand.SelfUse, shouldDispatchRegularCommand, MQttMessageType.Regular);


                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task ProcessSellRemainingSunNoCharging(InverterBattery battery, List<Models.InverterTypeCompanyActions> inverterTypeCompanyActions, List<InverterTypeCommands> inverterTypeCommands, Company company)
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var last5StateTransactions = await redisCacheService.GetKeyValue<SofarState>(battery.Inverter.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
                last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(5).ToList();

            if (last5StateTransactions.Count < 4)
            {/*
                _dbContext.Log.Add(new Log
                {
                    Level = "High",
                    Message = $"Not enough inverter transactions received within last {Constants.SofarStateMinutesPeriod} minutes for inverter: {battery.Inverter.RegisteredInverterId} at company {battery.Inverter.CompanyId}"
                });
                */

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                var sellRemainingSunNoChargingAction = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == Enums.ActionTypeCommand.SellRemainingSunNoCharging);
                var averageSolarProduction = last5StateTransactions.Sum(x => x.solarPV) / last5StateTransactions.Count;
                var averageInverterConsumption = last5StateTransactions.Sum(x => x.consumption) / last5StateTransactions.Count;
                //Send inverter mode control passive
                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService);

                if (battery.Inverter.UseInverterSelfUse)
                {
                    await batteryButtonActions.ProcessInverterModeControl(battery.Inverter, _dbContext, ActionTypeCommand.PassiveMode);
                }

                if (last5StateTransactions.FirstOrDefault()!.batterySOC >= battery.MaxLevel)
                {
                    //TODO: Vaadata kas päikest on üle maja tarbimise siis antireflux 150 
                    //Kui on puudu siis discharge max ja antireflux 0
                    string payload = "0";
 
                    DateTime currentDateTime = DateTime.Now;

                    var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);
                    string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                    string antiRefluxPayload = "0";

                    var currentHourSpotPrice = await _dbContext.SpotPrice
                        .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                            && x.DateTime.Month == currentDateTime.Month
                            && x.DateTime.Day == currentDateTime.Day
                            && x.DateTime.Hour == currentDateTime.Hour
                            && x.RegionId == company.RegionId);

                    if (averageSolarProduction > averageInverterConsumption
                        && currentHourSpotPrice.PriceNoTax - company!.BrokerPurchaseMargin > 0)
                    {
                        antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString();
                        mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);
                        antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                     
                    }
                    /* 
                    else
                    {
                        payload = $"{battery.Inverter.MaxPower * 1000}";
                    }
                    */

                    var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecordAntireflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                            x.InverterId == battery.InverterId
                                                            && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                    bool shouldDispatchCommandAntiReflux = true;

                    if (lastLoqRecordAntireflux != null
                        && lastLoqRecordAntireflux.Topic == antiRefluxTopic
                        && lastLoqRecordAntireflux.Payload == antiRefluxPayload
                        && lastLoqRecordAntireflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntireflux.CreatedAt!, 120)
                        && lastLoqRecordAntireflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommandAntiReflux = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSellRemainingSunNoCharging, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);


                    var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId);
                    var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                    var mqttLogFromRedisRegular = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecordRegular = mqttLogFromRedisRegular.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                            x.InverterId == battery.InverterId
                                                            && x.MQttMessageType == MQttMessageType.Regular
                                                            && x.Direction == Direction.Out);

                    bool shouldDispatchRegularCommand = true;

                    if (lastLoqRecordRegular != null
                        && lastLoqRecordRegular.Topic == topicForMqtt
                        && lastLoqRecordRegular.Payload == payload
                        && lastLoqRecordRegular.MQttMessageType == MQttMessageType.Regular
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordRegular.CreatedAt!, 120))
                    {
                        shouldDispatchRegularCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchRegularCommand).PublishMessages(topicForMqtt, payload)
                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSChargeWithRemainingSun, topicForMqtt, payload, ActionTypeCommand.SellRemainingSunNoCharging, shouldDispatchRegularCommand, MQttMessageType.Regular);

   
                    await _dbContext.SaveChangesAsync();
                }
                else if (last5StateTransactions.FirstOrDefault()!.batterySOC <= battery.MinLevel
                    &&   averageSolarProduction < averageInverterConsumption)
                {
                    string payload = "0";

                    DateTime currentDateTime = DateTime.Now;

                    var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);
                    string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                    var currentHourSpotPrice = await _dbContext.SpotPrice
                        .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                            && x.DateTime.Month == currentDateTime.Month
                            && x.DateTime.Day == currentDateTime.Day
                            && x.DateTime.Hour == currentDateTime.Hour
                            && x.RegionId == company.RegionId);

                    string antiRefluxPayload = "0";
                    
                    if (currentHourSpotPrice.PriceNoTax - company!.BrokerPurchaseMargin > 0)
                    {
                        antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString();
                    }


                    mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);
                    antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
    


                    var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecordAntireflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                            x.InverterId == battery.InverterId
                                                            && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                    bool shouldDispatchCommandAntiReflux = true;

                    if (lastLoqRecordAntireflux != null
                        && lastLoqRecordAntireflux.Topic == antiRefluxTopic
                        && lastLoqRecordAntireflux.Payload == antiRefluxPayload
                        && lastLoqRecordAntireflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntireflux.CreatedAt!, 120)
                        && lastLoqRecordAntireflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommandAntiReflux = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSellRemainingSunNoCharging, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);


                    var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId);
                    var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                    var mqttLogFromRedisRegular = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecordRegular = mqttLogFromRedisRegular.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                            x.InverterId == battery.InverterId
                                                            && x.MQttMessageType == MQttMessageType.Regular
                                                            && x.Direction == Direction.Out);

                    bool shouldDispatchRegularCommand = true;

                    if (lastLoqRecordRegular != null
                        && lastLoqRecordRegular.Topic == topicForMqtt
                        && lastLoqRecordRegular.Payload == payload
                        && lastLoqRecordRegular.MQttMessageType == MQttMessageType.Regular
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordRegular.CreatedAt!, 120))
                    {
                        shouldDispatchRegularCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchRegularCommand).PublishMessages(topicForMqtt, payload)
                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSChargeWithRemainingSun, topicForMqtt, payload, ActionTypeCommand.SellRemainingSunNoCharging, shouldDispatchRegularCommand, MQttMessageType.Regular);

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    DateTime currentDateTime = DateTime.Now;

                    string payLoad = "";
                    var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";

                    if (averageInverterConsumption >= averageSolarProduction)
                    {

                        var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);
                        string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                        var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                        var lastLoqRecordAntireflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                x.InverterId == battery.InverterId
                                                                && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                        bool shouldDispatchCommandAntiReflux = true;

                        if (lastLoqRecordAntireflux != null
                            && lastLoqRecordAntireflux.Topic == antiRefluxTopic
                            && lastLoqRecordAntireflux.Payload == "0"
                            && lastLoqRecordAntireflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntireflux.CreatedAt!, 120)
                            && lastLoqRecordAntireflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                        {
                            shouldDispatchCommandAntiReflux = false;
                        }
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, "0")
                            .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSellRemainingSunNoCharging, antiRefluxTopic, "0", ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);


                        payLoad = ((Convert.ToInt32(battery.Inverter.MaxPower) * 1000) / battery.Inverter.NumberOfInverters).ToString();
                    }
                    else
                    {
                        var currentHourSpotPrice = await _dbContext.SpotPrice
                            .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                                && x.DateTime.Month == currentDateTime.Month
                                && x.DateTime.Day == currentDateTime.Day
                                && x.DateTime.Hour == currentDateTime.Hour
                                && x.RegionId == company.RegionId);

                        var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == battery.Inverter.InverterTypeId && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);
                        string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                        string antiRefluxPayload = "0";

                        if (currentHourSpotPrice.PriceNoTax > company!.BrokerPurchaseMargin)
                        {
                            antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString();
                        }

                        var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                        var lastLoqRecordAntireflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                x.InverterId == battery.InverterId
                                                                && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                        bool shouldDispatchCommandAntiReflux = true;

                        if (lastLoqRecordAntireflux != null
                            && lastLoqRecordAntireflux.Topic == antiRefluxTopic
                            && lastLoqRecordAntireflux.Payload == antiRefluxPayload
                            && lastLoqRecordAntireflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntireflux.CreatedAt!, 120)
                            && lastLoqRecordAntireflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                        {
                            shouldDispatchCommandAntiReflux = false;
                        }
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                            .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSellRemainingSunNoCharging, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);



                        topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                        payLoad = "0";
                    }

                    var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                    x.InverterId == battery.InverterId
                                                    && x.MQttMessageType == MQttMessageType.Regular
                                                    && x.MqttMessageOrigin == MqttMessageOrigin.BBSSellRemainingSunNoCharging
                                                    && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);


                    bool shouldDispatchCommand = true;

                    if (lastLoqRecord != null
                    && lastLoqRecord.Topic == topicForMqtt
                    && lastLoqRecord.Payload == payLoad
                    && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120)
                    && lastLoqRecord.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchCommand = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payLoad)
                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSellRemainingSunNoCharging, topicForMqtt, payLoad, ActionTypeCommand.SellRemainingSunNoCharging, shouldDispatchCommand, MQttMessageType.Regular);


                }

            }
        }
    }

    private async Task ProcessChargeWithRemainingSun(InverterBattery battery, List<Models.InverterTypeCompanyActions> inverterTypeCompanyActions, List<InverterTypeCommands> inverterTypeCommands, Company company)
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            ActionType returnedActionTypeToUse = ActionType.None;

            var last5StateTransactions = await redisCacheService.GetKeyValue<SofarState>(battery.Inverter.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
               last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(5).ToList();

            if (last5StateTransactions.Count > 0)
            {
                var averageBatteryProduction = last5StateTransactions.Sum(x => x.solarPV) / last5StateTransactions.Count;

                var averageInverterConsumption = last5StateTransactions.Sum(x => x.consumption) / last5StateTransactions.Count;

                var remainingSolarPower = averageBatteryProduction - (averageInverterConsumption + 250);

                var currentDateTime = DateTime.Now;

                var currentHourSpotPrice = await _dbContext.SpotPrice
                 .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                         && x.DateTime.Month == currentDateTime.Month
                         && x.DateTime.Day == currentDateTime.Day
                         && x.DateTime.Hour == currentDateTime.Hour
                         && x.RegionId == company.RegionId);

                string payLoad = "";


                var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                var sellRemainingSunNoChargingAction = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == Enums.ActionTypeCommand.SellRemainingSunNoCharging);
                var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.InverterTypeId == sellRemainingSunNoChargingAction.InverterTypeId && x.ActionType == sellRemainingSunNoChargingAction.ActionType);
                var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                var lastLoqRecord = mqttLogFromRedis != null && mqttLogFromRedis.Count > 0 ? mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                            x.InverterId == battery.InverterId
                                                            && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                            && x.MQttMessageType == MQttMessageType.Regular
                                                            && x.Direction == Direction.Out) : null;


                ActionTypeCommand actionTypeCommandToUse = ActionTypeCommand.ChargeWithRemainingSun;

                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService, mqttClient);

                if (last5StateTransactions.Average(x => x.solarPV) <= 50
                    && last5StateTransactions.Average(x => x.batterySOC) <= battery.MinLevel)
                {
                    bool shouldDispatchAntiReflux = true;
                    var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                        x.InverterTypeId == battery.Inverter.InverterTypeId
                        && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

                    string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                    var lastLoqRecordAntiReflux = mqttLogFromRedis != null && mqttLogFromRedis.Count > 0 ? mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                        x.InverterId == battery.InverterId
                        && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                        && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && x.Direction == Direction.Out) : null;

                    if (lastLoqRecordAntiReflux != null
                        && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                        && lastLoqRecordAntiReflux.Payload == "0"
                        && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120)
                        && lastLoqRecordAntiReflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchAntiReflux = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchAntiReflux).PublishMessages(antiRefluxTopic, "0")
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, "0", ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchAntiReflux, MQttMessageType.ThreephaseAntireflux);

                    topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                    payLoad = "0";

                }
                else if (last5StateTransactions.Average(x => x.solarPV) <= 50
                         && last5StateTransactions.Average(x => x.batterySOC) > battery.MinLevel)
                {

                    bool shouldDispatchAntiReflux = true;
                    var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                        x.InverterTypeId == battery.Inverter.InverterTypeId
                        && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                    string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                    var antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString();

                    var lastLoqRecordAntiReflux = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                        x.InverterId == battery.InverterId
                        && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                        && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && x.Direction == Direction.Out);

                    if (lastLoqRecordAntiReflux != null
                        && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                        && lastLoqRecordAntiReflux.Payload == antiRefluxPayload
                        && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120)
                        && lastLoqRecordAntiReflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                    {
                        shouldDispatchAntiReflux = false;
                    }
                    await EnsureConnected();
                    await new MqttClientService(mqttClient, mqttLogger, shouldDispatchAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchAntiReflux, MQttMessageType.ThreephaseAntireflux);

                    topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                    payLoad = "0";
                }
                else
                {
                    if (remainingSolarPower >= 0)
                    {
                        topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";


                        bool shouldDispatchCommandLocalProcessSelfUse = true;

                        var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                        x.InverterTypeId == battery.Inverter.InverterTypeId
                        && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

                        string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                        var lastLoqRecordAntiReflux = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                    x.InverterId == battery.InverterId
                                                    && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                    && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                                                    && x.Direction == Direction.Out);

                        if (lastLoqRecordAntiReflux != null
                            && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                            && lastLoqRecordAntiReflux.Payload == "0"
                            && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120)
                            && lastLoqRecordAntiReflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                        {
                            shouldDispatchCommandLocalProcessSelfUse = false;
                        }
                        await processLock.WaitAsync();
                        payLoad = await batteryButtonActions.ProcessSelfUse(battery.Inverter, battery, currentHourSpotPrice, _dbContext, shouldDispatchCommandLocalProcessSelfUse);
                        processLock.Release();
                    }
                    else
                    {
                        // DateTime previousHourDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0).AddHours(-1);

                        // (payLoad, returnedActionTypeToUse) = await batteryButtonActions.ProcessSellRemainingSunNoCharging(previousHourDateTime, currentHourSpotPrice, currentHourSpotPrice, battery.Inverter, battery, company, _dbContext, true, false, ActionTypeCommand.ChargeWithRemainingSun, averageBatteryProduction);
                        bool shouldDispatchAntiReflux = true;
                        var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                            x.InverterTypeId == battery.Inverter.InverterTypeId
                            && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                        string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";
                        var antiRefluxPayload = InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString();

                        var lastLoqRecordAntiReflux = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                            x.InverterId == battery.InverterId
                            && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                            && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                            && x.Direction == Direction.Out);

                        if (lastLoqRecordAntiReflux != null
                            && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                            && lastLoqRecordAntiReflux.Payload == antiRefluxPayload
                            && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                            && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120)
                            && lastLoqRecordAntiReflux.CreatedAt!.Value.Hour == currentDateTime.Hour)
                        {
                            shouldDispatchAntiReflux = false;
                        }
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, mqttLogger, shouldDispatchAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                            .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.AutoServiceBattery, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchAntiReflux, MQttMessageType.ThreephaseAntireflux);

                        payLoad = "0";

                        if (returnedActionTypeToUse == ActionType.Charge)
                        {
                            topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == battery.Inverter.InverterTypeId).MqttTopic}";
                        }
                    }
                }



                bool shouldDispatchCommand = true;

                if (lastLoqRecord != null
                    && lastLoqRecord.Topic == topicForMqtt
                    && lastLoqRecord.Payload == payLoad
                    && lastLoqRecord.ActionTypeCommand == actionTypeCommandToUse
                    && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120)
                    && lastLoqRecord.CreatedAt!.Value.Hour == currentDateTime.Hour)
                {
                    shouldDispatchCommand = false;
                }

                await _dbContext.SaveChangesAsync();


                if (shouldDispatchCommand)
                {
                    foreach (var record in inverterTypeCompanyActions)
                    {
                        if (record.ActionTypeCommand == actionTypeCommandToUse)
                        {
                            record.ActionState = true;
                        }
                        else
                        {
                            if (record.ActionTypeCommand != ActionTypeCommand.AutoMode)
                            {
                                record.ActionState = false;
                            }
                        }
                        _dbContext.Entry(record).State = EntityState.Modified;
                    }

                    await _dbContext.SaveChangesAsync();
                }
                await EnsureConnected();
                await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payLoad)
                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSChargeWithRemainingSun, topicForMqtt, payLoad, actionTypeCommandToUse, shouldDispatchCommand, MQttMessageType.Regular);
            }
        }
    }
    private async Task ProcessMinMaxThresoldState()
    {
        //  var stopWatch = new Stopwatch();
        //  stopWatch.Start();

        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var inverterBatteries = await _dbContext.InverterBattery.Where(x=> x.Enabled).Include(x => x.Inverter).ThenInclude(x => x.RegisteredInverter).ToListAsync();
            var companies = await _dbContext.Company.ToListAsync();

            var inverterTypeCommands = await _dbContext.InverterTypeCommands.ToListAsync();

            foreach (var battery in inverterBatteries)
            {

                var inverterTypeCompanyActions = await _dbContext.InverterTypeCompanyActions.Where(x => x.InverterId == battery.InverterId).ToListAsync();
                var company = companies.FirstOrDefault(x => x.Id == battery.Inverter.CompanyId);
                if (inverterTypeCompanyActions.Any(x =>
                x.InverterId == battery.InverterId
                && x.ActionState == true
                && x.ActionTypeCommand != ActionTypeCommand.AutoMode)) //Must be keep battery level
                {
                    var last5StateTransactions = await redisCacheService.GetKeyValue<SofarState>(battery.Inverter.RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
                       last5StateTransactions = last5StateTransactions.OrderByDescending(x => x.CreatedAt).Take(5).ToList();

                    if (last5StateTransactions.Count < 4)
                    {
                          await _dbContext.SaveChangesAsync();
                        // continue;
                    }
                    else
                    {
                        var averageBatteryLevel = last5StateTransactions.Sum(x => x.batterySOC) / last5StateTransactions.Count;
                        var actionStateFullDischarge = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower);
                        var actionStateSelfUse = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.SelfUse);
                        var actionStateFullCharge = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.ChargeMax);
                        var actionStateSellRemainingSunNoCharging = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);
                        var actionStateChargeWithRemainingSun = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun);
                        var actionStateHzMarket = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.HzMarket);
                        var actionStateInverterSelfUse = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.InverterSelfUse);


                        var averageSolarProduction = last5StateTransactions.Sum(x => x.solarPV) / last5StateTransactions.Count;
                        var averageInverterConsumption = last5StateTransactions.Sum(x => x.consumption) / last5StateTransactions.Count;

                        var fuseboxCommandType = FuseBoxCommandType.STANDBY;

                        if (actionStateHzMarket != null && actionStateHzMarket.ActionState)
                        {
                            var registeredInverter = battery.Inverter.RegisteredInverter.Name;

                            var activeFuseBoxAction = await _dbContext.FuseBoxSchedRegMsg.Include(x => x.FuseBoxMessageHeader).Include(x => x.FuseBoxPowSet)
                                .Where(x => x.FuseBoxMessageHeader.device_id == registeredInverter && x.actualStart != null && x.actualEnd == null).FirstOrDefaultAsync();

                            if (activeFuseBoxAction != null)
                            {
                                if (activeFuseBoxAction.FuseBoxPowSet.PowerValue < 0)
                                {
                                    fuseboxCommandType = FuseBoxCommandType.DISCHARGE;
                                }
                                else
                                {
                                    fuseboxCommandType = FuseBoxCommandType.CHARGE;
                                }
                            }
                        }

                            if (actionStateFullDischarge!.ActionState &&
                                last5StateTransactions.FirstOrDefault()!.batterySOC <= battery.MinLevel + battery.BatteryMinLevelWithConsumeMax)
                            {
                                foreach (var record in inverterTypeCompanyActions)
                                {
                                    if (record.ActionTypeCommand == ActionTypeCommand.SelfUse)
                                    {
                                        record.ActionState = true;
                                    }
                                    else
                                    {
                                        if (record.ActionTypeCommand != ActionTypeCommand.AutoMode)
                                        {
                                            record.ActionState = false;
                                        }
                                    }
                                    _dbContext.Entry(record).State = EntityState.Modified;
                                }

     
                                await _dbContext.SaveChangesAsync();

                            var InverterTypeCompanyAction = inverterTypeCompanyActions.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse);

           
                            var inverterBatteryButtonProcessor = new InverterBatteryButtonProcessor(company, InverterTypeCompanyAction.Id, redisCacheService, mqttLogger);
                            await inverterBatteryButtonProcessor.Process();
                            //Method used until 23.01.2025 await ProcessConsumeMaxMinLevel(inverterTypeCompanyActions, last5StateTransactions, inverterTypeCommands, battery, _dbContext, company);
                        }

                            if (last5StateTransactions.FirstOrDefault()!.batterySOC <= battery.MinLevel && actionStateSellRemainingSunNoCharging!.ActionState)
                            {


                                if (averageInverterConsumption >= averageSolarProduction)
                                {

                                    var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                                    var currentDateTime = DateTime.Now;

                                    var currentHourSpotPrice = await _dbContext.SpotPrice
                                     .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                                             && x.DateTime.Month == currentDateTime.Month
                                             && x.DateTime.Day == currentDateTime.Day
                                             && x.DateTime.Hour == currentDateTime.Hour
                                             && x.RegionId == company.RegionId);
                                    //Hinnakontroll - kui hind on kehv siis antireflux 0, vastasel juhul antireflux 150
                                    var actionToSendAntiReflux = currentHourSpotPrice.PriceNoTax > company.BrokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                                    var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                                                                        x.InverterTypeId == battery.Inverter.InverterTypeId
                                                                        && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

                                    string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                                    string antiRefluxPayload = actionToSendAntiReflux == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString() : "0";

                                    var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                    var lastLoqRecordAntiReflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                            x.InverterId == battery.Inverter.Id
                                                            && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                                    bool shouldDispatchCommandAntiReflux = true;

                                    if (lastLoqRecordAntiReflux != null
                                        && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                                        && lastLoqRecordAntiReflux.Payload == antiRefluxPayload
                                        && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120))
                                    {
                                        shouldDispatchCommandAntiReflux = false;
                                    }

                                    BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService);

                                    if (battery.Inverter.UseInverterSelfUse)
                                    {
                                        await batteryButtonActions.ProcessInverterModeControl(battery.Inverter, _dbContext, ActionTypeCommand.PassiveMode);
                                    }
                                await EnsureConnected();
                                await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);

                                    ActionTypeCommand actionTypeCommandToUse = ActionTypeCommand.SellRemainingSunNoCharging;
                                    string payload = "0";

                                    var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                    var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                                            x.InverterId == battery.InverterId
                                                                                            && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                                                            && x.MQttMessageType == MQttMessageType.Regular
                                                                                            && x.Direction == Direction.Out);


                                    var sellRemainingSunNoChargingAction = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);
                                    var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId);
                                    var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                                    bool shouldDispatchCommand = true;

                                    if (lastLoqRecord != null
                                        && lastLoqRecord.Topic == topicForMqtt
                                        && lastLoqRecord.Payload == payload
                                        && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                                        && (lastLoqRecord.ActionTypeCommand == actionTypeCommandToUse
                                        || lastLoqRecord.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging)
                                        && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                                    {
                                        shouldDispatchCommand = false;
                                    }
                                await EnsureConnected();
                                await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payload)
                                        .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, topicForMqtt, payload, actionTypeCommandToUse, shouldDispatchCommand, MQttMessageType.Regular);
                                }

                                continue;

                            }

                            if (last5StateTransactions.FirstOrDefault()!.batterySOC <= battery.MinLevel &&
                            (!actionStateFullCharge!.ActionState
                            && !actionStateChargeWithRemainingSun!.ActionState
                            && !actionStateSellRemainingSunNoCharging!.ActionState
                            || (actionStateHzMarket != null && actionStateHzMarket!.ActionState && fuseboxCommandType != FuseBoxCommandType.CHARGE)))
                            {
                                var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                                var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Charge && x.InverterTypeId == battery.Inverter.InverterTypeId);
                                var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";
                                var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                                        x.InverterId == battery.InverterId
                                                                                        && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                                                        && x.MQttMessageType == MQttMessageType.Regular
                                                                                        && x.Direction == Direction.Out);

                                ActionTypeCommand actionTypeCommandToUse = averageBatteryLevel <= battery.MinLevel ? ActionTypeCommand.ChargeWithRemainingSun : ActionTypeCommand.SellRemainingSunNoCharging;

                                bool shouldDispatchCommand = true;

                                if (lastLoqRecord != null
                                    && lastLoqRecord.Topic == topicForMqtt
                                    && lastLoqRecord.Payload == "0"
                                    && lastLoqRecord.ActionTypeCommand == actionTypeCommandToUse
                                    && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                                {
                                    shouldDispatchCommand = false;
                                }

                                if (shouldDispatchCommand)
                                {
                                    foreach (var record in inverterTypeCompanyActions)
                                    {
                                        if (record.ActionTypeCommand == actionTypeCommandToUse)
                                        {
                                            record.ActionState = true;
                                        }
                                        else
                                        {
                                            if (record.ActionTypeCommand != ActionTypeCommand.AutoMode)
                                            {
                                                record.ActionState = false;
                                            }
                                        }
                                        _dbContext.Entry(record).State = EntityState.Modified;
                                    }
             
                                    await _dbContext.SaveChangesAsync();
                                }

                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService);

                                if (battery.Inverter.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(battery.Inverter, _dbContext, ActionTypeCommand.PassiveMode);
                                }
                            await EnsureConnected();
                            await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, "0")
                                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, topicForMqtt, "0", actionTypeCommandToUse, shouldDispatchCommand, MQttMessageType.Regular);



                                continue;
                            }

                            if (last5StateTransactions.FirstOrDefault()!.batterySOC >= battery.MaxLevel &&
                                ((actionStateInverterSelfUse.ActionState || actionStateSelfUse.ActionState) && last5StateTransactions.FirstOrDefault().battery_power > 100))
                            {
                                var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                                var currentDateTime = DateTime.Now;

                                var currentHourSpotPrice = await _dbContext.SpotPrice
                                 .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                                         && x.DateTime.Month == currentDateTime.Month
                                         && x.DateTime.Day == currentDateTime.Day
                                         && x.DateTime.Hour == currentDateTime.Hour
                                         && x.RegionId == company.RegionId);
                                //Hinnakontroll - kui hind on kehv siis antireflux 0, vastasel juhul antireflux 150
                                var actionToSendAntiReflux = currentHourSpotPrice.PriceNoTax > company.BrokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                                var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                                                                    x.InverterTypeId == battery.Inverter.InverterTypeId
                                                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

                                string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                                string antiRefluxPayload = actionToSendAntiReflux == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString() : "0";

                                var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                var lastLoqRecordAntiReflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                        x.InverterId == battery.Inverter.Id
                                                        && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                                bool shouldDispatchCommandAntiReflux = true;

                                if (lastLoqRecordAntiReflux != null
                                    && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                                    && lastLoqRecordAntiReflux.Payload == antiRefluxPayload
                                    && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120))
                                {
                                    shouldDispatchCommandAntiReflux = false;
                                }

                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService);

                                if (battery.Inverter.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(battery.Inverter, _dbContext, ActionTypeCommand.PassiveMode);
                                }
                            await EnsureConnected();
                            await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);

                                ActionTypeCommand actionTypeCommandToUse = ActionTypeCommand.SellRemainingSunNoCharging;
                                string payload = "0";

                                var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                                        x.InverterId == battery.InverterId
                                                                                        && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                                                        && x.MQttMessageType == MQttMessageType.Regular
                                                                                        && x.Direction == Direction.Out);


                                var sellRemainingSunNoChargingAction = inverterTypeCompanyActions.FirstOrDefault(x => x.InverterId == battery.InverterId && x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);
                                var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId);
                                var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                                bool shouldDispatchCommand = true;

                                if (lastLoqRecord != null
                                    && lastLoqRecord.Topic == topicForMqtt
                                    && lastLoqRecord.Payload == payload
                                    && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                                    && (lastLoqRecord.ActionTypeCommand == actionTypeCommandToUse
                                    || lastLoqRecord.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging)
                                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                                {
                                    shouldDispatchCommand = false;
                                }

                                if (shouldDispatchCommand)
                                {
                                    foreach (var record in inverterTypeCompanyActions)
                                    {
                                        if (record.ActionTypeCommand == actionTypeCommandToUse)
                                        {
                                            record.ActionState = true;
                                        }
                                        else
                                        {
                                            if (record.ActionTypeCommand != ActionTypeCommand.AutoMode)
                                            {
                                                record.ActionState = false;
                                            }
                                        }

                                        _dbContext.Entry(record).State = EntityState.Modified;
                                    }

                                    _dbContext.Log.Add(new Log
                                    {
                                        Level = "Low",
                                        Message = $"Battery level adjustment sent with topic: {topicForMqtt}. With payload: 0"
                                    });

                                    await ProcessAntiRefluxWhenBatteryFullyCharged(battery);

                                    await _dbContext.SaveChangesAsync();
                                }
                            await EnsureConnected();
                            await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payload)
                                .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, topicForMqtt, payload, actionTypeCommandToUse, shouldDispatchCommand, MQttMessageType.Regular);

                                continue;
                            }

                            if (last5StateTransactions.FirstOrDefault().batterySOC >= battery.MaxLevel &&
                                (!actionStateFullDischarge.ActionState && !actionStateSelfUse.ActionState && !actionStateInverterSelfUse.ActionState
                                && !actionStateSellRemainingSunNoCharging.ActionState))
                            {
                                var registeredMqttInverter = battery.Inverter.RegisteredInverter.Name;
                                var currentDateTime = DateTime.Now;

                                var currentHourSpotPrice = await _dbContext.SpotPrice
                                 .FirstOrDefaultAsync(x => x.DateTime.Year == currentDateTime.Year
                                         && x.DateTime.Month == currentDateTime.Month
                                         && x.DateTime.Day == currentDateTime.Day
                                         && x.DateTime.Hour == currentDateTime.Hour
                                         && x.RegionId == company.RegionId);
                                //Hinnakontroll - kui hind on kehv siis antireflux 0, vastasel juhul antireflux 150
                                var actionToSendAntiReflux = currentHourSpotPrice.PriceNoTax > company.BrokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                                var mqttTopicForAntiReflux = inverterTypeCommands.FirstOrDefault(x =>
                                                                    x.InverterTypeId == battery.Inverter.InverterTypeId
                                                                    && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

                                string antiRefluxTopic = $"{registeredMqttInverter}{mqttTopicForAntiReflux!.MqttTopic}";

                                string antiRefluxPayload = actionToSendAntiReflux == ActionType.ThreePhaseAntiRefluxOn ? InverterHelper.SofarThreePhaseAntiRefluxPayload(battery.Inverter.MaxSalesPowerCapacity).ToString() : "0";

                                var mqttLogFromRedisAntiReflux = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                var lastLoqRecordAntiReflux = mqttLogFromRedisAntiReflux.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                        x.InverterId == battery.Inverter.Id
                                                        && x.MQttMessageType == MQttMessageType.ThreephaseAntireflux);

                                bool shouldDispatchCommandAntiReflux = true;

                                if (lastLoqRecordAntiReflux != null
                                    && lastLoqRecordAntiReflux.Topic == antiRefluxTopic
                                    && lastLoqRecordAntiReflux.Payload == antiRefluxPayload
                                    && lastLoqRecordAntiReflux.MQttMessageType == MQttMessageType.ThreephaseAntireflux
                                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecordAntiReflux.CreatedAt!, 120))
                                {
                                    shouldDispatchCommandAntiReflux = false;
                                }

                                BatteryButtonActions batteryButtonActions = new BatteryButtonActions(mqttLogger, redisCacheService);

                                if (battery.Inverter.UseInverterSelfUse)
                                {
                                    await batteryButtonActions.ProcessInverterModeControl(battery.Inverter, _dbContext, ActionTypeCommand.PassiveMode);
                                }
                            await EnsureConnected();
                            await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommandAntiReflux).PublishMessages(antiRefluxTopic, antiRefluxPayload)
                                    .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, antiRefluxTopic, antiRefluxPayload, ActionTypeCommand.ThreePhaseAntiReflux, shouldDispatchCommandAntiReflux, MQttMessageType.ThreephaseAntireflux);

                                ActionTypeCommand actionTypeCommandToUse = ActionTypeCommand.SellRemainingSunNoCharging;
                                
                             if (averageSolarProduction < averageInverterConsumption)
                            {
                                actionTypeCommandToUse = ActionTypeCommand.ChargeWithRemainingSun;
                            }

                                string payload = "0";

                                var mqttLogFromRedis = await redisCacheService.GetKeyValue<MqttMessageLog>(Constants.CacheKeys.MqttLogKey(battery.InverterId), Constants.MqttLogUnixOffset);

                                var lastLoqRecord = mqttLogFromRedis.OrderByDescending(x => x.CreatedAt).FirstOrDefault(x =>
                                                                                        x.InverterId == battery.InverterId
                                                                                        && x.MqttMessageOrigin != MqttMessageOrigin.AutoServiceInverter
                                                                                        && x.MQttMessageType == MQttMessageType.Regular
                                                                                        && x.Direction == Direction.Out);

                             
                                var inverterTypeCommand = inverterTypeCommands.FirstOrDefault(x => x.ActionType == ActionType.Discharge && x.InverterTypeId == battery.Inverter.InverterTypeId);
                                var topicForMqtt = $"{registeredMqttInverter}{inverterTypeCommand!.MqttTopic}";

                                bool shouldDispatchCommand = true;

                                if (lastLoqRecord != null
                                    && lastLoqRecord.Topic == topicForMqtt
                                    && lastLoqRecord.Payload == payload
                                    && lastLoqRecord.MQttMessageType == MQttMessageType.Regular
                                    && (lastLoqRecord.ActionTypeCommand == actionTypeCommandToUse
                                    || lastLoqRecord.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging)
                                    && DateTimeHelper.IsDateTimeWithinRequiredMinutesRange((DateTime)lastLoqRecord.CreatedAt!, 120))
                                {
                                    shouldDispatchCommand = false;
                                }

                                if (shouldDispatchCommand)
                                {
                                    foreach (var record in inverterTypeCompanyActions)
                                    {
                                        if (record.ActionTypeCommand == actionTypeCommandToUse)
                                        {
                                            record.ActionState = true;
                                        }
                                        else
                                        {
                                            if (record.ActionTypeCommand != ActionTypeCommand.AutoMode)
                                            {
                                                record.ActionState = false;
                                            }
                                        }

                                        _dbContext.Entry(record).State = EntityState.Modified;
                                    }

                                    _dbContext.Log.Add(new Log
                                    {
                                        Level = "Low",
                                        Message = $"Battery level adjustment sent with topic: {topicForMqtt}. With payload: 0"
                                    });

                                    await ProcessAntiRefluxWhenBatteryFullyCharged(battery);

                                    await _dbContext.SaveChangesAsync();
                                }
                            await EnsureConnected();
                            await new MqttClientService(mqttClient, mqttLogger, shouldDispatchCommand).PublishMessages(topicForMqtt, payload)
                                .Result.LogMessage(_dbContext, battery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, topicForMqtt, payload, actionTypeCommandToUse, shouldDispatchCommand, MQttMessageType.Regular);

                            }
                     
                    }
                }
            }

            // stopWatch.Stop();
            //  await Console.Out.WriteLineAsync($"ProcessMinMaxThresoldState took {stopWatch.ElapsedMilliseconds}");
        }
    }

        private async Task ProcessAntiRefluxWhenBatteryFullyCharged(InverterBattery inverterBattery)
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
                .Where(x => x.OffHour.InverterId == inverterBattery.Inverter.Id)
                .FirstOrDefaultAsync();

                if (inverterCompanyHour != null && inverterCompanyHour.OffHour.ActionType == ActionType.ThreePhaseAntiRefluxOff)
                {
                    var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == inverterBattery.Inverter.CompanyId);

                    if (company != null)
                    {
                        var actionToSend = inverterCompanyHour.SpotPrice.PriceNoTax > company!.BrokerPurchaseMargin ? ActionType.ThreePhaseAntiRefluxOn : ActionType.ThreePhaseAntiRefluxOff;

                        if (actionToSend == ActionType.ThreePhaseAntiRefluxOn)
                        {
                            var mqttTopicForAntiReflux = await _dbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                            x.InverterTypeId == inverterBattery.Inverter.InverterTypeId
                            && x.ActionType == ActionType.ThreePhaseAntiRefluxOn);

                            string antiRefluxTopic = $"{inverterBattery.Inverter.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                            var antiRefluxPayLoad = InverterHelper.SofarThreePhaseAntiRefluxPayload(inverterBattery.Inverter.MaxSalesPowerCapacity).ToString();
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, mqttLogger, true).PublishMessages(antiRefluxTopic, antiRefluxPayLoad)
                            .Result.LogMessage(_dbContext, inverterBattery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, antiRefluxTopic, antiRefluxPayLoad, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);
                            //Update database to antireflux off for this hour
                            var antiRefluxRecord = await _dbContext.InverterCompanyHours.FirstOrDefaultAsync(x =>
                            x.CompanyId == inverterBattery.Inverter.CompanyId
                            && x.SpotPriceId == inverterCompanyHour.SpotPrice.Id);

                            if (antiRefluxRecord != null)
                            {
                                antiRefluxRecord!.ActionType = ActionType.ThreePhaseAntiRefluxOn;
                            }



                            await _dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            var mqttTopicForAntiReflux = await _dbContext.InverterTypeCommands.FirstOrDefaultAsync(x =>
                                x.InverterTypeId == inverterBattery.Inverter.InverterTypeId
                                && x.ActionType == ActionType.ThreePhaseAntiRefluxOff);

                            string antiRefluxTopic = $"{inverterBattery.Inverter.RegisteredInverter.Name}{mqttTopicForAntiReflux!.MqttTopic}";
                        await EnsureConnected();
                        await new MqttClientService(mqttClient, mqttLogger, true).PublishMessages(antiRefluxTopic, "0")
                            .Result.LogMessage(_dbContext, inverterBattery.InverterId, Direction.Out, MqttMessageOrigin.BBSSMinMaxThresold, antiRefluxTopic, "0", ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);
                            //Update database to antireflux off for this hour
                            var antiRefluxRecord = await _dbContext.InverterCompanyHours.FirstOrDefaultAsync(x =>
                            x.CompanyId == inverterBattery.Inverter.CompanyId
                            && x.SpotPriceId == inverterCompanyHour.SpotPrice.Id);
                            if (antiRefluxRecord != null)
                            {
                                antiRefluxRecord!.ActionType = ActionType.ThreePhaseAntiRefluxOff;
                            }

                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
            }
        }
    }

