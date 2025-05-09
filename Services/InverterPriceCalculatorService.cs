using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Engine.Helpers;

namespace SmartElectricityAPI.Services;

public class InverterPriceCalculatorService
{
    private Company? company = null;
    private MySQLDBContext? _dbContext = null;
    private DateTime? startDateTime = null;
    private DateTime? endDateTime = null;
    private List<SpotPrice>? datesrange = null;
    private Inverter? inverter = null;

    public InverterPriceCalculatorService(Company company, DateTime startDateTime, DateTime endDateTime)
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

        inverter = await _dbContext!.Inverter.FirstOrDefaultAsync(x => x.CompanyId == company.Id);

        datesrange = await _dbContext.SpotPrice.Where(x => x.RegionId == company!.RegionId && x.Date >= DateOnly.FromDateTime((DateTime)startDateTime!) && x.Date <= DateOnly.FromDateTime((DateTime)endDateTime!)).OrderBy(x => x.DateTime).Reverse().ToListAsync();
    }

    private async Task PerformCalculations()
    {
        if (datesrange != null && datesrange.Count > 0 && inverter != null)
        {
            foreach (var item in datesrange!)
            {
                if (! await _dbContext!.InverterCompanyHours.AnyAsync(x => x.InverterId == inverter!.Id && x.CompanyId == inverter.CompanyId && x.SpotPriceId == item.Id))
                {
                    _dbContext.InverterCompanyHours.Add(
                        new InverterCompanyHours
                        {
                            InverterId = inverter.Id,
                            CompanyId = inverter.CompanyId,
                            ActionType = item.PriceNoTax > company!.BrokerPurchaseMargin ? Enums.ActionType.ThreePhaseAntiRefluxOn : Enums.ActionType.ThreePhaseAntiRefluxOff,
                            SpotPriceId = item.Id,
                            CostWithSalesMargin = PriceCostHelper.CalculatePriceWithSalesMarginCosts(item, company),
                            CostWithPurchaseMargin = PriceCostHelper.CalculateMaxPriceWithPurchaseMarginCosts(item, company)
                        });
                }
            }

            await _dbContext!.SaveChangesAsync();
        }
    }
}
