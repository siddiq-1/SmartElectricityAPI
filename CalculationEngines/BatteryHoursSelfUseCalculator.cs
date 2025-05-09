using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;

namespace SmartElectricityAPI.Engine;

public class BatteryHoursSelfUseCalculator
{
    private DateTime DateTimeMin { get; set; }
    private DateTime DateTimeMax { get; set; }
    private Region Region { get; set; }
    private InverterBattery InverterBattery { get; set; }
    private MySQLDBContext _dbContext;
    private IMapper Mapper { get; set; }
    private int CompanyId { get; set; }
    private int RegisteredInverterId { get; set; }
    private int TotalHours { get; set; }
    private Company Company { get; set; }
    private Inverter Inverter { get; set; }

    public BatteryHoursSelfUseCalculator(DateTime dateTimeMin, DateTime dateTimeMax, Region region, InverterBattery inverterBattery, int companyId, int registeredInverterId)
    {
        Mapper = AutoMapperConfig.InitializeAutoMapper();
        DateTimeMin = dateTimeMin;
        DateTimeMax = dateTimeMax;
        Region = region;
        InverterBattery = inverterBattery;
        CompanyId = companyId;
        RegisteredInverterId = registeredInverterId;
        TotalHours = (int)(DateTimeMax - DateTimeMin).TotalHours;
    }

    public async Task PopulateData()
    {
        _dbContext = await new DatabaseService().CreateDbContextAsync();

        Company = await  _dbContext.Company.FirstOrDefaultAsync(x => x.Id == CompanyId);
        Inverter =  await _dbContext.Inverter.FirstOrDefaultAsync(x => x.RegisteredInverterId == RegisteredInverterId);

        await GetAverageConsumptionForDailyHours(CompanyId, RegisteredInverterId, 14);

        var wholeDayList = await _dbContext.SpotPrice.Where(x => x.DateTime >= DateTimeMin && x.DateTime <= DateTimeMax && x.RegionId == Region.Id).OrderByDescending(x => x.DateTime).ToListAsync();

        var finalGrandDayListWithCharging = new List<BatteryControlHours>();

        List<InverterHoursAvgConsumption> inverterHoursAvgConsumption = new List<InverterHoursAvgConsumption>();

        if (!Inverter.UseFixedAvgHourlyWatts)
        {
            inverterHoursAvgConsumption = GetAverageConsumptionForWeekDays().Result;
        }


        foreach (var item in wholeDayList)
        {
            int averageConsumption = 0;

            if (Inverter.UseFixedAvgHourlyWatts)
            {
                averageConsumption = Inverter.FixedAvgHourlyWatts;
            }
            else
            {
                var avgConsumptionList = inverterHoursAvgConsumption
                    .Where(x => x.TimeHour == item.Time && x.DayOfWeek == item.Date.DayOfWeek).ToList();

                if (avgConsumptionList != null && avgConsumptionList.Count > 0)
                {
                    averageConsumption = (int)avgConsumptionList.Average(x => x.AvgHourlyConsumption);
                }
                else
                {
                    Console.WriteLine($"Company name:{Company.Name} Problematic time:{item.Time} and dayOfWeek:{item.Date.DayOfWeek}");
                    averageConsumption = 300;
                }
                
            }

            averageConsumption = averageConsumption * Convert.ToInt32((InverterBattery.AdditionalTimeForBatteryChargingPercentage + 1));

            finalGrandDayListWithCharging.Add(new BatteryControlHours
            {
                SpotPriceMax = item,
                InverterBatteryId = InverterBattery.Id,
                ActionTypeCommand = ActionTypeCommand.SelfUse,
                AmountCharged = 0,
                GroupNumber = 0,
                HourProfit = 0,
                LineProfit = 0,
                MaxAvgHourlyConsumption = 0,
                MaxAvgHourlyConsumptionOriginal = averageConsumption,
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


        ProcessSelfUseType(finalGrandDayListWithCharging);

        await ClearBatteryControlHoursScheduleForPeriod();


        foreach (var entity in finalGrandDayListWithCharging)
        {
            _dbContext.Entry(entity).State = EntityState.Added;
        }

      //  _dbContext.SaveChanges();

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> GetAverageConsumptionForDailyHours(int companyId, int registeredInverterId, int numberOfDaysBack)
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

        List<TimeSpan> expectedTimeSpans = Enumerable.Range(0, TotalHours)
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

        if (result.Count < TotalHours)
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

    private List<BatteryControlHours> ProcessSelfUseType(List<BatteryControlHours> BatteryControlHours)
    {
        if (Inverter.UseInverterSelfUse)
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

    private async Task ClearBatteryControlHoursScheduleForPeriod()
    {
        var scheduleRecordToDelete = await _dbContext.BatteryControlHours.Where(x => x.SpotPriceMax.DateTime >= DateTimeMin && x.SpotPriceMax.DateTime <= DateTimeMax && x.InverterBatteryId == InverterBattery.Id).ToListAsync();

        if (scheduleRecordToDelete.Count > 0)
        {
            _dbContext.BatteryControlHours.RemoveRange(scheduleRecordToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }

}
