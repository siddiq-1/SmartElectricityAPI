using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CompanyPriceRatesController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<CompanyHourlyFees> _logger;
    public CompanyPriceRatesController(MySQLDBContext context, ILogger<CompanyHourlyFees> logger)
    {
        _dbContext = context;
        _logger = logger;
    }

    [HttpGet, Authorize]
    public async Task<IEnumerable<CompanyHourlyFees>> GetCompanyPriceRates()
    {
        var companyId = User.Claims.Where(x => x.Type == Constants.CompanyId).FirstOrDefault();
        var companies = Array.ConvertAll(companyId.Value.Split(','), int.Parse).ToList();

        if (companies.Count == 1)
        {
            return await _dbContext.CompanyHourlyFees.Include(i => i.Company).Where(x => x.CompanyId == companies.FirstOrDefault()).ToListAsync();
        }
        else
        {
            return await _dbContext.CompanyHourlyFees.Include(i => i.Company).Where(x => companies.Contains(x.CompanyId)).ToListAsync();
        }
    }
}
