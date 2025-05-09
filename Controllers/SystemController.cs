using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.BackgroundServices;
using SmartElectricityAPI.Database;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    private readonly InverterHourService _brokerEngine;
    private readonly MySQLDBContext _dbContext;
    private DbContextOptions<MySQLDBContext> _dbContextOptions;

    public SystemController(InverterHourService brokerEngine, DbContextOptions<MySQLDBContext> dbContextOptions)
    {
        _brokerEngine = brokerEngine;
        _dbContextOptions = dbContextOptions;

    }
    [HttpPost,Route("Restart"), Authorize]
    public async Task<IActionResult> Restart()
    {
        await _brokerEngine.StopAsync(new CancellationToken());

        await _brokerEngine.StartAsync(new CancellationToken());

        return Ok("Service restarted.");
    }

    [HttpPost, Route("RefreshCustomersData"), Authorize]
    public async Task<IActionResult> RefreshCustomersData()
    {
        await _brokerEngine.RefreshCustomersData();

        return Ok();
    }
}
