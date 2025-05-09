using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Engine;

public class CompanyPriceRates
{
    private MySQLDBContext _dbContext;

    public CompanyPriceRates(MySQLDBContext context)
    {
        _dbContext = context;
    }


    public async Task GenerateTransactionsForDate(DateOnly priceDate)
    {
        var companyHourlyFees = await _dbContext.CompanyHourlyFees.Select(x => x.CompanyId).Distinct().ToListAsync();

        if (companyHourlyFees.Count > 0)
        {
            foreach (var item in companyHourlyFees)
            {
                var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == item);
                var templateCompanyFees = await _dbContext.CompanyHourlyFees.Where(x => x.CompanyId == item).ToListAsync();
                var companyHourlyFeesTransactions = new List<CompanyHourlyFeesTransactions>();
                var existingCompanyHourlyFeesTransactions = await _dbContext.CompanyHourlyFeesTransactions.Where(x => x.CompanyId == item && x.Date == priceDate).ToListAsync();

                foreach (var tempRecord in templateCompanyFees)
                {
                    var newTransactionRecord = new CompanyHourlyFeesTransactions
                    {

                        Date = priceDate,
                        CompanyId = tempRecord.CompanyId,
                        Time = tempRecord.Time,
                    };

                    if (DateTimeHelper.IsWeekend(priceDate) && company!.UseNightTimeFeeOnSaturdayAndSunday)
                    {
                        newTransactionRecord.BrokerServiceFree = company.BrokerSalesMargin;
                        newTransactionRecord.NetworkServiceFree = company.NetworkServiceFeeNightTime;
                    }
                    else
                    {
                        newTransactionRecord.BrokerServiceFree = tempRecord.BrokerServiceFee;
                        newTransactionRecord.NetworkServiceFree = tempRecord.NetworkServiceFee;
                    }


                    if (!existingCompanyHourlyFeesTransactions.Any(x => x.Time == newTransactionRecord.Time && x.Date == newTransactionRecord.Date && x.CompanyId == newTransactionRecord.CompanyId))
                    {
                        companyHourlyFeesTransactions.Add(newTransactionRecord);
                    }

                }

                _dbContext.CompanyHourlyFeesTransactions.AddRange(companyHourlyFeesTransactions);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
