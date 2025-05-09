using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Engine;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SmartElectricityAPI.Services;

public class DevicePriceCalculatorService
{
    private Company? company = null;
    private MySQLDBContext? _dbContext = null;
    private DateTime? startDateTime = null;
    private DateTime? endDateTime = null;
    private List<SpotPrice>? datesrange = null;
    private List<Device>? devices = null;

    public DevicePriceCalculatorService(Company company, DateTime startDateTime, DateTime endDateTime)
    {
        this.company = company;
        this.startDateTime = startDateTime;
        this.endDateTime = endDateTime;
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

        datesrange = await _dbContext.SpotPrice.Where(x => x.RegionId == company!.RegionId && x.Date >= DateOnly.FromDateTime((DateTime)startDateTime!) && x.Date <= DateOnly.FromDateTime((DateTime)endDateTime!)).OrderBy(x => x.DateTime).Reverse().ToListAsync();

        devices = await _dbContext.Device.Where(x => x.CompanyId == company!.Id).ToListAsync();
    }

    private async Task PerformCalculations()
    {
        if (datesrange != null)
        {
            foreach (var device in devices!)
            {
                var deviceCompanyHours = await _dbContext!.DeviceCompanyHours.Where(x => x.CompanyId == company!.Id && x.DeviceId == device.Id && datesrange!.Select(x => x.Id).Contains(x.SpotPriceId)).ToListAsync();

                _dbContext.DeviceCompanyHours.RemoveRange(deviceCompanyHours);

                await _dbContext.SaveChangesAsync();
            }

            foreach (var device in devices)
            {
                DevicePriceCalculator devicePriceCalculator = new DevicePriceCalculator(_dbContext!, device, datesrange!, company!.RegionId);

                await devicePriceCalculator.OffPricesCalculatorV2();
            }
        }
    }
}
