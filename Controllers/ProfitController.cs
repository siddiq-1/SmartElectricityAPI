using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Claims;

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
            if (_userInfo.SelectedCompanyId != 0)
            {
                var companyProfit = await _dbContext.SofarStateHourly
                        .Where(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Date.Value.Year == year)
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

                return Ok(companyProfit);
            }

            return BadRequest();
        }


        [HttpGet("v1/{year}"), Authorize]
        public async Task<ActionResult> GetCompanyProfitByKwh(int year)
        {
            if (_userInfo.SelectedCompanyId != 0)
            {
                var result = await _dbContext.CompanyProfitByKwh.FromSqlRaw("call GetCompanyVatImpact()").ToListAsync();
                var currentCompanyName = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == _userInfo.SelectedCompanyId);

                foreach (var item in result)
                {
                    if (currentCompanyName != null && item.CompanyName != currentCompanyName.Name)
                    {
                        item.CompanyName = $"{item.CompanyName.First()}*****{item.CompanyName.Last()}";
                    }
                }
                var loggedInResult = currentCompanyName != null ? result.FirstOrDefault(x => x.CompanyName == currentCompanyName.Name) : new CompanyProfitByKwh();

                return Ok(new
                {
                    Top10Records = result.Take(10),
                    LoggedInRecord = loggedInResult
                });
            }

            return BadRequest();
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
