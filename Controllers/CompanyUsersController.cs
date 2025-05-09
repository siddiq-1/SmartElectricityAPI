using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Models.ViewModel;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CompanyUsersController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<CompanyUsersController> _logger;
    private readonly IUserInfo _userInfo;
    public CompanyUsersController(MySQLDBContext context, ILogger<CompanyUsersController> logger, IUserInfo userInfo)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
    }
    [HttpGet]
    public async Task<IEnumerable<CompanyUsers>> GetCompanyUsers()
    {
        return await _dbContext.CompanyUsers.Include(i=> i.Company).Where(x => x.UserId == _userInfo.Id).ToListAsync();
    }

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult> GetCompanyUsersForCompanyId(int id)
    {
        if (_userInfo.IsAdmin)
        {
            var companyUsers = await _dbContext.CompanyUsers
            .Where(x => x.CompanyId == id)
            .Select(cu => new
            {
                cu.Id,
                User = new
                {
                    cu.User.Id,
                    cu.User.Username,
                    cu.User.Email
                },
                cu.Permission // Pulling the whole Permission entity
            })
            .ToListAsync();

            return Ok(companyUsers);
         }

        return BadRequest();
     
    }

    [HttpPost, Authorize]
    public async Task<ActionResult> PostCompanyUser([FromBody] AddCompanyUserViewModel addCompanyUserViewModel)
    {
        if (_userInfo.IsAdmin)
        {

            if (!await _dbContext.User.AnyAsync(x => x.Email == addCompanyUserViewModel.Email))
            {
                return BadRequest("User with e-mail address does not exist.");
            }

            var user = await _dbContext.User.Where(x => x.Email == addCompanyUserViewModel.Email).FirstOrDefaultAsync();

            if (await _dbContext.CompanyUsers.AnyAsync(x => x.CompanyId == addCompanyUserViewModel.CompanyId && x.UserId == user.Id))
            {
                return BadRequest("User already exists in company permissions.");
            }

               _dbContext.CompanyUsers.Add(new CompanyUsers { CompanyId = addCompanyUserViewModel.CompanyId,
                                                              PermissionId = addCompanyUserViewModel.PermissionId,
                                                              UserId = user.Id});


            var addedCompanyUser = await _dbContext.User.FirstOrDefaultAsync(x => x.Id == user.Id);

            if (addedCompanyUser!.SelectedCompanyId == null)
            {
                _dbContext.Entry(addedCompanyUser).State = EntityState.Modified;
                addedCompanyUser.SelectedCompanyId = addCompanyUserViewModel.CompanyId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        return BadRequest();
    }

    [HttpPut, Authorize]
    public async Task<ActionResult> PutCompanyUser([FromBody] AddCompanyUserViewModel addCompanyUserViewModel)
    {
        if (_userInfo.IsAdmin)
        {
            if (!await _dbContext.User.AnyAsync(x => x.Email == addCompanyUserViewModel.Email))
            {
                return BadRequest("User with e-mail address does not exist.");
            }

            var companyUser = await _dbContext.CompanyUsers.Where(x => x.Id == addCompanyUserViewModel.Id).FirstOrDefaultAsync();

            companyUser.PermissionId = addCompanyUserViewModel.PermissionId;


            _dbContext.Entry(companyUser).State = EntityState.Modified;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        return BadRequest();
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<ActionResult> DeleteCompanyUser(int id)
    {
        if (_userInfo.IsAdmin)
        {

            var companyUser = await _dbContext.CompanyUsers.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (companyUser != null)
            {
                _dbContext.CompanyUsers.Remove(companyUser);
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }

        return BadRequest();
    }

}
