using Microsoft.EntityFrameworkCore;
using Polly;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Engine;
using SmartElectricityAPI.Migrations;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.ThirdParty.SpotHintaFi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Timers;

namespace SmartElectricityAPI;

public class SpotPriceEngine : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MySQLDBContext _dbContext;

    public SpotPriceEngine(IServiceProvider serviceProvider, MySQLDBContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;

    }

    private List<SpotPrice> spotHintaFi;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryPolicy = Policy
        .Handle<Exception>() 
        .WaitAndRetryAsync(5, retryAttempt =>
        TimeSpan.FromSeconds(120),
        (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} encountered an exception: {exception.Message}. Delaying for {timeSpan}. Stack trace:{exception.StackTrace}. Source: {exception.Source} ");
        });

        if (!Debugger.IsAttached)
        {
            while (true)
            {
            await retryPolicy.ExecuteAsync(async () =>
            {
                ProcessCustomersWithNordPoolPrice();

               // ProcessFixedPriceCustomers();

                await Task.Delay(TimeSpan.FromMinutes(ThirdParty.SpotHintaFi.Constants.CheckNewPricesEveryXMinute));
            });
         }
         }
    }



    private async void ProcessCustomersWithNordPoolPrice()
    {
        bool pricesGenerated = await ProcessDayForwardPrices();

        if (pricesGenerated)
        {
            //TODO: Needs refactoring!


                var companies = await _dbContext.Company.Where(x => x.UseFixedPrices == false).ToListAsync();
                var solarPanelCapacity = await _dbContext.SolarPanelCapacity.ToListAsync();
                var countrySolarCapacity = await _dbContext.CountrySolarCapacity.ToListAsync();
                var inverters = await _dbContext.Inverter.ToListAsync();
                var regions = await _dbContext.Region.ToListAsync();
                var inverterBatteries = await _dbContext.InverterBattery.ToListAsync();
                var WeatherApiComService = new WeatherApiComService(_serviceProvider.GetRequiredService<IHttpClientFactory>(), _serviceProvider.GetRequiredService<IConfiguration>());

                foreach (var company in companies)
                {
                    var dateTomorrow = DateTime.Now.AddDays(1);
                    var dateToday = DateTime.Now.AddHours(1);

                    var selectedInverter = inverters.FirstOrDefault(x => x.CompanyId == company.Id);
                   
                    var devicePriceCalculator = new DevicePriceCalculatorService(company, dateTomorrow, DateTime.Now.AddDays(2));
                    await devicePriceCalculator.ProcessOperations();   
                

                    if (selectedInverter != null)
                    {
                        var inverterPriceCalculator = new InverterPriceCalculatorService(company, dateTomorrow, DateTime.Now.AddDays(2));
                        await inverterPriceCalculator.ProcessOperations();
                    }
                               

                    if (selectedInverter != null && selectedInverter.CalculationFormula == Enums.CalculationFormula.Winter)
                    {
                        if (selectedInverter != null && selectedInverter.UseWeatherForecast)
                        {
                            var countrySolarCapacityForCompany = countrySolarCapacity.Where(x => x.CountryId == company.CountryId).ToList();
                            WeatherProcessingService weatherProcessingService = new WeatherProcessingService(company, selectedInverter, DateOnly.FromDateTime(dateTomorrow), WeatherApiComService, solarPanelCapacity, countrySolarCapacityForCompany);
                            await weatherProcessingService.ProcessCurrenDateWeatherData();
                            await weatherProcessingService.ProcessWeatherData();
                        }

                        var batteryPriceCalculator = new BatteryPriceCalculatorService(company, dateTomorrow, DateTime.Now.AddDays(2), WeatherApiComService);
                        await batteryPriceCalculator.ProcessOperations();
                    }

                    if (selectedInverter != null && (selectedInverter.CalculationFormula == Enums.CalculationFormula.Summer
                    || selectedInverter.CalculationFormula == Enums.CalculationFormula.SummerWithoutConsumeMax
                    || selectedInverter.CalculationFormula == Enums.CalculationFormula.SummerMedium))
                    {
                        // try
                        //  {
                        var region = regions.FirstOrDefault(x => x.Id == company.RegionId);
                        var inverterBattery = inverterBatteries.FirstOrDefault(x => x.InverterId == selectedInverter.Id);

                        if (inverterBattery != null)
                        {
                            BatteryHoursCalculatorSummer batteryHoursCalculatorSummer = new BatteryHoursCalculatorSummer(new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, dateToday.Hour, 0, 0), new DateTime(dateTomorrow.Year, dateTomorrow.Month, dateTomorrow.Day, 23, 0, 0), region, inverterBattery, company.Id, (int)selectedInverter.RegisteredInverterId, WeatherApiComService);
                            await batteryHoursCalculatorSummer.PopulateData();
                        }
                        // }
                        /*
                        catch (Exception ex)
                        {
                            var errorLog = new ErrorLog { Message = $"{ex.Message}, stacktrace: {ex.InnerException.TargetSite.Name}" };
                            _dbContext.ErrorLog.Add(errorLog);
                            await _dbContext.SaveChangesAsync();
                        }
                        */

                    }

                    if (selectedInverter != null && selectedInverter.CalculationFormula == Enums.CalculationFormula.SelfUse)
                    {
                    //    try
                      //  {
                            if (selectedInverter != null && selectedInverter.UseWeatherForecast)
                            {
                                var countrySolarCapacityForCompany = countrySolarCapacity.Where(x => x.CountryId == company.CountryId).ToList();
                                WeatherProcessingService weatherProcessingService = new WeatherProcessingService(company, selectedInverter, DateOnly.FromDateTime(dateTomorrow), WeatherApiComService, solarPanelCapacity, countrySolarCapacityForCompany);
                                await weatherProcessingService.ProcessCurrenDateWeatherData();
                                await weatherProcessingService.ProcessWeatherData();
                            }

                            var region = regions.FirstOrDefault(x => x.Id == company.RegionId);
                            var inverterBattery = inverterBatteries.FirstOrDefault(x => x.InverterId == selectedInverter.Id);

                        if (inverterBattery != null && region != null && selectedInverter != null && selectedInverter.RegisteredInverterId != null)
                        {
                            BatteryHoursSelfUseCalculator batteryHoursSelfUseCalculator = new BatteryHoursSelfUseCalculator(new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, dateToday.Hour, 0, 0), new DateTime(dateTomorrow.Year, dateTomorrow.Month, dateTomorrow.Day, 23, 0, 0), region, inverterBattery, company.Id, (int)selectedInverter.RegisteredInverterId);
                            await batteryHoursSelfUseCalculator.PopulateData();
                        }
                            /*
                        }

                        catch (Exception ex)
                        {
                            var errorLog = new ErrorLog { Message = $"{ex.Message}, stacktrace: {ex.InnerException.TargetSite.Name}" };
                            _dbContext.ErrorLog.Add(errorLog);
                            await _dbContext.SaveChangesAsync();
                        }
                    */

                    }

                    await Task.Delay(1000);
                }
           
        }       
    }

    private void RemoveDupeSpotPriceHourRecord()
    {
        spotHintaFi.Sort((p1, p2) => p1.DateTime.CompareTo(p2.DateTime));

        List<SpotPrice> toRemove = new List<SpotPrice>();
        SpotPrice previous = null;
        double priceSum = 0;
        int count = 0;
        int maxRank = int.MinValue;

        foreach (SpotPrice current in spotHintaFi)
        {
            if (previous == null || previous.DateTime.Hour != current.DateTime.Hour)
            {
                if (previous != null)
                {
                    previous.Update(priceSum / count, maxRank);
                }
                priceSum = current.PriceNoTax;
                count = 1;
                maxRank = current.Rank;
                previous = current;
            }
            else
            {
                priceSum += current.PriceNoTax;
                count++;
                if (current.Rank > maxRank)
                {
                    maxRank = current.Rank;
                }
                toRemove.Add(current);
            }
        }

        if (previous != null)
        {
            previous.Update(priceSum / count, maxRank);
        }

        foreach (SpotPrice sp in toRemove)
        {
            spotHintaFi.Remove(sp);
        }
    }

    public async Task<bool> ProcessDayForwardPrices()
    {
        bool hasNewPricesCalculated = false;
        // Define the start and end times of your range
        TimeSpan startTime = new TimeSpan(14, 0, 0); // 14:00
        TimeSpan endTime = new TimeSpan(23, 59, 59);   // 16:00

        // Get the current time
        TimeSpan currentTime = DateTime.Now.TimeOfDay;

        if (currentTime >= startTime && currentTime <= endTime)
        {
            using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
            {              

                var activeRegions = await _dbContext.Company.Select(x => x.RegionId).Distinct().ToListAsync();
                var regionsInUse = await _dbContext.Region.Where(x => activeRegions.Contains(x.Id)).ToListAsync();

                var today = DateOnly.FromDateTime(DateTime.Now);
                var regionIdsInUse = regionsInUse.Select(r => r.Id).ToList();
                bool pricesExist = true;

                var hasPrices = new List<int?>();

                var tomorrowPrices = await _dbContext.SpotPrice.Where(x => x.Date > today).ToListAsync();


                foreach (var region in regionIdsInUse)
                {
                    var pricesForRegion = tomorrowPrices.Count(x => x.RegionId == region);

                    if (pricesForRegion >= 15)
                    {
                        hasPrices.Add(region);
                    }
                }


                /*
                var hasPrices = await _dbContext.SpotPrice
                    .Where(x => x.Date > today && regionIdsInUse.Contains(x.RegionId))
                    .Select(x => x.RegionId)
                    .Distinct()
                    .ToListAsync();
                */

                foreach (var regionId in regionIdsInUse)
                {
                    var hasPrice = hasPrices.Contains(regionId);
                    if (!hasPrice)
                    {
                        pricesExist = false;
                        break;
                    }
                }

                if (!pricesExist)
                {
                    bool companyHourlyFeesTransactionsGenerated = false;
                    NordPoolPriceServiceV2 nordPoolPriceService = new NordPoolPriceServiceV2();
                    DateOnly dateTomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

                    foreach (var region in regionsInUse)
                    {
                        try
                        {
                            // spotHintaFi = Api.GetDayForwardPrices(region.Abbreviation).Result!;
                            spotHintaFi = nordPoolPriceService.GetPrices(region.Abbreviation, dateTomorrow).Result!;
                        }
                        catch (Exception ex)
                        {
                            var errorLog = new ErrorLog { Message = ex.Message };
                            _dbContext.ErrorLog.Add(errorLog);
                            await _dbContext.SaveChangesAsync();
                            return false;
                        }


                        if (spotHintaFi != null)
                        {
                            Console.WriteLine($"Result on dayforward prices: {spotHintaFi.Count}");
                            foreach (var SpotPrice in spotHintaFi)
                            {
                                SpotPrice.RegionId = region.Id;
                            }

                            RemoveDupeSpotPriceHourRecord();

                        }
                        else
                        {
                            Console.WriteLine("No next day prices!");
                        }


                        if (spotHintaFi != null && spotHintaFi.Count > 0 && _dbContext.SpotPrice.Count(x => x.RegionId == region.Id && x.Date == DateOnly.FromDateTime(DateTime.Now.AddDays(1))) <= 1)
                        {
                            List<SpotPrice> Last48Prices = await _dbContext.SpotPrice.Where(x => x.RegionId == region.Id).AsNoTracking().OrderByDescending(x => x.DateTime).Take(48).ToListAsync();
                            bool hasNewPrices = false;
                            foreach (var item in spotHintaFi)
                            {
                                if (Last48Prices.Count == 0 || !Last48Prices.Any(x => x.DateTime == item.DateTime))
                                {
                                    _dbContext.SpotPrice.Add(item);
                                    await _dbContext.SaveChangesAsync();
                                    hasNewPrices = true;
                                }
                            }

                            if (hasNewPrices)
                            {
                                if (!companyHourlyFeesTransactionsGenerated)
                                {
                                    CompanyPriceRates companyPriceRates = new CompanyPriceRates(_dbContext);
                                    await companyPriceRates.GenerateTransactionsForDate(spotHintaFi.FirstOrDefault()!.Date);

                                    companyHourlyFeesTransactionsGenerated = true;
                                }

                                hasNewPricesCalculated = true;
                            }
                        }
                    }

                    if (spotHintaFi != null && spotHintaFi.Count > 0)
                    {
                        hasNewPricesCalculated = true;
                    }

                }

                await _dbContext.DisposeAsync();
            }          
        }

        return hasNewPricesCalculated;
    }

}
