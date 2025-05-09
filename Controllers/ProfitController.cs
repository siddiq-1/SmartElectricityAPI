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
                var baseQuery = await _dbContext.SofarStateHourly
                    .Include(x => x.Company)
                    .Where(x => x.Date.Value.Year == year)
                    .Join(_dbContext.InverterBattery,
                          s => s.RegisteredInverterId,
                          ib => ib.InverterId,
                          (s, ib) => new { s, ib })
                    .Where(x => x.ib.CapacityKWh > 0)
                    .GroupBy(x => new
                    {
                        x.s.Date.Value.Year,
                        x.s.Date.Value.Month,
                        x.s.Company.Name,
                        x.s.CompanyId,
                        x.ib.CapacityKWh
                    })
                    .Select(g => new CompanyProfitByKwh
                    {
                        Month = g.Key.Month,
                        CompanyId = g.Key.CompanyId.Value,
                        CompanyName = g.Key.Name,
                        Capacity = g.Key.CapacityKWh,
                        SumWinOrLoseFromMyGridUsage = g.Sum(x => x.s.WinOrLoseFromMyGridUsage)
                    })
                    .ToListAsync();

                // Collect unique companies/months
                var lookupDates = baseQuery.Select(x => new
                {
                    CompanyId = x.CompanyId,
                    Date = new DateOnly(year, x.Month, 1)
                }).Distinct().ToList();

                var companyIds = baseQuery.Select(x => x.CompanyId).Distinct().ToList();

                // Load all VATs in one query
                var vatRanges = await _dbContext.CountryVatRange
                    .Where(v => companyIds.Contains(v.CountryId))
                    .ToListAsync();

                // Load all company-user mappings in one go
                var companyUsers = await _dbContext.CompanyUsers
                    .Where(cu => companyIds.Contains(cu.CompanyId))
                    .ToListAsync();

                foreach (var item in baseQuery)
                {
                    var lookupDate = new DateOnly(year, item.Month, 1);

                    var vat = vatRanges.FirstOrDefault(x =>
                        x.CountryId == item.CompanyId &&
                        lookupDate >= x.StartDate && lookupDate <= x.EndDate);

                    var isLoginUserFromCompany = companyUsers
                        .Any(x => x.CompanyId == item.CompanyId && x.UserId == _userInfo.Id);

                    if (vat != null)
                    {
                        item.SumWinOrLoseFromMyGridUsage *= vat.VatRate;
                        item.ProfitPerKwh = item.SumWinOrLoseFromMyGridUsage / item.Capacity;
                    }

                    item.IsLoggedInUser = isLoginUserFromCompany;

                    if (!isLoginUserFromCompany)
                    {
                        item.CompanyName = $@"{item.CompanyName.First()}******{item.CompanyName.Last()}";
                    }
                }

                var result = baseQuery
                    .OrderByDescending(x => x.ProfitPerKwh)
                    .Select((x, index) => new
                    {
                        Position = index + 1,
                        IsLoggedInUser = x.IsLoggedInUser,
                        CompanyName = x.CompanyName,
                        ProfitPerKwh = x.ProfitPerKwh
                    })
                    .ToList();

                return Ok(new
                {
                    Top10Records = result.Take(10),
                    LoggedInRecord = result.FirstOrDefault(x => x.IsLoggedInUser)
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
