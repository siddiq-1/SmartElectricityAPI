using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Engine.Helpers;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.Engine;

public class InverterCalculator
{
  
    private MySQLDBContext _dbContext;

    public InverterCalculator(MySQLDBContext context)
    {
        _dbContext = context;
    }
    public async Task Calculate(int regionId)
    {

        List<SpotPrice> datesrange = await _dbContext.SpotPrice.Where(x => x.RegionId == regionId && x.Date >= DateOnly.FromDateTime(DateTime.Now) && x.Date <= DateOnly.FromDateTime(DateTime.Now.AddDays(1))).ToListAsync();

        var companiesInRegion = await _dbContext.Company.Where(x => x.RegionId == regionId).Select(s => s.Id).ToListAsync();
        List<Inverter> inverter =await _dbContext.Inverter.Where(x => companiesInRegion.Contains(x.CompanyId)).ToListAsync();

        foreach (var item in inverter)
        {
            await InverterHoursCalculator(item, datesrange);
        }
    }

    public async Task InverterHoursCalculator(Inverter inverter, List<SpotPrice> datesrange)
    {

        var company = await _dbContext.Company.FirstOrDefaultAsync(x=> x.Id == inverter.CompanyId);

        foreach (var item in datesrange)
        {
            if (! await _dbContext.InverterCompanyHours.AnyAsync(x=> x.InverterId == inverter.Id && x.CompanyId == inverter.CompanyId && x.SpotPriceId == item.Id))
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

         await _dbContext.SaveChangesAsync();
     
    }
}
