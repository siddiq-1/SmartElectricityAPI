using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using System;

namespace SmartElectricityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfitController : ControllerBase
    {
        private MySQLDBContext _dbContext;
        private readonly IUserInfo _userInfo;

        public ProfitController(MySQLDBContext context, IUserInfo userInfo)
        {
            _dbContext = context;
            _userInfo = userInfo;
        }

        [HttpGet("{year}"), Authorize]
        public async Task<ActionResult> GetCompanyProfit(int year)
        {
            //if (_userInfo.SelectedCompanyId != 0)
            //{
            //    var companyProfit = await _dbContext.SofarStateHourly
            //            .Where(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Date.Value.Year == year)
            //            .GroupBy(x => new { x.Date.Value.Year, x.Date.Value.Month })
            //            .Select(g => new CompanyProfit
            //            {
            //                Month = g.Key.Month,
            //                SumCostOfConsumpWithOutMygGid = g.Sum(x => x.CostOfConsumpWithOutMygGid),
            //                SumCostPurchaseMinusSellFromGrid = g.Sum(x => x.CostPurchaseMinusSellFromGrid),
            //                SumWinOrLoseFromMyGridUsage = g.Sum(x => x.WinOrLoseFromMyGridUsage)
            //            })
            //            .ToListAsync();

            //    foreach (var item in companyProfit)
            //    {
            //        var lookupDate = new DateOnly(year, item.Month, 1);
            //        var countryVatRange = await _dbContext.CountryVatRange
            //        .Where(x => x.CountryId == _userInfo.SelectedCompanyId && lookupDate >= x.StartDate && lookupDate <= x.EndDate)
            //        .FirstOrDefaultAsync();

            //        if (countryVatRange != null)
            //        {
            //            item.SumWinOrLoseFromMyGridUsage = item.SumWinOrLoseFromMyGridUsage * countryVatRange.VatRate;
            //            item.SumCostOfConsumpWithOutMygGid = item.SumCostOfConsumpWithOutMygGid * countryVatRange.VatRate;
            //            item.SumCostPurchaseMinusSellFromGrid = item.SumCostPurchaseMinusSellFromGrid * countryVatRange.VatRate;
            //        }     
            //    }
                return Ok(await GetCompanyWithoutBatteryProfit(year));
            //}

            //return BadRequest();
        }

       [NonAction]
        private async Task<List<CompanyProfit>> GetCompanyWithoutBatteryProfit(int year)
        {
            //if (_userInfo.SelectedCompanyId != 0)
            //{
                var companyProfit = await _dbContext.SofarStateHourly
                        .Where(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Date.Value.Year == year && (x.battery_power != 0 && x.battery_tempMax != 0 && x.battery_current != 0 && x.batterySOC != 0 && x.battery_tempMin != 0 && x.battery_voltage != 0))
                        .GroupBy(x => new { x.Date.Value.Year, x.Date.Value.Month })
                        .Select(g => new CompanyProfit
                        {
                            Month = g.Key.Month,
                            SumCostOfConsumpWithOutMygGid = g.Sum(x => x.CostOfConsumpWithOutMygGid),
                            SumCostPurchaseMinusSellFromGrid = g.Sum(x => x.CostPurchaseMinusSellFromGrid),
                            SumWinOrLoseFromMyGridUsage = g.Sum(x => x.WinOrLoseFromMyGridUsage)
                        })
                        .ToListAsync();

                foreach (var item in companyProfit)
                {
                    var lookupDate = new DateOnly(year, item.Month, 1);
                    var countryVatRange = await _dbContext.CountryVatRange
                    .Where(x => x.CountryId == _userInfo.SelectedCompanyId && lookupDate >= x.StartDate && lookupDate <= x.EndDate)
                    .FirstOrDefaultAsync();

                    if (countryVatRange != null)
                    {
                        item.SumWinOrLoseFromMyGridUsage = item.SumWinOrLoseFromMyGridUsage * countryVatRange.VatRate;
                        item.SumCostOfConsumpWithOutMygGid = item.SumCostOfConsumpWithOutMygGid * countryVatRange.VatRate;
                        item.SumCostPurchaseMinusSellFromGrid = item.SumCostPurchaseMinusSellFromGrid * countryVatRange.VatRate;
                    }
                }

                return companyProfit;
            //}
        }


        [HttpGet("AvailableYears"), Authorize]
        public async Task<ActionResult> GetAvailableYears()
        {
            if (_userInfo.SelectedCompanyId != 0)
            {
                var availableYears = await _dbContext.SofarStateHourly
                        .Where(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Date.HasValue)
                        .Select(x => x.Date!.Value.Year)
                        .Distinct()
                        .ToListAsync();

                if (availableYears != null && availableYears.Count > 0)
                {
                    return Ok(availableYears);
                }
                else
                {
                    return NotFound();
                }                
            }

            return BadRequest();
        }
    }
}
