using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ViewModel;
using SmartElectricityAPI.Response;
using SmartElectricityAPI.Services;
using System.Text.RegularExpressions;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly ILogger<UserController> _logger;
    private readonly IUserInfo _userInfo;
    private readonly UserService _userService;

    public UserController(MySQLDBContext context, ILogger<UserController> logger, IUserInfo userInfo, UserService userService)
    {
        _dbContext = context;
        _logger = logger;
        _userInfo = userInfo;
        _userService = userService;
    }
    [HttpGet, Authorize]
    public async Task<ActionResult<User>> GetUsers(int? companyId)
    {

        if (_userInfo.IsAdmin)
        {
            return Ok(_dbContext.User.OrderByDescending(x => x.Id).ToList());
        }

        if (companyId != null && !_userInfo.Companies.Any(x => x == companyId))
        {
            return BadRequest();
        }

        if (companyId != null && _userInfo.Companies.Any(x=> x == companyId))
        {
            var result = (from u in _dbContext.User
                          join cu in _dbContext.CompanyUsers
                          on u.Id equals cu.UserId
                          where cu.CompanyId == companyId
                          select new User
                          {
                              Id = u.Id,
                              Email = u.Email,
                              Username = u.Username,
                             // Permission = u.Permission,
                            //  PermissionId = u.PermissionId,

                          }).ToList();

            return Ok(result);
        }

        if (_userInfo.Companies.Count == 1)
        {
            var result = (from u in _dbContext.User
                       join cu in _dbContext.CompanyUsers
                       on u.Id equals cu.UserId
                       where cu.CompanyId == _userInfo.Companies.FirstOrDefault()
                       select new User
                       {
                           Id = u.Id,
                           Email = u.Email,
                           Username = u.Username,
                         //  Permission = u.Permission,
                         //  PermissionId = u.PermissionId,

                       }).ToList();

            return Ok(result);
        }
        else
        {
            var result = (from u in _dbContext.User
                          join cu in _dbContext.CompanyUsers
                          on u.Id equals cu.UserId
                          join c in _dbContext.Company on cu.CompanyId equals c.Id
                          where _userInfo.Companies.Any(x => x == cu.CompanyId)
                          group cu.CompanyId by new
                          {
                              u.Id,
                              u.Email,
                              u.Username,
                             // u.PermissionId
                          } into g
                          select new
                          {
                              Id = g.Key.Id,
                              Email = g.Key.Email,
                              Username = g.Key.Username,
                             // PermissionId = g.Key.PermissionId,
                              CompanyIds = g.Select(x => new { CompanyId = x, CompanyName = _dbContext.Company.FirstOrDefault(y => y.Id == x)!.Name }).ToList()
                          }).ToList();

            return Ok(result);

        }
    }

    [HttpGet("{id}"), Authorize]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        //TODO: Validation of who can see what info
        if (_dbContext.User == null)
        {
            return NotFound();
        }

        if (_userInfo.IsAdmin)
        {
            var user = await _dbContext.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Password = null;
            user.RefreshToken = null;

            return user;
        }

        return BadRequest();

    }

    [HttpPost, Authorize]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        if (_userInfo.IsAdmin)
        {
            if (EmailExists(user.Email))
            {
                return Conflict("E-mail already exists.");
            }
            var mqttPassword = user.Password;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _dbContext.User.Add(user);
            try
            {
                await _dbContext.SaveChangesAsync();

                var mqttUser = new MqttUsers
                {
                    IsClientIdRequired = false,
                    Username = RemoveSpacesAndReplaceAccents(user.Username),
                    Password = mqttPassword,
                    ClientId = user.ClientId
                };

                _dbContext.MqttUsers.Add(mqttUser);

                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new ExceptionMessageContent()
                {
                    Error = "Bad Request",
                    Message = ex.InnerException!.Message
                });
            }
        }

        return BadRequest();
 
    }

    [HttpGet, Route("MqttUsers"), Authorize]
    public async Task<ActionResult> GetMqttUsers()
    {
        if (_userInfo.IsAdmin)
        {
            var mqttUsers = _dbContext.MqttUsers.ToList();

            if (mqttUsers != null && mqttUsers.Count > 0)
                return Ok(mqttUsers);
            else
                return NotFound();
        }

        return BadRequest();


    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (_dbContext.User.Any(x => x.Email == model.Email && x.Id != id))
        {
            return BadRequest("User with that email already exists.");
        }
        var user = _dbContext.User.Find(id);

        user!.Username = model.Username;
        user.Email = model.Email;
        user.IsAdmin = model.IsAdmin;
        user.ClientId = model.ClientId;

        if (!string.IsNullOrEmpty(model.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var existingMqttUser = _dbContext.MqttUsers.FirstOrDefault(x => x.Username == RemoveSpacesAndReplaceAccents(model.Username));

            if (existingMqttUser == null)
            {
                var mqttUser = new MqttUsers
                {
                    IsClientIdRequired = false,
                    Username = RemoveSpacesAndReplaceAccents(model.Username),
                    Password = model.Password,
                    ClientId = model.ClientId
                };

                _dbContext.MqttUsers.Add(mqttUser);
            }
            else
            {
                existingMqttUser.Username = RemoveSpacesAndReplaceAccents(model.Username);
                existingMqttUser.Password = model.Password;
                existingMqttUser.ClientId = model.ClientId;
            }            

            await _dbContext.SaveChangesAsync();
        }


        _dbContext.Entry(user).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserIdExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok(user);
    }

    [HttpPut, Route("UserSelectedCompany"), Authorize]
    public async Task<IActionResult> UpdateUserSelectedCompany([FromBody] UpdateUserViewModel model)
    {

        var user = _dbContext.User.Find(_userInfo.Id);

        user.SelectedCompanyId = model.SelectedCompanyId;


        _dbContext.Entry(user).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {

        }

        return Ok(user);
    }

    private bool EmailExists(string email)
    {
        return (_dbContext.User?.Any(x => x.Email == email)).GetValueOrDefault();
    }

    private bool UserIdExists(long id)
    {
        return (_dbContext.User?.Any(x => x.Id == id)).GetValueOrDefault();
    }

    private string RemoveSpacesAndReplaceAccents(string input)
    {
        input = Regex.Replace(input, "[ä]", "a");
        input = Regex.Replace(input, "[ü]", "u");
        input = Regex.Replace(input, "[õ]", "o");
        input = Regex.Replace(input, "[ö]", "o");
        input = Regex.Replace(input, "[š]", "s");
        input = Regex.Replace(input, "[ž]", "z");
        input = Regex.Replace(input, "[Ä]", "A");
        input = Regex.Replace(input, "[Ü]", "U");
        input = Regex.Replace(input, "[Õ]", "O");
        input = Regex.Replace(input, "[Ö]", "O");
        input = Regex.Replace(input, "[Š]", "S");
        input = Regex.Replace(input, "[Ž]", "Z");

        input = input.Replace(" ", "");

        return input;
    }


}
