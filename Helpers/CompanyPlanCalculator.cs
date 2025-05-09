using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Engine;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Infrastructure;

public class CompanyPlanCalculator
{
    private int _companyId;
    private WeatherApiComService _weatherApiComService;

    public CompanyPlanCalculator(int companyId, WeatherApiComService weatherApiComService)
    {
        this._companyId = companyId;
        _weatherApiComService = weatherApiComService;
    }

    public CompanyPlanCalculator(WeatherApiComService weatherApiComService)
    {
        _weatherApiComService = weatherApiComService;
    }

    public async Task CalculateForMissingInverter()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1).Date);

            var query =
                from battery in _dbContext.InverterBattery
                join inverter in _dbContext.Inverter on battery.InverterId equals inverter.Id
                join company in _dbContext.Company on inverter.CompanyId equals company.Id
                where company.IsActive
                && inverter.MaxPower > 1
                && _dbContext.BatteryControlHours
                    .Where(c => c.InverterBatteryId == battery.Id
                    && c.SpotPriceMax.Date == tomorrow)
                    .Count() < 24
                select new
                {
                    Company = company,
                    InverterBattery = battery
                };

            var result = await query.ToListAsync();

            foreach (var item in result)
            {
                Console.WriteLine($"PlanCalcMissing Calc for company ${item.Company.Name}");
                this._companyId = item.Company.Id;
                await CalculateInverter();           
            }
        }
    }

    public async Task CalculateForMissingDevices()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1).Date);

            var query =
                from company in _dbContext.Company
                join device in _dbContext.Device on company.Id equals device.CompanyId
                where company.IsActive
                && _dbContext.DeviceCompanyHours
                    .Where(c => c.DeviceId == device.Id
                    && c.SpotPrice.Date == tomorrow)
                    .Count() < 24
                select new
                {
                    Company = company,
                    Device = device
                };

            var result = await query.ToListAsync();

           
            foreach (var item in result)
            {
                Console.WriteLine($"PlanCalcMissing DeviceOnly Calc for company ${item.Company.Name}");
                this._companyId = item.Company.Id;

                await CalculateDevice();
            }          
        }
    }

    public async Task CalculateDevice()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var company = await _dbContext.Company.Include(x => x.Region).FirstOrDefaultAsync(x => x.Id == this._companyId);
            var region = await _dbContext.Region.FirstOrDefaultAsync(x => x.Id == company.RegionId);

            var currentDateTime = DateTime.Now;

            var isTodayCalculated = await _dbContext.DeviceCompanyHours.AnyAsync(x => x.CompanyId == company.Id && x.SpotPrice.Date == DateOnly.FromDateTime(currentDateTime));

            var currentDateTimeForCalc = DateTime.Now;

            if (isTodayCalculated)
            {
                currentDateTimeForCalc = currentDateTimeForCalc.AddDays(1);
            }
           
            var startDateTime = new DateTime(currentDateTimeForCalc.Year, currentDateTimeForCalc.Month, currentDateTimeForCalc.Day, 0, 0, 0);
            var endDateTime = new DateTime(currentDateTimeForCalc.Year, currentDateTimeForCalc.Month, currentDateTimeForCalc.Day, 23, 0, 0);

            List<SpotPrice> datesrange = await _dbContext.SpotPrice.Where(x => x.RegionId == company.RegionId && x.Date >= DateOnly.FromDateTime(startDateTime) && x.Date <= DateOnly.FromDateTime(endDateTime)).OrderBy(x => x.DateTime).Reverse().Take(Constants.NumberOfPricesToGet).Reverse().ToListAsync();
            List<Device> devices = await _dbContext.Device.Where(x => x.CompanyId == company.Id).ToListAsync();


            foreach (var device in devices)
            {
                var deviceCompanyHours = await _dbContext.DeviceCompanyHours.Where(x => x.CompanyId == company.Id && x.DeviceId == device.Id && datesrange.Select(x => x.Id).Contains(x.SpotPriceId)).ToListAsync();
                _dbContext.DeviceCompanyHours.RemoveRange(deviceCompanyHours);

                await _dbContext.SaveChangesAsync();
            }

            foreach (var device in devices)
            {
                DevicePriceCalculator devicePriceCalculator = new DevicePriceCalculator(_dbContext, device, datesrange, company.RegionId);

                await devicePriceCalculator.OffPricesCalculatorV2();
            }           
        }
    }

    public async Task CalculateInverter()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var company = await _dbContext.Company.Include(x => x.Region).FirstOrDefaultAsync(x => x.Id == this._companyId);
            var region = await _dbContext.Region.FirstOrDefaultAsync(x => x.Id == company.RegionId);

            List<SpotPrice> datesrange = await _dbContext.SpotPrice.Where(x => x.RegionId == company.RegionId && x.Date >= DateOnly.FromDateTime(DateTime.Now) && x.Date <= DateOnly.FromDateTime(DateTime.Now.AddDays(2))).OrderBy(x => x.DateTime).Reverse().Take(Constants.NumberOfPricesToGet).Reverse().ToListAsync();

            List<Inverter> inverters = await _dbContext.Inverter.Where(x => x.CompanyId == company.Id).ToListAsync();

            foreach (var inverter in inverters)
            {
                var inverterCompanyHours = await _dbContext.InverterCompanyHours.Where(x => x.CompanyId == company.Id && x.InverterId == inverter.Id && datesrange.Select(x => x.Id).Contains(x.SpotPriceId)).ToListAsync();

                _dbContext.InverterCompanyHours.RemoveRange(inverterCompanyHours);

                await _dbContext.SaveChangesAsync();
            }


            foreach (var inverter in inverters)
            {
                InverterCalculator inverterCalculator = new InverterCalculator(_dbContext);

                await inverterCalculator.InverterHoursCalculator(inverter, datesrange);
            }

            DateTime minDateTime = datesrange.Min(x => x.DateTime);
            DateTime maxDateTime = datesrange.Max(x => x.DateTime);

            var countrySolarCapacityForCompany = await _dbContext.CountrySolarCapacity.Where(x => x.CountryId == company.CountryId).ToListAsync();
            var solarPanelCapacity = await _dbContext.SolarPanelCapacity.ToListAsync();
            var dateTomorrow = DateTime.Now.AddDays(1);

            foreach (var inverter in inverters)
            {
                var inverterBatteries = await _dbContext.InverterBattery.Where(x => x.InverterId == inverter.Id).ToListAsync();
                if (inverter != null && inverter.CalculationFormula == CalculationFormula.Winter)
                {
                    foreach (var item in inverterBatteries)
                    {
                        DateTime startDate = minDateTime;
                        while (startDate <= maxDateTime)
                        {
                            BatteryHoursCalculator batteryHoursCalculator = new BatteryHoursCalculator(new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0), new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 0, 0), company.Region, item, company.Id, (int)inverter.RegisteredInverterId!, _weatherApiComService, _dbContext);
                            await batteryHoursCalculator.MainCalculation();
                            startDate = startDate.AddDays(1);
                        }
                    }

                    if (inverter != null && inverter.UseWeatherForecast)
                    {
                        WeatherProcessingService weatherProcessingService = new WeatherProcessingService(company, inverter, DateOnly.FromDateTime(dateTomorrow), _weatherApiComService, solarPanelCapacity, countrySolarCapacityForCompany);
                        await weatherProcessingService.ProcessCurrenDateWeatherData();
                        await weatherProcessingService.ProcessWeatherData();
                    }
                }

                if (inverter.CalculationFormula == CalculationFormula.Summer
                    || inverter.CalculationFormula == CalculationFormula.SummerWithoutConsumeMax
                    || inverter.CalculationFormula == CalculationFormula.SummerMedium)
                {

                    DateTime dateToday = DateTime.Now.AddHours(1);
                    var inverterBattery = inverterBatteries.FirstOrDefault(x => x.InverterId == inverter.Id);

                    if (inverterBattery != null)
                    {
                        BatteryHoursCalculatorSummer batteryHoursCalculatorSummer = new BatteryHoursCalculatorSummer
                            (new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, dateToday.Hour, 0, 0),
                            new DateTime(dateTomorrow.Year, dateTomorrow.Month, dateTomorrow.Day, 23, 0, 0),
                            region, inverterBattery, company.Id, (int)inverter.RegisteredInverterId!, _weatherApiComService);
                        await batteryHoursCalculatorSummer.PopulateData();
                    }
                }

                if (inverter.CalculationFormula == CalculationFormula.SelfUse)
                {
                    try
                    {
                        if (inverter != null && inverter.UseWeatherForecast)
                        {
                            WeatherProcessingService weatherProcessingService = new WeatherProcessingService(company, inverter, DateOnly.FromDateTime(dateTomorrow), _weatherApiComService, solarPanelCapacity, countrySolarCapacityForCompany);
                            await weatherProcessingService.ProcessCurrenDateWeatherData();
                            await weatherProcessingService.ProcessWeatherData();
                        }

                        DateTime dateToday = DateTime.Now.AddHours(1);
                        var inverterBattery = inverterBatteries.FirstOrDefault(x => x.InverterId == inverter.Id);

                        if (inverterBattery != null)
                        {
                            BatteryHoursSelfUseCalculator batteryHoursSelfUseCalculator = new BatteryHoursSelfUseCalculator(new DateTime(dateToday.Year, dateToday.Month, dateToday.Day, dateToday.Hour, 0, 0), new DateTime(dateTomorrow.Year, dateTomorrow.Month, dateTomorrow.Day, 23, 0, 0), region, inverterBattery, company.Id, (int)inverter.RegisteredInverterId!);
                            await batteryHoursSelfUseCalculator.PopulateData();
                        }

                    }
                    catch (Exception ex)
                    {
                        var errorLog = new ErrorLog { Message = $"{ex.Message}, stacktrace: {ex.InnerException.TargetSite.Name}" };
                        _dbContext.ErrorLog.Add(errorLog);
                        await _dbContext.SaveChangesAsync();
                    }

                }
            }
        }

    }
}
