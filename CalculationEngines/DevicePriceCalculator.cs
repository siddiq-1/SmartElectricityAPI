using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Engine.Helpers;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using Constants = SmartElectricityAPI.Services.Constants;

namespace SmartElectricityAPI.Engine;

public class DevicePriceCalculator
{
    private MySQLDBContext _dbContext;
    private List<SpotPrice> endList = new();
    private Device device = new Device();
    private List<SpotPrice> datesrange = new();
    private int regionId;
    private Company company = new();
    public DevicePriceCalculator(MySQLDBContext context, Device device, List<SpotPrice> datesrange, int regionId)
	{

        _dbContext = context;
        this.device = device;
        this.datesrange = datesrange;
        this.regionId = regionId;
        company = _dbContext.Company.FirstAsync(Company => Company.Id == device.CompanyId).Result;

    }

    public async Task OffPricesCalculatorV2()
    {
        if (device.MaxStopHoursIn24h == 0
            || device.FirstHourPercentageKwPriceRequirementBeforeHeating == 0
            || device.MaxStopHoursConsecutive == 0)
        {

            return;
        }

        await RemoveHoursWithNoCalculation();

        AddSequenceNumber();

        AddSalesMargin();

        datesrange = CalculateFallPercentage();

        datesrange = ProcessWave(datesrange);

        endList = endList.OrderBy(x => x.SequenceNumber).ToList();

        if (endList.Count(x=> x.GroupNumber > 0) > device.MaxStopHoursIn24h)
        {
            FindLessProfitHour();       
        }

        if (endList.Count > 0 &&
            this.company.DeviceMinProfitInCents != null)
        {
            RemoveWavesWhereProfitNotExpected();
        }

        if (endList.Count > 0)
        {
            var deviceSensor = await _dbContext.Sensor.FirstOrDefaultAsync(x => x.DeviceId == device.Id && x.CompanyId == company.Id);

            if (deviceSensor != null
                && deviceSensor.DeviceActionType == Enums.DeviceActionType.On)
            {
                await InvertActionType();
            }              

            await SaveToDb();
        }
    }

    private void AddSalesMargin()
    {
        foreach (var item in datesrange)
        {
            item.CostWithSalesMargin = PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company);
        }
    }



    private async Task InvertActionType()
    {
        foreach (var item in endList)
        {
            if (item.deviceActionType == Enums.DeviceActionType.None)
            {
                item.deviceActionType = Enums.DeviceActionType.On;
            }

            if (item.deviceActionType == Enums.DeviceActionType.Off)
            {
                item.deviceActionType = Enums.DeviceActionType.None;
            }
        }      
     }

    private async Task SaveToDb()
    {
        foreach (var item in endList)
        {
            try
            {
                _dbContext.DeviceCompanyHours.Add(
                new DeviceCompanyHours
                {
                    Device = device,
                    Company = company, 
                    DeviceActionType = item.deviceActionType,
                    CostWithSalesMargin = item.CostWithSalesMargin,
                    SpotPrice = item
                });
            }
            catch (Exception ex)
            {

            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private void FindLessProfitHour()
    {
        var hoursToEliminate = endList.Count(x => x.GroupNumber > 0) - device.MaxStopHoursIn24h;

        foreach (var item in endList.Where(x => x.GroupNumber > 0).OrderBy(o=> o.GroupProfit).ThenBy(o=> o.DateTime).ToList())
        {
            if (hoursToEliminate == 0)
            {
                break;
            }
            item.GroupNumber = 0;
            item.GroupProfit = 0;
            item.deviceActionType = Enums.DeviceActionType.None;

            hoursToEliminate--;
        }
    }

    private void RemoveWavesWhereProfitNotExpected()
    {
        var endListGroupNumbers = endList.Where(x => x.GroupNumber > 0).Select(s => s.GroupNumber).Distinct().ToList();

        foreach (var groupNumber in endListGroupNumbers)
        {
            var firstRecord = endList.FirstOrDefault(x => x.GroupNumber == groupNumber);

            if (firstRecord != null && firstRecord.GroupProfit < this.company.DeviceMinProfitInCents / 100)
            {
                foreach (var item in endList.Where(x=> x.GroupNumber == groupNumber).ToList())
                {
                    item.GroupNumber = 0;
                    item.GroupProfit = 0;
                    item.deviceActionType = Enums.DeviceActionType.None;
                }
            }
        }
    }

    private List<SpotPrice> ProcessWave(List<SpotPrice> datesRange)
    {
        if (datesRange.Count == 0)
        {
            return datesRange;
        }

        var suitableFallHour = datesRange.FirstOrDefault(x => x.PercentageFall >= device.FirstHourPercentageKwPriceRequirementBeforeHeating);

        if (suitableFallHour == null)
        {
            foreach (var item in datesrange)
            {
                item.deviceActionType = Enums.DeviceActionType.None;
                endList.Add(item);
            }

            datesrange.RemoveAll(x=> datesrange.Any());

            return datesRange;
        }


        var suitableFallHourPrevRec = datesRange[datesRange.IndexOf(suitableFallHour) - 1];

        var waveRecords = datesRange.Where(x=> x.SequenceNumber > suitableFallHourPrevRec.SequenceNumber).TakeWhile(w=> w.CostWithSalesMargin >= suitableFallHourPrevRec.CostWithSalesMargin).ToList();
        
        if (waveRecords.Count == 0)
        {
            foreach (var item in datesrange)
            {
                item.deviceActionType = Enums.DeviceActionType.Off;
                endList.Add(item);
            }
            datesrange.RemoveAll(x => datesrange.Any());
            return datesRange;
        }

        waveRecords = CalculatePriceDifferenceWithCompareToRecord(waveRecords, suitableFallHourPrevRec);

        waveRecords = CalculateTotalEarningForEachWaveRecord(waveRecords);

        SetOffHoursToFinalList(waveRecords);

        return ProcessWave(datesRange);       
    }

    private void SetOffHoursToFinalList(List<SpotPrice> waveRecords)
    {
        var highestEarningRecord = waveRecords.OrderByDescending(x => x.TotalProfitEarning).FirstOrDefault();

        var offHourRecord = waveRecords.Where(x => x.SequenceNumber >= highestEarningRecord.SequenceNumber).Take(Convert.ToInt32(device.MaxStopHoursConsecutive)).ToList();

        var groupProfit = offHourRecord.Sum(x => x.AmountFall) / offHourRecord.Count;
        var maxGroupNo = endList.Count > 0 ? endList.Max(x => x.GroupNumber) : 0;

        foreach (var item in offHourRecord)
        {
            item.deviceActionType = Enums.DeviceActionType.Off;
            item.GroupNumber = maxGroupNo + 1;
            item.GroupProfit = groupProfit;
            endList.Add(item);
        }

        foreach (var item in waveRecords.Where(x=> x.SequenceNumber < highestEarningRecord.SequenceNumber).ToList())
        {
                item.deviceActionType = Enums.DeviceActionType.None;
                endList.Add(item);
        }

        datesrange.RemoveAll(x => waveRecords.Any(w => w.SequenceNumber < highestEarningRecord.SequenceNumber && w.SequenceNumber == x.SequenceNumber));

        datesrange.RemoveAll(x => offHourRecord.Any(o => o.SequenceNumber == x.SequenceNumber));
    }

    private List<SpotPrice> CalculateTotalEarningForEachWaveRecord(List<SpotPrice> waveRecords)
    {      
        foreach (var item in waveRecords)
        {
            item.TotalProfitEarning = item.AmountFall + waveRecords.Where(x=> x.SequenceNumber > item.SequenceNumber).Take(Convert.ToInt32(device.MaxStopHoursConsecutive) - 1).ToList().Sum(x => x.AmountFall);
        }

        return waveRecords;
    }

    private List<SpotPrice> CalculatePriceDifferenceWithCompareToRecord(List<SpotPrice> waveRecords, SpotPrice compareToRecord)
    {
        foreach (var item in waveRecords)
        {
            item.AmountFall = (double)item.CostWithSalesMargin - (double)compareToRecord.CostWithSalesMargin;     

        }

        return waveRecords;
    }

    private List<SpotPrice> CalculateFallPercentage()
    {
        SpotPrice? previousItem = null;

        foreach (var item in datesrange)
        {
            if (previousItem != null)
            {
                item.PercentageFall = (((double)item.CostWithSalesMargin / (double)previousItem.CostWithSalesMargin) - 1) * 100;
            }
            previousItem = item;
        }

        return datesrange;
    }

    private void AddSequenceNumber()
    {
        datesrange = datesrange.OrderByDescending(x => x.DateTime).ToList();
        int sequenceNum = 1;
        foreach (var item in datesrange)
        {
            item.SequenceNumber = sequenceNum;
            sequenceNum++;
        }    
    }

    private async Task<List<SpotPrice>> RemoveHoursWithNoCalculation()
    {
        var deviceHoursNoCalculations = await _dbContext.DeviceHoursNoCalculation.Where(x => x.DeviceId == device.Id).Select(s => s.Time).ToListAsync();

        datesrange = datesrange.Where(x => !deviceHoursNoCalculations.Contains(x.Time)).ToList();

        return datesrange;
    }
}
