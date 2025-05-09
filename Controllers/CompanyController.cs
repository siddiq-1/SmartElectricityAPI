using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Engine;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Migrations;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<CompanyController> _logger;
    private readonly IUserInfo _userInfo;
    private readonly WeatherApiComService _weatherApiComService;
    private readonly InverterApiService _inverterApiService;
    public CompanyController(MySQLDBContext context, ILogger<CompanyController> logger, IUserInfo userInfo, WeatherApiComService weatherApiComService, InverterApiService inverterApiService)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
        _weatherApiComService = weatherApiComService;
        _inverterApiService = inverterApiService;
    }
    [HttpGet, Authorize]
    public  async Task<IEnumerable<Company>> GetCompanies()
    {
        if (_userInfo.IsAdmin)
        {
            return await _dbContext.Company.OrderByDescending(x=> x.Id).Include(i => i.Country).Include(r => r.Region).ToListAsync();
        }
            var companyId = User.Claims.Where(x => x.Type == Constants.CompanyId).FirstOrDefault();
            var companies = Array.ConvertAll(companyId.Value.Split(','), int.Parse).ToList();

        if (companies.Count == 1)
        {
            return await _dbContext.Company.Where(x => x.Id == companies.FirstOrDefault()).ToListAsync();
        }
        else
        {
            return await _dbContext.Company.Where(x => companies.Contains(x.Id)).ToListAsync();
        }   
    }

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
        var company = await _dbContext.Company.Where(x=> x.Id == id).FirstOrDefaultAsync();

        if (company == null)
        {
            return NotFound();
        }

        return company;
    }
    [HttpGet, Route("GetUserCompany"), Authorize]
    public async Task<ActionResult> GetUserCompany()
    {
        var company = await _dbContext.Company.Where(x => x.Id == _userInfo.SelectedCompanyId).FirstOrDefaultAsync();

        if (company == null)
        {
            return NotFound();
        }

        return Ok(company);
    }

    [HttpGet, Route("GetCompanyIds"), Authorize]
    public async Task<ActionResult> GetCompanyIds()
    {
        if (_userInfo.IsAdmin)
        {
            DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1).Date);

            var query =
                from company in _dbContext.Company.OrderByDescending(x => x.Id)
                from inverter in _dbContext.Inverter.Where(i => i.CompanyId == company.Id).DefaultIfEmpty()
                from battery in _dbContext.InverterBattery.Where(b => b.InverterId == inverter.Id).DefaultIfEmpty()
                let batteryControlCount = _dbContext.BatteryControlHours
                    .Where(c => c.InverterBatteryId == battery.Id && c.SpotPriceMax.Date == tomorrow)
                    .Count()
                select new
                {
                    CompanyName = company.Name,
                    CompanyId = company.Id,
                    InverterId = inverter != null ? inverter.Id : (int?)null,
                    InverterBatteryId = battery != null ? battery.Id : (int?)null,
                    CapacityKWh = battery != null ? battery.CapacityKWh : (double?)null,
                    MaxPower = inverter != null ? inverter.MaxPower : (double?)null,
                    SolarPanelsMaxPower = inverter != null ? inverter.SolarPanelsMaxPower : (double?)null,
                    UseFixedAvgHourlyWatts = inverter != null ? inverter.UseFixedAvgHourlyWatts : false,
                    CalculationFormula = inverter != null ? inverter.CalculationFormula : (CalculationFormula?)null,
                    UseHzMarket = battery != null ? battery.UseHzMarket : false,
                    company.IsActive,
                    HasLessThan24BatteryControlHours = batteryControlCount < 24
                };

            var result = await query.ToListAsync();

            return Ok(result);
        }
        else
        {
            return BadRequest();
        }

    }



    [HttpPut("UseHzMarket/{id}"), Authorize]
    public async Task<IActionResult> UpdateHzMarket(int? id)
    {
        if (id == null || id == 0)
        {
            return BadRequest();
        }

        var battery = await _dbContext.InverterBattery.FirstOrDefaultAsync(x => x.Id == id);

        if (_userInfo.IsAdmin && battery != null)
        {
            battery.UseHzMarket = !battery.UseHzMarket;
            battery.ClientUseHzMarket = battery.UseHzMarket;

            _dbContext.Entry(battery).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            if (!Debugger.IsAttached)
            {
                var authRes = await _inverterApiService.GetToken();

                if (authRes != null)
                {
                    await _inverterApiService.RefreshInverterData(authRes.token);
                }
            }
           
        }
        else
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpPut("ToggleActive/{companyId}"), Authorize]
    public async Task<IActionResult> ToggleActive(int? companyId)
    {
        if (companyId == null || companyId == 0)
        {
            return BadRequest();
        }

        var company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == companyId);

        if (_userInfo.IsAdmin)
        {
            company.IsActive = !company.IsActive;


            _dbContext.Entry(company).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            if (!Debugger.IsAttached)
            {
                var authRes = await _inverterApiService.GetToken();

                if (authRes != null)
                {
                    await _inverterApiService.RefreshInverterData(authRes.token);
                }
            }


        }
        else
        {
            return BadRequest();
        }

        return Ok();
    }

    [HttpPost, Authorize]
    public async Task<ActionResult<Company>> PostCompany(Company company)
    {
        if (_userInfo.IsAdmin)
        {
            if (await CompanyNameExists(company.Name))
            {
                return Conflict("Company already exists.");
            }

            _dbContext.Company.Add(company);

            await _dbContext.SaveChangesAsync();

            var companyHourlyFees = new List<CompanyHourlyFees>();

            //Add initial company hourly fees
            for (int i = 0; i < 24; i++)
            {
                companyHourlyFees.Add(new CompanyHourlyFees { CompanyId = company.Id, Time = new TimeSpan(i, 0, 0), BrokerServiceFee = 0, NetworkServiceFee = 0 });
            }

            _dbContext.CompanyHourlyFees.AddRange(companyHourlyFees);

            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
        }
        else
        {
            return BadRequest();
        }

    }


    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult>UpdateCompany(int id, Company company)
    {
        if (id != company.Id)
        {
            return BadRequest();
        }

        if (!company.IsValid())
        {
            return BadRequest("Please check that time periods don\'t overlap and are filled");
        }

        if (await _dbContext.Company.AnyAsync(x=> x.Name == company.Name && x.Id != id))
        {
            return BadRequest("Company with that name already exists.");
        }

        if (company.Latitude == null || company.Longitude == null)
        {
            return BadRequest("Please enter address.");
        }

        var existingCompany = await _dbContext.Company.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (existingCompany == null)
        {
            return NotFound();
        }

        // Allow only admin users to change the IsActive property
        if (!_userInfo.IsAdmin)
        {
            company.IsActive = existingCompany.IsActive;
        }


        _dbContext.Entry(company).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CompanyIdExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok(company);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<ActionResult<Company>> DeleteCompany(int id)
    {
        if (_userInfo.IsAdmin)
        {
            var companyEntity = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == id);

            if (companyEntity == null)
            {
                return NotFound();
            }

            _dbContext.Company.Remove(companyEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(companyEntity);

        }
        else
        {
            return BadRequest();
        }

    }

    [HttpGet, Route("ReCalculateInverterSchedules"), Authorize]
    public async Task<ActionResult> ReCalculateInverterSchedules()
    {

        CompanyPlanCalculator companyPlanCalculator = new CompanyPlanCalculator((int)_userInfo.SelectedCompanyId!, _weatherApiComService);

        await companyPlanCalculator.CalculateInverter();     

        return Ok();
    }

    [HttpGet, Route("ReCalculateDeviceSchedules"), Authorize]
    public async Task<ActionResult> ReCalculateDeviceSchedules()
    {

        CompanyPlanCalculator companyPlanCalculator = new CompanyPlanCalculator((int)_userInfo.SelectedCompanyId!, _weatherApiComService);

        await companyPlanCalculator.CalculateDevice();

        return Ok();
    }

    [HttpGet, Route("ReCalculateSchedulesForAll"), Authorize]
    public async Task<ActionResult> ReCalculateSchedulesForAll()
    {
        CompanyPlanCalculator companyPlanCalculator = new CompanyPlanCalculator(_weatherApiComService);

        await companyPlanCalculator.CalculateForMissingInverter();
        await companyPlanCalculator.CalculateForMissingDevices();

        return Ok();
    }

    private async Task<bool> CompanyIdExists(long id)
    {
        return await _dbContext.Company?.AnyAsync(x => x.Id == id);
    }

    private async Task<bool> CompanyNameExists(string name)
    {
        return await _dbContext.Company?.AnyAsync(x => x.Name == name);
    }
}
