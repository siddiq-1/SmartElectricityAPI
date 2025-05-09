using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.Services;

public class FixedPriceService
{
    private readonly Company _company;
    private DateTime _startDateTime;
    private DateTime _endDateTime;
    private readonly MySQLDBContext _dbContext;
    public FixedPriceService(Company company, DateTime startDateTime, DateTime endDateTime, MySQLDBContext dbContext)
    {
        _company = company;
        _startDateTime = startDateTime;
        _endDateTime = endDateTime;
        _dbContext = dbContext;
    }

    public async Task GenerateFixedPriceRecords()
    {
        var fixedPrices = await _dbContext.CompanyFixedPrice
            .Where(x => x.CompanyId == _company.Id)
            .ToListAsync();

        var spotPrices = new List<SpotPrice>();

        foreach (var fixedPrice in fixedPrices)
        {
            spotPrices.Add(new SpotPrice
            {
                DateTime = _startDateTime,
                PriceNoTax = fixedPrice.PurchasePrice,
                Rank = 1
            });
        }
    }
}
