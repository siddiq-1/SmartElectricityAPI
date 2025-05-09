using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SwitchModelController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;

    public SwitchModelController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }

    [HttpGet, Authorize]
    public async Task<ActionResult> GetSwitchModels()
    {
        return Ok(_dbContext.SwitchModel.ToList());
    }
}
