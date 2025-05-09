using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Server;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.DBQueries;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Models;
using System.Text;
using System.Timers;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.Infrastructure;
using System.Diagnostics;

namespace SmartElectricityAPI.BackgroundServices;

public class InverterHourService : IHostedService, IDisposable
{
    private MySQLDBContext _dbContext;
    private IMqttClient _mqttClient;
    private readonly IMqttLogger _mqttLogger;
    private RedisCacheService redisCacheService;
    private MqttFactory _mqttFactory;
    private MqttClientOptions _mqttClientOptions;
    private List<RegisteredInverterTopicDto> registeredInverterTopics = new();
    private List<InverterTypeCommands> InverterTypeCommands = new();
    private List<Inverter> InverterWithRegisteredInverter = new();

    public InverterHourService(MySQLDBContext dbContext, IMqttLogger mqttLogger)
    {
        _dbContext = dbContext;
        _mqttLogger = mqttLogger;
        _mqttFactory = new MqttFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();
        _mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(CredentialHelpers.MqttServerBasicSettings().Server, CredentialHelpers.MqttServerBasicSettings().Port)
            .WithCredentials(CredentialHelpers.MqttServerBasicSettings().Username, CredentialHelpers.MqttServerBasicSettings().Password)
            .WithCleanSession()
            .Build();
    }
    public async Task RefreshCustomersData()
    {
        using (var mySQLDBContextLocal = await new DatabaseService().CreateDbContextAsync())
        {
            InverterQry inverterQry = new InverterQry(mySQLDBContextLocal);
            registeredInverterTopics = await inverterQry.GetRegisteredInventerTopics();
            InverterWithRegisteredInverter = await inverterQry.GetInvertersWithRegisteredInverter();
            InverterTypeCommands = await mySQLDBContextLocal.InverterTypeCommands.ToListAsync();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());

        if (!Debugger.IsAttached)
        {
            await RefreshCustomersData();

            await EstablishMqttConnection();

            await DoIntialProcessing();

            var currentTime = DateTime.Now;
            var nextHour = currentTime.AddHours(1).AddMinutes(-currentTime.Minute).AddSeconds(-currentTime.Second);
            // var nextHour = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second);
            var timeUntilNextHour = nextHour - currentTime;
            var timer = new System.Timers.Timer(timeUntilNextHour.TotalMilliseconds);

            timer.Elapsed += TimerElapsedEveryHour;
            timer.Start();
        }
    }

    private async Task DoIntialProcessing()
    {
        BatteryCommandsProcessor batteryCommandsProcessor = new BatteryCommandsProcessor(redisCacheService, _mqttLogger, _mqttClient, _dbContext, InverterTypeCommands, DateTime.Now);
        await batteryCommandsProcessor.ProcessBatteryCommands();
    }

    public async Task MqttReconnect()
    {
        if (_mqttClient.IsConnected)
        {
            try
            {
                await _mqttClient.DisconnectAsync();

                await EstablishMqttConnection();
            }
            catch (Exception exception)
            {
                await Console.Out.WriteLineAsync(exception.ToString());
            }
        }
    }

    public void Dispose() { }

    private async Task EstablishMqttConnection()
    {
        try
        {
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                string payload = "";
                try
                {
                    payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);


                    var fuseBoxMessageProcessor = new FuseBoxMessageProcessor();
                    fuseBoxMessageProcessor.Process(payload, e.ApplicationMessage.Topic, _mqttClient, _mqttLogger, redisCacheService, InverterWithRegisteredInverter);




                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing MQTT message: {ex.Message}");
                    Console.WriteLine($"For data: {payload}");
                }
            };


            _ = Task.Run(
                async () =>
                {
                    // User proper cancellation and no while(true).
                    while (true)
                    {
                        try
                        {
                            // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                            if (!await _mqttClient.TryPingAsync())
                            {
                                var connectResponse = await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
                                await Console.Out.WriteLineAsync(connectResponse.ResultCode.ToString());

                                var topicFilter = new List<MqttTopicFilter>();
                                /*
                                foreach (var item in registeredInverterTopics)
                                {
                                    topicFilter.Add(new MqttTopicFilter { Topic = item.RegisteredInverterAndTopic });
                                }

                                foreach (var sw in _dbContext.Sensor.Where(x => !string.IsNullOrWhiteSpace(x.Topic)).ToList())
                                {
                                    topicFilter.Add(new MqttTopicFilter { Topic = sw.Topic });
                                }

                                foreach (var sw in _dbContext.Switch.Where(x => !string.IsNullOrWhiteSpace(x.Topic)).ToList())
                                {
                                    topicFilter.Add(new MqttTopicFilter { Topic = sw.Topic });
                                }
                                */

                                // Loop through FuseBoxChannel enum and add them to topicFilter as string
                                foreach (FuseBoxChannels channel in Enum.GetValues(typeof(FuseBoxChannels)))
                                {
                                    topicFilter.Add(new MqttTopicFilter { Topic = channel.ToString() });
                                }

                                Console.WriteLine("subscribing iteration");

                                MqttClientSubscribeOptions objSubOptions = new MqttClientSubscribeOptions();
                                objSubOptions.TopicFilters = topicFilter;

                                var res = await _mqttClient.SubscribeAsync(objSubOptions, CancellationToken.None);
                            }
                        }
                        catch
                        {
                            // Handle the exception properly (logging etc.).
                        }
                        finally
                        {
                            // Check the connection state every 5 seconds and perform a reconnect if required.
                            await Task.Delay(TimeSpan.FromSeconds(30));
                        }
                    }
                });

        }
        catch (MqttCommunicationException ex)
        {
            Console.WriteLine(ex.Message.ToString());
        }
        catch (Exception generalException)
        {
            // Catch other unexpected exceptions to prevent the application from crashing
            Console.WriteLine(generalException.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _mqttClient.DisconnectAsync();
        Dispose();
        return Task.CompletedTask;
    }

    public async void TimerElapsedEveryHour(object sender, ElapsedEventArgs e) // (object sender, ElapsedEventArgs e)
    {
        BatteryCommandsProcessor batteryCommandsProcessor = new BatteryCommandsProcessor(redisCacheService, _mqttLogger, _mqttClient, _dbContext, InverterTypeCommands, e.SignalTime);
        await batteryCommandsProcessor.ProcessBatteryCommands();

        await Task.Delay(10000);


        await ProcessInverterCommands(sender, e);

        var currentTime = DateTime.Now;
        var nextHour = currentTime.AddHours(1).AddMinutes(-currentTime.Minute).AddSeconds(-currentTime.Second);
        //   var nextHour = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second);
        var timeUntilNextHour = nextHour - currentTime;

        // Reset the timer interval
        var timer = (System.Timers.Timer)sender;
        timer.Interval = timeUntilNextHour.TotalMilliseconds;

    }

    private async Task ProcessInverterCommands(object sender, ElapsedEventArgs e)
    {
        using (var mySQLDBContextLocal = await new DatabaseService().CreateDbContextAsync())
        {
            var entitiesList = mySQLDBContextLocal.ChangeTracker.Entries().ToList();

            foreach (var entity in entitiesList)
            {
                entity.Reload();
            }

            var distinctRegions = mySQLDBContextLocal.Company.Select(x => x.RegionId).Distinct().ToList();
            var usedRegions = mySQLDBContextLocal.Region.Where(x => distinctRegions.Contains(x.Id)).ToList();

            foreach (var region in usedRegions)
            {
                DateTime singalTime = e.SignalTime.AddHours(-region.OffsetHoursFromEstonianTime);

                var joinedData = await mySQLDBContextLocal.SpotPrice
                    .Where(x => x.DateTime.Year == singalTime.Year
                                && x.DateTime.Month == singalTime.Month
                                && x.DateTime.Day == singalTime.Day
                                && x.DateTime.Hour == singalTime.Hour
                                && x.RegionId == region.Id)
                    .Join(
                        mySQLDBContextLocal.InverterCompanyHours,
                        spotPrice => spotPrice.Id,
                        offHour => offHour.SpotPriceId,
                        (spotPrice, offHour) => new
                        {
                            SpotPrice = spotPrice,
                            OffHour = offHour
                        })
                    .Join(
                        mySQLDBContextLocal.Inverter,
                        joinedData => joinedData.OffHour.InverterId,
                        inverter => inverter.Id,
                        (joinedData, inverter) => new
                        {
                            joinedData.SpotPrice,
                            joinedData.OffHour,
                            Inverter = inverter
                        })
                    .Where(joinedData =>
                        !mySQLDBContextLocal.InverterBattery
                            .Any(inverterBattery =>
                                inverterBattery.InverterId == joinedData.Inverter.Id &&
                                inverterBattery.Enabled))
                    .ToListAsync();

                foreach (var item in joinedData)
                {
                    var listOfActions = await mySQLDBContextLocal.InverterTypeCommands.Where(
                        x => x.ActionType == item.OffHour.ActionType)
                        .ToListAsync();

                    List<Inverter> listOfInverters = mySQLDBContextLocal.Inverter.Include(i => i.RegisteredInverter).ToList();

                    foreach (var singleAction in listOfActions)
                    {
                        var inverterIdWithName = listOfInverters.Where(x => x.Id == item.OffHour.InverterId && x.RegisteredInverterId != null);
                        if (inverterIdWithName.Any())
                        {
                            string topic = $"{inverterIdWithName.FirstOrDefault()!.RegisteredInverter.Name}{singleAction.MqttTopic}";
                            string payload = singleAction.ActionType == ActionType.ThreePhaseAntiRefluxOn
                                ? InverterHelper.SofarThreePhaseAntiRefluxPayload(listOfInverters.FirstOrDefault(x => x.Id == item.OffHour.InverterId).MaxSalesPowerCapacity).ToString()
                                : "0";

                            await new MqttClientService(_mqttClient, _mqttLogger, true).PublishMessages(topic, payload)
                            .Result.LogMessage(mySQLDBContextLocal, inverterIdWithName.FirstOrDefault()!.Id, Direction.Out, MqttMessageOrigin.AutoServiceInverter, topic, payload, ActionTypeCommand.ThreePhaseAntiReflux, true, MQttMessageType.ThreephaseAntireflux);
                        }
                    }
                }
            }
        }
    }
}
