using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.Infrastructure;
using System.Diagnostics;
using SmartElectricityAPI.Helpers;
using System.Timers;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using SmartElectricityAPI.Enums;
using System.Text;
using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MQTTnet.Server;

namespace SmartElectricityAPI.BackgroundServices;

public class DeviceCompanyHourService : IHostedService, IDisposable
{
    private MySQLDBContext _dbContext;
    private IMqttClient _mqttClient;
    private readonly IMqttLogger _mqttLogger;
    private RedisCacheService redisCacheService;
    private MqttFactory _mqttFactory;
    private MqttClientOptions _mqttClientOptions;

    public DeviceCompanyHourService(MySQLDBContext dbContext, IMqttLogger mqttLogger)
    {
        _dbContext = dbContext;
        _mqttLogger = mqttLogger;
        _mqttFactory = new MqttFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();
        _mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(CredentialHelpers.MqttServerBasicSSLSettings().Server, CredentialHelpers.MqttServerBasicSSLSettings().Port)
            .WithTlsOptions(o =>
            { o.Build().UseTls = true;
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
            .WithCredentials(CredentialHelpers.MqttServerBasicSSLSettings().Username, CredentialHelpers.MqttServerBasicSSLSettings().Password).Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        redisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());

       if (!Debugger.IsAttached)
       {
           //  await RefreshCustomersData();

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
        SwitchCommandsProcessor switchCommandsProcessor = new SwitchCommandsProcessor(redisCacheService, _mqttLogger, _mqttClient, _dbContext, DateTime.Now);
        await switchCommandsProcessor.ProcessSwitchCommands();
    }

    public async void TimerElapsedEveryHour(object sender, ElapsedEventArgs e) // (object sender, ElapsedEventArgs e)
    {
        SwitchCommandsProcessor switchCommandsProcessor = new SwitchCommandsProcessor(redisCacheService, _mqttLogger, _mqttClient, _dbContext, e.SignalTime);
        await switchCommandsProcessor.ProcessSwitchCommands();

        var currentTime = DateTime.Now;
        var nextHour = currentTime.AddHours(1).AddMinutes(-currentTime.Minute).AddSeconds(-currentTime.Second);
        //   var nextHour = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second);
        var timeUntilNextHour = nextHour - currentTime;

        // Reset the timer interval
        var timer = (System.Timers.Timer)sender;
        timer.Interval = timeUntilNextHour.TotalMilliseconds;

    }

    private async Task EstablishMqttConnection()
    {
        try
        {
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
        
        this.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _mqttClient.DisconnectAsync();
    }
}
