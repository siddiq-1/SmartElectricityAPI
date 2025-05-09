using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Engine.Helpers;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models;

using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Services;


namespace SmartElectricityAPI.Engine;

public class BatteryHoursCalculator
{
    private DateTime DateTimeMin { get; set; }
    private DateTime DateTimeMax { get; set; }
    private Region Region { get; set; }
    private InverterBattery InverterBattery { get; set; }
    private MySQLDBContext _dbContext;
    private IMapper Mapper { get; set; }

    private int companyId { get; set; }
    private int registeredInverterId { get; set; }

    private WeatherApiComService WeatherApiComService { get; set; }
    private List<BatteryHoursSummer> BatteryHoursSummer { get; set; }

    public BatteryHoursCalculator(DateTime dateTimeMin, DateTime dateTimeMax, Region region, InverterBattery inverterBattery, int companyId, int registeredInverterId, WeatherApiComService weatherApiComService, MySQLDBContext dbContext)
    {


        Mapper = AutoMapperConfig.InitializeAutoMapper();

        DateTimeMin = dateTimeMin;
        DateTimeMax = dateTimeMax;
        Region = region;
        InverterBattery = inverterBattery;
        this.companyId = companyId;
        this.registeredInverterId = registeredInverterId;
        this.WeatherApiComService = weatherApiComService;
    }

    public async Task<bool> GetAverageConsumptionForDailyHours(int companyId, int registeredInverterId, int numberOfDaysBack, DayOfWeek dayOfWeek)
    {
      //  _dbContext.Database.SetCommandTimeout(300);
        var inverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.RegisteredInverterId == registeredInverterId);

        if (inverter == null)
        {
            return false;
        }


        if (inverter.UseFixedAvgHourlyWatts)
        {
            return true;
        }

        var dateCalculated = DateOnly.FromDateTime(DateTime.Now);
        DateTime fourWeeksAgo = DateTime.Now.AddDays(-numberOfDaysBack);

        List<TimeSpan> expectedTimeSpans = Enumerable.Range(0, 24)
         .Select(hour => TimeSpan.FromHours(hour))
         .ToList();
   
          var subQuery = await _dbContext.SofarStateHourlyTemp
          .Where(x =>
              x.Date >= DateOnly.FromDateTime(fourWeeksAgo)
              && x.Date < DateOnly.FromDateTime(DateTime.Now.AddDays(-1))
              && x.CompanyId == companyId
              && x.RegisteredInverterId == registeredInverterId)
          .GroupBy(x => new { x.Date, x.TimeHour })
          .Select(g => new
          {
              Date = (DateOnly)g.Key.Date!,
              g.Key.TimeHour,
              HourlyConsumption = g.Average(x => x.consumption),
          })
          .ToListAsync(); 


        var result = subQuery
        .GroupBy(x => new { WeekDay = x.Date.DayOfWeek, x.TimeHour })
        .Select(g => new InverterHoursAvgConsumption
        {
            DayOfWeek = g.Key.WeekDay,
            TimeHour = g.Key.TimeHour,
            AvgHourlyConsumption = (int)g.Average(x => x.HourlyConsumption),
            CompanyId = companyId,
            RegisteredInverterId = registeredInverterId,
            DateCalculated = dateCalculated,
            InverterId = inverter.Id

        })
        .Where(x => x.DayOfWeek == dayOfWeek)
        .OrderBy(x => x.DayOfWeek)
        .ThenBy(x => x.TimeHour)
        .ToList();


        if (result.Count > 0)
        {
            var averageOfDay = result.Average(x => x.AvgHourlyConsumption);

            foreach (var mandatoryTime in expectedTimeSpans)
            {
                if (!result.Any(x => x.TimeHour == mandatoryTime))
                {
                    result.Add(new InverterHoursAvgConsumption
                    {
                        DayOfWeek = result.FirstOrDefault().DayOfWeek,
                        TimeHour = mandatoryTime,
                        AvgHourlyConsumption = (int)averageOfDay,
                        CompanyId = companyId,
                        RegisteredInverterId = registeredInverterId,
                        DateCalculated = dateCalculated,
                        InverterId = inverter.Id
                    });
                }
            }
        }

        if (result.Count < 24)
        {
            return false;
        }

        foreach (var item in result)
        {
            var exists = await _dbContext.InverterHoursAvgConsumption.AnyAsync(e =>
            e.InverterId == item.InverterId &&
            e.RegisteredInverterId == item.RegisteredInverterId &&
            e.DateCalculated == item.DateCalculated &&
            e.DayOfWeek == item.DayOfWeek &&
            e.TimeHour == item.TimeHour);

            if (!exists)
            {
                _dbContext.InverterHoursAvgConsumption.Add(item);
            }

        }

        await _dbContext.SaveChangesAsync();

        return true;

    }

    private async Task<List<InverterHoursAvgConsumption>> GetAverageConsumptionForWeekDays()
    {
        HashSet<DayOfWeek> daysOfWeekInPeriod = GetDaysOfWeekInPeriod();

        var inverterHoursAvgConsumption = await _dbContext.InverterHoursAvgConsumption.Where(x => daysOfWeekInPeriod.Contains(x.DayOfWeek) && x.InverterId == InverterBattery.InverterId).ToListAsync();

        inverterHoursAvgConsumption = inverterHoursAvgConsumption
                                       .GroupBy(x => new { x.DateCalculated, x.TimeHour, x.DayOfWeek })
                                        .Select(g => g.OrderByDescending(x => x.DateCalculated).FirstOrDefault())
                                        .ToList()!;

        return inverterHoursAvgConsumption;
    }

    private HashSet<DayOfWeek> GetDaysOfWeekInPeriod()
    {
        HashSet<DayOfWeek> daysOfWeekInPeriod = new HashSet<DayOfWeek>();

        for (DateTime date = DateTimeMin; date <= DateTimeMax; date = date.AddDays(1))
        {
            daysOfWeekInPeriod.Add(date.DayOfWeek);
        }

        return daysOfWeekInPeriod;
    }

    private async Task<List<BatteryControlHours>> ProcessSelfUseCalculation(List<SpotPrice> spotPrices)
    {
        var selfUseCalculation = await Calculate(CalculationType.SelfUse, spotPrices);

        foreach (var item in selfUseCalculation)
        {
            item.MinChargingPowerWhOriginalDiff = item.MinChargingPowerWh != null && item.MinChargingPowerWhOriginal != null ? item.MinChargingPowerWhOriginal - item.MinChargingPowerWh : null;
            item.MaxAvgHourlyConsumptionOriginalDiff = item.MaxAvgHourlyConsumption != null && item.MaxAvgHourlyConsumptionOriginal != null ? item.MaxAvgHourlyConsumptionOriginal - item.MaxAvgHourlyConsumption : null;
            item.AmountCharged = item.MaxAvgHourlyConsumptionOriginal; // > item.MaxAvgHourlyConsumptionOriginalDiff ? item.MaxAvgHourlyConsumptionOriginalDiff : item.MinChargingPowerWhOriginalDiff;
            item.LineProfit = item.MaxMinPriceDifference != null ? item.MaxMinPriceDifference * item.AmountCharged : 0;
        }

        foreach (var item in selfUseCalculation)
        {
            item.HourProfit = selfUseCalculation.Where(x => x.MaxPriceHour == item.MaxPriceHour && item.MaxMinPriceDifference != null).Sum(x => x.MaxMinPriceDifference * x.AmountCharged);
        }


        selfUseCalculation = selfUseCalculation.OrderBy(x => x.MaxPriceHour).ToList();

        selfUseCalculation = selfUseCalculation.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse).OrderByDescending(x => x.MaxMinPriceDifference).ToList();

        var maxUsableBatteryWatts = (InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)) * (InverterBattery.AdditionalTimeForBatteryChargingPercentage + 1);

        foreach (var item in selfUseCalculation)
        {
            if (maxUsableBatteryWatts <= 0)
            {
                return selfUseCalculation;
            }
            if (maxUsableBatteryWatts - ((double)item.AmountCharged * (InverterBattery.AdditionalTimeForBatteryChargingPercentage + 1)) > 0)
            {
                item.UsableWatts = item.AmountCharged;
                item.UsableWattsProfit = item.UsableWatts * item.MaxMinPriceDifference;
                maxUsableBatteryWatts = maxUsableBatteryWatts - ((double)item.AmountCharged * (InverterBattery.AdditionalTimeForBatteryChargingPercentage + 1));
            }
            else
            {
                item.UsableWatts = (int)maxUsableBatteryWatts;
                item.UsableWattsProfit = item.UsableWatts * item.MaxMinPriceDifference;
                maxUsableBatteryWatts = 0;
            }
        }

        return selfUseCalculation;

    }

    private async Task<List<BatteryControlHours>> ProcessSalesCalculation(List<SpotPrice> spotPrices, int maxUsableBatteryWatts)
    {
        var salesCalculation = await Calculate(CalculationType.Sales, spotPrices);

        foreach (var item in salesCalculation)
        {
            item.MinChargingPowerWhOriginalDiff = item.MinChargingPowerWh != null && item.MinChargingPowerWhOriginal != null ? item.MinChargingPowerWhOriginal - item.MinChargingPowerWh : null;
            item.MaxAvgHourlyConsumptionOriginalDiff = item.MaxAvgHourlyConsumption != null && item.MaxAvgHourlyConsumptionOriginal != null ? item.MaxAvgHourlyConsumptionOriginal - item.MaxAvgHourlyConsumption : null;
        }

        salesCalculation = salesCalculation.Where(x => x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).OrderByDescending(x => x.MaxMinPriceDifference).ToList();

        foreach (var item in salesCalculation)
        {

            if (maxUsableBatteryWatts - (InverterBattery.ChargingPowerFromGridKWh * 1000) > 0)
            {
                item.UsableWatts = (int)InverterBattery.ChargingPowerFromGridKWh * 1000;
                item.UsableWattsProfit = item.UsableWatts * item.MaxMinPriceDifference;
                maxUsableBatteryWatts = maxUsableBatteryWatts - (int)InverterBattery.ChargingPowerFromGridKWh * 1000;
            }
            else
            {
                item.UsableWatts = (int)maxUsableBatteryWatts;
                item.UsableWattsProfit = item.UsableWatts * item.MaxMinPriceDifference;
                maxUsableBatteryWatts = 0;
            }
        }

        return salesCalculation;
    }

    private async Task ClearBatteryControlHoursScheduleForPeriod()
    {
        var scheduleRecordToDelete = await _dbContext.BatteryControlHours.Where(x => x.SpotPriceMax.DateTime >= DateTimeMin && x.SpotPriceMax.DateTime <= DateTimeMax && x.InverterBatteryId == InverterBattery.Id).ToListAsync();

        if (scheduleRecordToDelete.Count > 0)
        {
            _dbContext.BatteryControlHours.RemoveRange(scheduleRecordToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<BatteryWave>> GenerateWaves()
    {
        var company = await _dbContext.Inverter.Where(x => x.Id == InverterBattery.InverterId).Include(x => x.Company).Select(x => x.Company).FirstOrDefaultAsync();
        var spotPriceRange = await _dbContext.SpotPrice.Where(x => x.DateTime >= DateTimeMin && x.DateTime <= DateTimeMax && x.RegionId == Region.Id).OrderByDescending(x => x.DateTime).ToListAsync();

      //  var inverterCompanyHours = _dbContext.InverterCompanyHours.Where(x => x.CompanyId == companyId && x.InverterId == InverterBattery.InverterId
        //&& x.SpotPrice.DateTime >= DateTimeMin && x.SpotPrice.DateTime <= DateTimeMax).ToList();    

        var expectedSelfUseProfit = company.ExpectedProfitForSelfUseOnlyInCents / 100;
        var endList = new List<BatteryWave>();

        var maxRecord = new SpotPrice();
        var previousRecord = new SpotPrice();
        var processingRecordNumber = 0;
        var totalRecords = spotPriceRange.Count;
        var endOfWaveRecord = new SpotPrice();
        SpotPrice startOfWaveRecord = null;
        int waveNumber = 1;

        foreach (var item in spotPriceRange.OrderByDescending(x => x.DateTime))
        {
            processingRecordNumber++;

            if (processingRecordNumber == totalRecords)
            {
                var priceDifference = PriceCostHelper.CalculatePriceWithSalesMarginCosts(maxRecord, company) - PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company);
                if (priceDifference > expectedSelfUseProfit)
                {
                    endList.Add(new BatteryWave { Id = waveNumber, spotPriceList = spotPriceRange.OrderByDescending(x => x.DateTime).Where(x => x.DateTime <= endOfWaveRecord.DateTime && x.DateTime >= item.DateTime).ToList() });
                }
                else
                {
                    if (endList.Count > 0)
                    {
                        var endListMaxRecord = endList.OrderByDescending(x => x.Id).FirstOrDefault();
                        var maxSpotPrice = endListMaxRecord.spotPriceList.OrderByDescending(x => x.DateTime).FirstOrDefault();
                        endList.RemoveAll(x => x.Id == endListMaxRecord.Id);

                        endList.Add(new BatteryWave { Id = endListMaxRecord.Id, spotPriceList = spotPriceRange.OrderByDescending(x => x.DateTime).Where(x => x.DateTime <= maxSpotPrice.DateTime && x.DateTime >= item.DateTime).ToList() });
                    }

                }

                break;
            }

            if (processingRecordNumber == 1)
            {
                maxRecord = item;
                endOfWaveRecord = item;
                previousRecord = item;
                continue;
            }

            if (PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company) > PriceCostHelper.CalculatePriceWithSalesMarginCosts(maxRecord, company) && startOfWaveRecord == null)
            {
                maxRecord = item;
            }

            if (processingRecordNumber != 1 && item.Id != maxRecord.Id)
            {
                var priceDifference = PriceCostHelper.CalculatePriceWithSalesMarginCosts(maxRecord, company) - PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company);
                if (priceDifference >= expectedSelfUseProfit)
                {
                    if (startOfWaveRecord == null)
                    {
                        startOfWaveRecord = item;
                    }
                    else
                    {
                        if (PriceCostHelper.CalculatePriceWithSalesMarginCosts(startOfWaveRecord, company) > PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company))
                        {
                            startOfWaveRecord = item;
                        }
                    }
                }

                if (startOfWaveRecord != null && (PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company) - PriceCostHelper.CalculatePriceWithSalesMarginCosts(startOfWaveRecord, company)) > expectedSelfUseProfit)
                {
                    endList.Add(new BatteryWave { Id = waveNumber, spotPriceList = spotPriceRange.OrderByDescending(x => x.DateTime).Where(x => x.DateTime <= endOfWaveRecord.DateTime && x.DateTime >= previousRecord.DateTime).ToList() });

                    maxRecord = previousRecord;
                    endOfWaveRecord = previousRecord;
                    previousRecord = previousRecord;
                    startOfWaveRecord = null;
                    waveNumber++;

                    if (PriceCostHelper.CalculatePriceWithSalesMarginCosts(maxRecord, company) < PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company))
                    {
                        maxRecord = item;
                    }

                    var priceDiff = PriceCostHelper.CalculatePriceWithSalesMarginCosts(maxRecord, company) - PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company);
                    if (item.Id != maxRecord.Id && priceDifference >= expectedSelfUseProfit)
                    {
                        if (startOfWaveRecord == null)
                        {
                            startOfWaveRecord = item;
                        }
                        else
                        {
                            if (PriceCostHelper.CalculatePriceWithSalesMarginCosts(startOfWaveRecord, company) > PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company))
                            {
                                startOfWaveRecord = item;
                            }
                        }
                    }
                    continue;
                }
            }
            previousRecord = item;
        }

        return endList;
    }

    public class BatteryWave
    {
        public int Id { get; set; }
        public List<SpotPrice> spotPriceList;
    }

    public async Task<List<BatteryControlHours>> MainCalculation()
    {
        _dbContext = await new DatabaseService().CreateDbContextAsync();

        bool hasDataForDates = true;
        var inverterInfo = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.Id == InverterBattery.InverterId);

        DateTime startDate = DateTimeMin;
        while (startDate <= DateTimeMax)
        {
            if (hasDataForDates)
            {
                hasDataForDates = await GetAverageConsumptionForDailyHours(companyId, registeredInverterId, 14, startDate.DayOfWeek);

                if (!hasDataForDates)
                {
                    hasDataForDates = await GetAverageConsumptionForDailyHours(companyId, registeredInverterId, 28, startDate.DayOfWeek);
                }

                if (!hasDataForDates)
                {
                    break;
                }

                startDate = startDate.AddDays(1);
            }
        }

        if (!hasDataForDates)
        {
            await _dbContext.DisposeAsync();
            return new List<BatteryControlHours>();
        }     
        
        var wholeDayList = await _dbContext.SpotPrice.Where(x => x.DateTime >= DateTimeMin && x.DateTime <= DateTimeMax && x.RegionId == Region.Id).OrderByDescending(x => x.DateTime).ToListAsync();
        var finalGrandDayListWithCharging = new List<BatteryControlHours>();

        //Calculate waves
        var listOfWaves = await GenerateWaves();

        var finalWavesList = new List<BatteryControlHours>();
        var maxUsableBatteryWatts = Convert.ToInt32((InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)) * (InverterBattery.AdditionalTimeForBatteryChargingPercentage + 1));
     //   var maxUsableBatteryWatts = (int)(InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10));  

        foreach (var wave in listOfWaves)
        {
            var salesCalculation = await ProcessSalesCalculation(wave.spotPriceList, maxUsableBatteryWatts);

            var selfUseCalculation = await ProcessSelfUseCalculation(wave.spotPriceList);

            if ((salesCalculation == null || salesCalculation.Count == 0) && (selfUseCalculation == null || selfUseCalculation.Count == 0))
            {
                continue;
            }

            if (selfUseCalculation.Sum(x => x.UsableWattsProfit) > salesCalculation.Sum(x => x.UsableWattsProfit))
            {
                selfUseCalculation.ForEach(item => item.WaveNumber = wave.Id);
                finalWavesList.AddRange(selfUseCalculation);
            }
            else
            {
                salesCalculation.ForEach(item => item.WaveNumber = wave.Id);
                finalWavesList.AddRange(salesCalculation);
            }
        }

        finalWavesList = finalWavesList.OrderBy(x => x.MaxPriceHour).ToList();

        //Calculate whole period
        var finalWholeDayList = new List<BatteryControlHours>();
      
        var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == companyId);
        var wholePeriodSalesCalculation = await ProcessSalesCalculation(wholeDayList, maxUsableBatteryWatts);
        var wholePeriodSelfUseCalculation = await ProcessSelfUseCalculation(wholeDayList);
        var extraBatteryChargingHours = new List<BatteryControlHours>();

        var totalWattsUsed = wholePeriodSelfUseCalculation.GroupBy(x => new { x.MinChargingPowerWhOriginal, x.SpotPriceMin.DateTime }).Select(g => g.First()).ToList().Where(x=> x.UsableWatts > 0).Sum(x => x.MinChargingPowerWhOriginal);


        if (wholePeriodSalesCalculation.Count > 0 && wholePeriodSelfUseCalculation.Count > 0)
        {
            if (wholePeriodSalesCalculation.Count > 0 && totalWattsUsed < maxUsableBatteryWatts)
            {
                wholePeriodSelfUseCalculation = CalculateSalesHoursWithoutExtraBatteryBuffer(wholePeriodSalesCalculation, wholePeriodSelfUseCalculation, inverterInfo, maxUsableBatteryWatts);

                 extraBatteryChargingHours = wholePeriodSelfUseCalculation.Where(x => x.UsableWatts == null).DistinctBy(x=> x.MinPriceHour).ToList();
            }
            else
            {
                wholePeriodSelfUseCalculation = await CalculateSalesHoursWithExtraBatteryBuffer(wholePeriodSalesCalculation, wholePeriodSelfUseCalculation, inverterInfo, company, maxUsableBatteryWatts);
            }
        }


        if (wholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit) > wholePeriodSalesCalculation.Sum(x => x.UsableWattsProfit))
        {
            finalWholeDayList.AddRange(wholePeriodSelfUseCalculation);
        }
        else
        {
            finalWholeDayList.AddRange(wholePeriodSalesCalculation);
        }

        //Calculate day final list to DB
        var finalGrandDayList = new List<BatteryControlHours>();

        if (finalWavesList.Sum(x => x.UsableWattsProfit) > finalWholeDayList.Sum(x => x.UsableWattsProfit))
        {
            finalGrandDayList.AddRange(finalWavesList);
        }
        else
        {
            finalGrandDayList.AddRange(finalWholeDayList);
        }

     //   finalGrandDayList = finalGrandDayList.Where(x => x.UsableWatts > 0).ToList();



        if (finalGrandDayList == null || finalGrandDayList.Count == 0)
        {

            foreach (var item in wholeDayList)
            {
                finalGrandDayListWithCharging.Add(new BatteryControlHours
                {
                    SpotPriceMax = item,
                    InverterBatteryId = InverterBattery.Id,
                    ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                    AmountCharged = 0,
                    GroupNumber = 9999,
                    HourProfit = 0,
                    LineProfit = 0,
                    MaxAvgHourlyConsumption = 0,
                    MaxAvgHourlyConsumptionOriginal = 0,
                    MaxAvgHourlyConsumptionOriginalDiff = 0,
                    MaxMinPriceDifference = 0,
                    MaxPriceHour = item.Time,
                    MaxPriceWithCost = 0,
                    MinChargingPowerWh = 0,
                    MinChargingPowerWhOriginal = 0,
                    MinChargingPowerWhOriginalDiff = 0,
                    MinPriceHour = item.Time,
                    MinPriceWithCost = 0,
                    Rank = 0,
                    UsableWatts = 0,
                    UsableWattsProfit = 0,
                    WaveNumber = 0,
                });
            }

            finalGrandDayListWithCharging = finalGrandDayListWithCharging.OrderBy(x => x.MaxPriceHour).ToList();

            await ClearBatteryControlHoursScheduleForPeriod();


            foreach (var entity in finalGrandDayListWithCharging)
            {
                _dbContext.Entry(entity).State = EntityState.Added;
            }

            ProcessSelfUseType(finalGrandDayListWithCharging, inverterInfo);

            await _dbContext.SaveChangesAsync();

            return new List<BatteryControlHours>();
        }

        foreach (var item in finalGrandDayList)
        {
            finalGrandDayListWithCharging.Add(new BatteryControlHours
            {
                SpotPriceMax = item.SpotPriceMin,
                SpotPriceMaxId = item.SpotPriceMin.Id,
                ActionTypeCommand = ActionTypeCommand.ChargeMax,
                AmountCharged = item.AmountCharged,
                GroupNumber = item.GroupNumber,
                HourProfit = 0,
                LineProfit = 0,
                MaxAvgHourlyConsumption = 0,
                MaxAvgHourlyConsumptionOriginal = 0,
                MaxAvgHourlyConsumptionOriginalDiff = 0,
                MaxMinPriceDifference = 0,
                MaxPriceHour = item.MinPriceHour,
                MaxPriceWithCost = 0,
                MinChargingPowerWh = 0,
                MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal,
                MinChargingPowerWhOriginalDiff = 0,
                MinPriceHour = item.MinPriceHour,
                MinPriceWithCost = item.MinPriceWithCost,
                Rank = 0,
                UsableWatts = 0,
                UsableWattsProfit = 0,
                WaveNumber = item.WaveNumber,
            });

            finalGrandDayListWithCharging.Add(new BatteryControlHours
            {
                SpotPriceMax = item.SpotPriceMax,
                SpotPriceMaxId = item.SpotPriceMax.Id,
                ActionTypeCommand = item.ActionTypeCommand,
                AmountCharged = item.AmountCharged,
                GroupNumber = item.GroupNumber,
                HourProfit = item.HourProfit,
                LineProfit = item.LineProfit,
                MaxAvgHourlyConsumption = item.MaxAvgHourlyConsumption,
                MaxAvgHourlyConsumptionOriginal = item.MaxAvgHourlyConsumptionOriginal,
                MaxAvgHourlyConsumptionOriginalDiff = item.MaxAvgHourlyConsumptionOriginalDiff,
                MaxMinPriceDifference = item.MaxMinPriceDifference,
                MaxPriceHour = item.MaxPriceHour,
                MaxPriceWithCost = item.MaxPriceWithCost,
                MinChargingPowerWh = item.MinChargingPowerWh,
                MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal,
                MinChargingPowerWhOriginalDiff = item.MinChargingPowerWhOriginalDiff,
                MinPriceHour = item.MinPriceHour,
                MinPriceWithCost = item.MinPriceWithCost,
                Rank = 0,
                UsableWatts = item.UsableWatts,
                UsableWattsProfit = item.UsableWattsProfit,
                WaveNumber = item.WaveNumber,
            });
        }

        finalGrandDayListWithCharging = finalGrandDayListWithCharging.Where(x => x.UsableWattsProfit != null).OrderBy(x => x.MaxPriceHour).ToList();
        int finalGrandDayListCounter = 1;
        var previousRecord = new BatteryControlHours();

        foreach (var item in finalGrandDayListWithCharging.ToList())
        {
            if (finalGrandDayListCounter == 1)
            {
                finalGrandDayListCounter++;
                previousRecord = item;
                continue;
            }

            if (item.SpotPriceMax.Id == previousRecord.SpotPriceMax.Id)
            {
                previousRecord.UsableWatts = previousRecord.UsableWatts + item.UsableWatts;
                previousRecord.UsableWattsProfit = previousRecord.UsableWattsProfit + item.UsableWattsProfit;
                finalGrandDayListWithCharging.Remove(item);
            }

            previousRecord = item;
            finalGrandDayListCounter++;
        }

        foreach (var item in finalGrandDayListWithCharging)
        {
            item.InverterBatteryId = InverterBattery.Id;
        }

        foreach (var item in extraBatteryChargingHours)
        {
            if (!finalGrandDayListWithCharging.Any(x => x.MinPriceHour == item.MinPriceHour))
            {
                finalGrandDayListWithCharging.Add(new BatteryControlHours
                {
                    SpotPriceMax = item.SpotPriceMin,
                    InverterBatteryId = InverterBattery.Id,
                    ActionTypeCommand = ActionTypeCommand.ChargeMax,
                    SpotPriceMaxId = item.SpotPriceMin.Id,
                    SpotPriceMin = item.SpotPriceMin,
                    AmountCharged = item.AmountCharged,
                    GroupNumber = 0,
                    HourProfit = 0,
                    LineProfit = 0,
                    MaxAvgHourlyConsumption = 0,
                    MaxAvgHourlyConsumptionOriginal = 0,
                    MaxAvgHourlyConsumptionOriginalDiff = 0,
                    MaxMinPriceDifference = 0,
                    MaxPriceHour = item.MaxPriceHour,
                    MaxPriceWithCost = 0,
                    MinChargingPowerWh = 0,
                    MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal,
                    MinChargingPowerWhOriginalDiff = 0,
                    MinPriceHour = item.MinPriceHour,
                    MinPriceWithCost = 0,
                    Rank = 0,
                    UsableWatts = 0,
                    UsableWattsProfit = 0,
                    WaveNumber = 0
                });
            }
        }

        if (extraBatteryChargingHours.Count > 0)
        {
            foreach (var item in wholeDayList)
            {
                if (!finalGrandDayListWithCharging.Any(x => x.SpotPriceMax.Id == item.Id))
                {
                    if (extraBatteryChargingHours.Any(x => x.SpotPriceMin.Time != item.Time))
                    {
                        finalGrandDayListWithCharging.Add(new BatteryControlHours
                        {
                            SpotPriceMax = item,
                            InverterBatteryId = InverterBattery.Id,
                            ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                            AmountCharged = 0,
                            GroupNumber = 9999,
                            HourProfit = 0,
                            LineProfit = 0,
                            MaxAvgHourlyConsumption = 0,
                            MaxAvgHourlyConsumptionOriginal = 0,
                            MaxAvgHourlyConsumptionOriginalDiff = 0,
                            MaxMinPriceDifference = 0,
                            MaxPriceHour = item.Time,
                            MaxPriceWithCost = 0,
                            MinChargingPowerWh = 0,
                            MinChargingPowerWhOriginal = 0,
                            MinChargingPowerWhOriginalDiff = 0,
                            MinPriceHour = item.Time,
                            MinPriceWithCost = 0,
                            Rank = 0,
                            UsableWatts = 0,
                            UsableWattsProfit = 0,
                            WaveNumber = 0,
                        });
                    }

                }
            }
        }
        else
        {
            foreach (var item in wholeDayList)
            {
                if (!finalGrandDayListWithCharging.Any(x => x.SpotPriceMax.Id == item.Id))
                {

                        finalGrandDayListWithCharging.Add(new BatteryControlHours
                        {
                            SpotPriceMax = item,
                            InverterBatteryId = InverterBattery.Id,
                            ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                            AmountCharged = 0,
                            GroupNumber = 9999,
                            HourProfit = 0,
                            LineProfit = 0,
                            MaxAvgHourlyConsumption = 0,
                            MaxAvgHourlyConsumptionOriginal = 0,
                            MaxAvgHourlyConsumptionOriginalDiff = 0,
                            MaxMinPriceDifference = 0,
                            MaxPriceHour = item.Time,
                            MaxPriceWithCost = 0,
                            MinChargingPowerWh = 0,
                            MinChargingPowerWhOriginal = 0,
                            MinChargingPowerWhOriginalDiff = 0,
                            MinPriceHour = item.Time,
                            MinPriceWithCost = 0,
                            Rank = 0,
                            UsableWatts = 0,
                            UsableWattsProfit = 0,
                            WaveNumber = 0,
                        });


                }
            }
        }


        finalGrandDayListWithCharging = finalGrandDayListWithCharging.OrderBy(x => x.MaxPriceHour).ToList();

        await ClearBatteryControlHoursScheduleForPeriod();


        foreach (var entity in finalGrandDayListWithCharging)
        {
            _dbContext.Entry(entity).State = EntityState.Added;
        }

        if (inverterInfo.PVInverterIsSeparated)
        {
            await BuildBatteryhoursSummerData();
            finalGrandDayListWithCharging = await ReplaceChargeRemainingSunWithSelfUseWhenHasSolarEnergy(finalGrandDayListWithCharging, inverterInfo, company);
        }

        ProcessSelfUseType(finalGrandDayListWithCharging, inverterInfo);

        if (inverterInfo.CalculationFormula == CalculationFormula.Winter)
        {
            await _dbContext.SaveChangesAsync();

            await _dbContext.DisposeAsync();
        }

        return finalGrandDayListWithCharging;
    }

    private async Task BuildBatteryhoursSummerData()
    {
        var batteryHoursSummers = BuildInitialData();

        var weatherSuccessful = await ProcessWeatherRequest();

        if (weatherSuccessful)
        {
            batteryHoursSummers = await ProcessWeatherData(batteryHoursSummers);

            batteryHoursSummers = await ProcessPrices(batteryHoursSummers);

            BatteryHoursSummer = batteryHoursSummers;
        }  


    }

    private async Task<List<BatteryHoursSummer>> ProcessPrices(List<BatteryHoursSummer> batteryHoursSummers)
    {
        var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == companyId);

        var spotPrices = await _dbContext.SpotPrice.Where(x => x.RegionId == company.RegionId && x.Date >= DateOnly.FromDateTime(DateTimeMin) && x.Date <= DateOnly.FromDateTime(DateTimeMax)).ToListAsync();

        foreach (var item in batteryHoursSummers)
        {
            var spotPrice = spotPrices.FirstOrDefault(x => x.DateTime == item.DateTime);

            if (spotPrice != null)
            {
                item.PurchasePrice = (double)PriceCostHelper.CalculateMaxPriceWithPurchaseMarginCosts(spotPrice, company);
                item.SalePrice = (double)PriceCostHelper.CalculatePriceWithSalesMarginCosts(spotPrice, company);
                item.SpotPrice = spotPrice;
            }
        }

        return batteryHoursSummers;
    }

    private async Task<bool> ProcessWeatherRequest()
    {
        var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == companyId);
        var inverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.Id == InverterBattery.InverterId);
        var solarPanelCapacityRecordForDay = await _dbContext.SolarPanelCapacity.CountAsync(x => x.SolarPanelsDirecation == inverter.SolarPanelsDirecation);
        TimeSpan difference = DateTimeMax - DateTimeMin;
        int numberOfWeatherRecordExpected = (difference.Days + 1) * solarPanelCapacityRecordForDay;

        var solarPanelCapacity = await _dbContext.SolarPanelCapacity.ToListAsync();
        var countrySolarCapacity = await _dbContext.CountrySolarCapacity.Where(x => x.CountryId == company.CountryId).ToListAsync();

        WeatherProcessingService weatherProcessingService = new WeatherProcessingService(company, inverter, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), WeatherApiComService, solarPanelCapacity, countrySolarCapacity);
        await weatherProcessingService.ProcessCurrenDateWeatherData();
        await weatherProcessingService.ProcessWeatherData();

        var numberOfWeatherDataRecords = await _dbContext.WeatherForecastData.CountAsync(x => x.InverterId == InverterBattery.InverterId && x.Date >= DateOnly.FromDateTime(DateTimeMin) && x.Date <= DateOnly.FromDateTime(DateTimeMax));

        if (numberOfWeatherDataRecords >= numberOfWeatherRecordExpected)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private async Task<List<BatteryHoursSummer>> ProcessWeatherData(List<BatteryHoursSummer> batteryHoursSummers)
    {
        var weatherData = await _dbContext.WeatherForecastData.Where(x => x.InverterId == InverterBattery.InverterId && x.Date >= DateOnly.FromDateTime(DateTimeMin) && x.Date <= DateOnly.FromDateTime(DateTimeMax)).ToListAsync();

        foreach (var item in batteryHoursSummers)
        {
            var weatherItem = weatherData.FirstOrDefault(x => x.DateTime == item.DateTime);

            if (weatherItem != null)
            {
                var resultConsumption = item.AvgConsumption - weatherItem.EstimatedSolarPower;
                item.ForecastedConsumption = resultConsumption < 0 ? 0 : resultConsumption;

                if (resultConsumption < 0)
                {
                    item.ForecastedSolarRemaining = Math.Abs(resultConsumption);
                }

                item.EstimatedSolar = weatherItem.EstimatedSolarPower;
            }
        }

        return batteryHoursSummers;
    }

    private List<BatteryHoursSummer> BuildInitialData()
    {
        var batteryHoursSummers = new List<BatteryHoursSummer>();

        var runningDateTime = DateTimeMin;

        while (runningDateTime <= DateTimeMax)
        {
            var batteryHoursSummer = new BatteryHoursSummer
            {
                DateTime = runningDateTime,
                AvgConsumption = 0,
                ForecastedConsumption = 0,
                PurchasePrice = 0,
                SalePrice = 0,
                ForecastedSolarRemaining = 0
            };

            batteryHoursSummers.Add(batteryHoursSummer);

            runningDateTime = runningDateTime.AddHours(1);
        }

        return batteryHoursSummers;
    }

    private async Task<List<BatteryControlHours>> ReplaceChargeRemainingSunWithSelfUseWhenHasSolarEnergy(List<BatteryControlHours> batteryControlHours, Inverter inverter, Company company)
    {

        if (BatteryHoursSummer != null)
        {
            foreach (var item in batteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun))
            {
                var matchingRecord = BatteryHoursSummer.FirstOrDefault(b => b.DateTime == item.SpotPriceMax.DateTime);

                if (matchingRecord != null && matchingRecord.EstimatedSolar > 100
                    && matchingRecord.SalePrice >= (company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {
                    item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                    item.MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal;
                    item.RemainingBattery = item.RemainingBattery;
                    item.UsableWatts = 0;
                    item.RemainingBattery = 0;
                    item.ConsumeBatterySellPower = 0;

                    if (item.ActionTypeCommand == ActionTypeCommand.InverterSelfUse || item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                    {
                        if (inverter.UseFixedAvgHourlyWatts)
                        {
                            item.MaxAvgHourlyConsumptionOriginal = inverter.FixedAvgHourlyWatts;
                        }
                        else
                        {
                            var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == inverter.Id
                                            && x.DayOfWeek == item.SpotPriceMax.Date.DayOfWeek
                                            && x.TimeHour == item.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                            item.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                        }
                    }
                }
            }
        }

        return batteryControlHours;
    }

    private List<BatteryControlHours> ProcessSelfUseType(List<BatteryControlHours> BatteryControlHours, Inverter inverter)
    {
        if (inverter.UseInverterSelfUse)
        {
            foreach (var item in BatteryControlHours)
            {
                if (item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                {
                    item.ActionTypeCommand = ActionTypeCommand.InverterSelfUse;
                }
            }
        }

        return BatteryControlHours;
    }

    private async Task<List<BatteryControlHours>> CalculateSalesHoursWithExtraBatteryBuffer(List<BatteryControlHours> wholePeriodSalesCalculation, List<BatteryControlHours> wholePeriodSelfUseCalculation, Inverter inverter, Company company, int maxUsableBatteryWatts)
    {
        string wholePeriodSelfUseCalculationJSON = JsonConvert.SerializeObject(wholePeriodSelfUseCalculation);
        var copyOfWholePeriodSelfUseCalculation = JsonConvert.DeserializeObject<List<BatteryControlHours>>(wholePeriodSelfUseCalculationJSON);    

        foreach (var item in wholePeriodSalesCalculation)
        {
            var batteryControlHours = new List<BatteryControlHours>();
            var availableSpotPriceRange = await _dbContext.SpotPrice.Where(x => x.DateTime >= DateTimeMin && x.DateTime < item.SpotPriceMax.DateTime && x.RegionId == Region.Id).ToListAsync();

            for (int i = availableSpotPriceRange.Count - 1; i == 0; i--)
            {
                var priceItem = availableSpotPriceRange[i];
                if (copyOfWholePeriodSelfUseCalculation.Any(x => x.SpotPriceMin.DateTime == priceItem.DateTime || x.SpotPriceMax.DateTime == priceItem.DateTime))
                {
                    availableSpotPriceRange.RemoveAt(i);
                }
            }

            for (int i = availableSpotPriceRange.Count - 1; i == 0; i--)
            {
                var priceItem = availableSpotPriceRange[i];
                var priceItemCost = (double)PriceCostHelper.CalculatePriceWithSalesMarginCosts(priceItem, company);

                var maxMinPriceDifference = item.MaxPriceWithCost - priceItemCost;

                if (maxMinPriceDifference * 100 >= company.ExpectedProfitForSelfUseOnlyInCents)
                {
                    batteryControlHours.Add(new BatteryControlHours
                    { SpotPriceMin = availableSpotPriceRange[i],
                        SpotPriceMax = item.SpotPriceMax,
                        MaxMinPriceDifference = maxMinPriceDifference,
                     MaxPriceWithCost = item.MaxPriceWithCost,
                    MinPriceWithCost = priceItemCost});
                }
            }

            if (batteryControlHours.Count > 0 && inverter.MaxSalesPowerCapacity > 0)
            {

                var cheapestHour = batteryControlHours.OrderBy(x => x.MinPriceWithCost).FirstOrDefault();

                foreach (var selfUseItem in copyOfWholePeriodSelfUseCalculation)
                {
                    if (selfUseItem.SpotPriceMax.Id == item.SpotPriceMax.Id)
                    {
                        var inverterMaxPowerRemaining = Convert.ToInt32(inverter.MaxSalesPowerCapacity * 1000) + selfUseItem.UsableWatts;

                        if (inverterMaxPowerRemaining > Convert.ToInt32(inverter.MaxPower * 1000))
                        {
                            inverterMaxPowerRemaining = Convert.ToInt32(inverter.MaxPower * 1000);
                        }

                        selfUseItem.SpotPriceMin = cheapestHour.SpotPriceMin;
                        selfUseItem.MinPriceHour = cheapestHour.SpotPriceMin.Time;
                        selfUseItem.UsableWatts = inverterMaxPowerRemaining;
                        selfUseItem.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                        selfUseItem.MaxMinPriceDifference = cheapestHour.MaxMinPriceDifference;
                        selfUseItem.LineProfit = selfUseItem.MaxMinPriceDifference * selfUseItem.UsableWatts;
                        selfUseItem.UsableWattsProfit = selfUseItem.LineProfit;
                    }

                   if (copyOfWholePeriodSelfUseCalculation.Sum(x => x.UsableWatts) > maxUsableBatteryWatts)
                    {
                        break;
                    }

                }

                var wholeDayOriginalProfit = wholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit);
                var wholeDayModifiedProfit = copyOfWholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit);

                if (wholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit) > copyOfWholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit))
                {
                    break;
                }
                else
                {
                    wholePeriodSelfUseCalculationJSON = JsonConvert.SerializeObject(copyOfWholePeriodSelfUseCalculation);
                    wholePeriodSelfUseCalculation = JsonConvert.DeserializeObject<List<BatteryControlHours>>(wholePeriodSelfUseCalculationJSON);
                }
            }
        }

        return wholePeriodSelfUseCalculation;
    }

    private List<BatteryControlHours> CalculateSalesHoursWithoutExtraBatteryBuffer(List<BatteryControlHours> wholePeriodSalesCalculation, List<BatteryControlHours> wholePeriodSelfUseCalculation, Inverter inverter, int maxUsableBatteryWatts)
    {
        string wholePeriodSelfUseCalculationJSON = JsonConvert.SerializeObject(wholePeriodSelfUseCalculation);
        var copyOfWholePeriodSelfUseCalculation = JsonConvert.DeserializeObject<List<BatteryControlHours>>(wholePeriodSelfUseCalculationJSON);

        foreach (var item in wholePeriodSalesCalculation)
        {
            if (copyOfWholePeriodSelfUseCalculation.Sum(x=> x.UsableWatts) > maxUsableBatteryWatts)
            {
                return wholePeriodSelfUseCalculation;
            }

            var inverterMaxPowerRemaining = Convert.ToInt32(inverter.MaxSalesPowerCapacity * 1000);

            foreach (var selfItem in copyOfWholePeriodSelfUseCalculation)
            {
                if (selfItem.SpotPriceMax.Id == item.SpotPriceMax.Id)
                {
                    if (selfItem != null && selfItem.UsableWatts != null)
                    {
                        inverterMaxPowerRemaining = inverterMaxPowerRemaining + (int)selfItem.UsableWatts;
                    }
                    

                    if (inverterMaxPowerRemaining > Convert.ToInt32(inverter.MaxPower * 1000))
                    {
                        inverterMaxPowerRemaining = Convert.ToInt32(inverter.MaxPower * 1000);
                    }

                    var previousUsableWatts = 0;

                    if (selfItem != null && selfItem.UsableWatts != null)
                    {
                        previousUsableWatts = (int)selfItem.UsableWatts;
                    }

                    if (inverter.MaxSalesPowerCapacity > 0)
                    {
                        selfItem.UsableWatts = inverterMaxPowerRemaining;


                        selfItem.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                        selfItem.LineProfit = item.MaxMinPriceDifference * selfItem.UsableWatts;
                        selfItem.UsableWattsProfit = selfItem.LineProfit;

                        inverterMaxPowerRemaining -= (int)previousUsableWatts;
                    }                       


                    while (inverterMaxPowerRemaining > 0)
                    {
                        var nextMinProfitRecord = copyOfWholePeriodSelfUseCalculation.OrderBy(o => o.MaxMinPriceDifference).FirstOrDefault(x => x.UsableWatts > 0);
                        inverterMaxPowerRemaining -= (int)nextMinProfitRecord.UsableWatts;

                        if (inverterMaxPowerRemaining > 0)
                        {
                            nextMinProfitRecord.UsableWatts = 0;
                            nextMinProfitRecord.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                            nextMinProfitRecord.LineProfit = 0;
                            nextMinProfitRecord.UsableWattsProfit = 0;
                        }
                        else
                        {
                            continue;
                        }

                    }

                }

            }

            var wholeDayOriginalProfit = wholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit);
            var wholeDayModifiedProfit = copyOfWholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit);

            if (wholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit) > copyOfWholePeriodSelfUseCalculation.Sum(x => x.UsableWattsProfit))
            {
                break;
            }
            else
            {
                wholePeriodSelfUseCalculationJSON = JsonConvert.SerializeObject(copyOfWholePeriodSelfUseCalculation);
                wholePeriodSelfUseCalculation = JsonConvert.DeserializeObject<List<BatteryControlHours>>(wholePeriodSelfUseCalculationJSON);
            }
        }

        return wholePeriodSelfUseCalculation;
    }

    public async Task <List<BatteryHoursCalculatorDto>> ConvertSpotPricesToDto(List<SpotPrice> spotPrices)
    {
        var batteryHoursCalculators = Mapper.Map<List<BatteryHoursCalculatorDto>>(spotPrices);
        var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == companyId);
        foreach (var item in batteryHoursCalculators)
        {
            item.CostWithSalesMargin = PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company);
        }

        return batteryHoursCalculators;
    }

    private async Task<List<BatteryControlHours>> Calculate(CalculationType calculationType, List<SpotPrice> spotPriceRange)
    {
        var company = await _dbContext.Inverter.Where(x => x.Id == InverterBattery.InverterId).Include(x => x.Company).Select(x => x.Company).FirstOrDefaultAsync();
        spotPriceRange = spotPriceRange.OrderByDescending(x => x.DateTime).ToList();

        List<BatteryHoursCalculatorDto> batteryHoursCalculatorDto = await ConvertSpotPricesToDto(spotPriceRange);

        var inverterHoursAvgConsumption = GetAverageConsumptionForWeekDays().Result;
        var inverterInfo = await _dbContext.Inverter.Include(x => x.InverterBattery).FirstOrDefaultAsync(x => x.Id == InverterBattery.InverterId);
        foreach (var item in batteryHoursCalculatorDto)
        {
            int averageConsumption = 0;
            if (inverterInfo.UseFixedAvgHourlyWatts)
            {
                averageConsumption = inverterInfo.FixedAvgHourlyWatts;
            }
            else
            {
                averageConsumption = (int)inverterHoursAvgConsumption.Where(x => x.TimeHour == item.Time && x.DayOfWeek == item.Date.DayOfWeek).ToList().Average(x => x.AvgHourlyConsumption);
            }

            item.AvgHourlyConsumption = calculationType == CalculationType.Sales ?
                (int)Math.Round(InverterBattery.ChargingPowerFromGridKWh * 1000) :
                    averageConsumption > (int)Math.Round(InverterBattery.ChargingPowerFromGridKWh * 1000) ?
                    (int)Math.Round(InverterBattery.ChargingPowerFromGridKWh * 1000) :
                    averageConsumption;

            item.ChargingPowerWh = (int)Math.Round(InverterBattery.ChargingPowerFromGridKWh * 1000);
        }

        var copyOfBatteryHoursCalculatorDto =
            JsonConvert.DeserializeObject<List<BatteryHoursCalculatorDto>>(
            JsonConvert.SerializeObject(batteryHoursCalculatorDto));

        var orderedPricesList = new List<BatteryControlHours>();

        //Aku laadimise tundides mahu vähendamine.
        foreach (var item in batteryHoursCalculatorDto)
        {
            item.AvgHourlyConsumption = item.AvgHourlyConsumption * Convert.ToInt32((inverterInfo.InverterBattery.FirstOrDefault().AdditionalTimeForBatteryChargingPercentage + 1));
        }

        while (batteryHoursCalculatorDto.Count > 0)
        {
            BatteryHoursCalculatorDto maxPriceRecord = new BatteryHoursCalculatorDto{ };
            BatteryHoursCalculatorDto minPriceRecord = new BatteryHoursCalculatorDto { };


            if (calculationType == CalculationType.Sales)
            {
                maxPriceRecord = batteryHoursCalculatorDto.OrderByDescending(x => x.PriceNoTax).FirstOrDefault();
                minPriceRecord = batteryHoursCalculatorDto.Where(x => x.DateTime < maxPriceRecord.DateTime).ToList().OrderBy(x => x.PriceNoTax).FirstOrDefault();
            }
            else
            {
                 maxPriceRecord = batteryHoursCalculatorDto.OrderByDescending(x => x.CostWithSalesMargin).FirstOrDefault();
                 minPriceRecord = batteryHoursCalculatorDto.Where(x => x.DateTime < maxPriceRecord.DateTime).ToList().OrderBy(x => x.CostWithSalesMargin).FirstOrDefault();
            }
        

            var minPriceWithCost = PriceCostHelper.CalculatePriceWithSalesMarginCosts(minPriceRecord, company);
            var maxPriceWithCost = calculationType == CalculationType.Sales ?
                PriceCostHelper.CalculateMaxPriceWithPurchaseMarginCosts(maxPriceRecord, company) :
                PriceCostHelper.CalculatePriceWithSalesMarginCosts(maxPriceRecord, company);

            var priceWithCostDifference = maxPriceWithCost - minPriceWithCost;
            var actionTypeCommand = calculationType == CalculationType.Sales ? DetermineActionTypeCommandForSalesPriceDifference(company, priceWithCostDifference, inverterInfo) : DetermineActionTypeCommandForSelfUsePriceDifference(company, priceWithCostDifference);

            double minMaxConsumptionDifference = 0;

            if (minPriceRecord != null && minPriceRecord.ChargingPowerWh >= 0)
            {
                minMaxConsumptionDifference = minPriceRecord.ChargingPowerWh - maxPriceRecord.AvgHourlyConsumption;

                if (minMaxConsumptionDifference >= 0)
                {
                    minPriceRecord.ChargingPowerWh = (int)minMaxConsumptionDifference;
                    maxPriceRecord.AvgHourlyConsumption = 0;
                }
                else
                {
                    maxPriceRecord.AvgHourlyConsumption = maxPriceRecord.AvgHourlyConsumption - minPriceRecord.ChargingPowerWh;
                    minPriceRecord.ChargingPowerWh = 0;
                }
            }

            orderedPricesList.Add(new BatteryControlHours
            {
                SpotPriceMin = minPriceRecord,
                SpotPriceMax = maxPriceRecord,
                MinPriceWithCost = minPriceWithCost,
                MinPriceHour = minPriceRecord != null ? minPriceRecord.Time : null,
                MaxPriceWithCost = maxPriceWithCost,
                MaxPriceHour = maxPriceRecord != null ? maxPriceRecord.Time : null,
                MaxMinPriceDifference = priceWithCostDifference,
                MinChargingPowerWhOriginal = minPriceRecord != null ? copyOfBatteryHoursCalculatorDto.FirstOrDefault(x => x.Id == minPriceRecord.Id).ChargingPowerWh : null,
                MinChargingPowerWh = minPriceRecord?.ChargingPowerWh,
                MaxAvgHourlyConsumptionOriginal = maxPriceRecord != null ? copyOfBatteryHoursCalculatorDto.FirstOrDefault(x => x.Id == maxPriceRecord.Id).AvgHourlyConsumption : null,
                MaxAvgHourlyConsumption = maxPriceRecord?.AvgHourlyConsumption,
                ActionTypeCommand = actionTypeCommand
            });


            if (minPriceRecord != null && minPriceRecord.ChargingPowerWh <= 0)
            {
                batteryHoursCalculatorDto.RemoveAll(x => x.Id == minPriceRecord.Id);

            }

            if (minPriceRecord == null || maxPriceRecord.AvgHourlyConsumption <= 0)
            {
                batteryHoursCalculatorDto.RemoveAll(x => x.Id == maxPriceRecord.Id);
            }

        }
        return orderedPricesList;
    }

    private ActionTypeCommand DetermineActionTypeCommandForSalesPriceDifference(Company company, double? maxMinPriceDifference, Inverter inverter)
    {
        ActionTypeCommand actionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;

        if (maxMinPriceDifference == null)
        {
            return actionTypeCommand;
        }

        if (maxMinPriceDifference * 100 > company.ExpectedProfitForSelfUseOnlyInCents && inverter.MaxSalesPowerCapacity > 0)
        {
            actionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
        }

        return actionTypeCommand;
    }

    private ActionTypeCommand DetermineActionTypeCommandForSelfUsePriceDifference(Company company, double? maxMinPriceDifference)
    {
        ActionTypeCommand actionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;

        if (maxMinPriceDifference == null)
        {
            return actionTypeCommand;
        }

        if (maxMinPriceDifference * 100 > company.ExpectedProfitForSelfUseOnlyInCents)
        {
            actionTypeCommand = ActionTypeCommand.SelfUse;
        }

        return actionTypeCommand;
    }
}
