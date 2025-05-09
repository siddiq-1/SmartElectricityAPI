using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using System.Diagnostics;

namespace SmartElectricityAPI.BackgroundServices;

public class SystemService : BackgroundService
{

    private readonly InverterHourService _brokerEngine;
    public SystemService(InverterHourService brokerEngine)
    {
        _brokerEngine = brokerEngine;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
          if (!Debugger.IsAttached)
          {
            while (!stoppingToken.IsCancellationRequested)
            {


                await Task.Delay(TimeSpan.FromMinutes(Constants.RestartSystemIntervalForBrokerService), stoppingToken);

                var currentDate = DateTime.Now;

              //  if (currentDate.Hour )

                /*
                await _sofarStateBufferService.StopAsync(new CancellationToken());
                await Task.Delay(5000);
                await _sofarStateBufferService.StartAsync(new CancellationToken());
                */

            }
          }
    }
}
