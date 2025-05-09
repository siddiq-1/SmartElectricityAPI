using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Engine;

namespace SmartElectricityAPI.Services;

public class BatteryPriceCalculatorService
{
    private Company? company = null;
    private MySQLDBContext? _dbContext = null;
    private DateTime startDateTime;
    private DateTime endDateTime;
    private Inverter? inverter = null;
    private WeatherApiComService _weatherApiComService { get;set; }

    public BatteryPriceCalculatorService(Company company, DateTime startDateTime, DateTime endDateTime, WeatherApiComService weatherApiComService)
    {
        this.company = company;
        this.startDateTime = startDateTime;
        this.endDateTime = endDateTime;
        _weatherApiComService = weatherApiComService;
    }

    public async Task ProcessOperations()
    {
        _dbContext = await new DatabaseService().CreateDbContextAsync();

        await FindParameters();

        await PerformCalculations();

        await _dbContext!.DisposeAsync();
    }

    private async Task FindParameters()
    {
        company = await _dbContext!.Company.Include(x => x.Region).FirstOrDefaultAsync(x => x.Id == company!.Id);

        inverter = await _dbContext!.Inverter.Include(x=> x.InverterBattery).FirstOrDefaultAsync(x => x.CompanyId == company!.Id);
    }

    private async Task PerformCalculations()
    {
        if (inverter != null && inverter.InverterBattery.Count > 0)
        {
            BatteryHoursCalculator batteryHoursCalculator = new BatteryHoursCalculator(new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0), new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, 23, 0, 0), company!.Region!, inverter!.InverterBattery.FirstOrDefault()!, company.Id, (int)inverter.RegisteredInverterId!, _weatherApiComService, _dbContext);
            
            await batteryHoursCalculator.MainCalculation();
        }
    }

}
