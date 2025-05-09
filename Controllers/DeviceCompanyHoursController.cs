using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Request;
using System.Globalization;

namespace SmartElectricityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceCompanyHoursController : ControllerBase
    {
        private MySQLDBContext _dbContext;
        private readonly ILogger<DeviceCompanyHoursController> _logger;
        private readonly IUserInfo _userInfo;

        public DeviceCompanyHoursController(MySQLDBContext context, ILogger<DeviceCompanyHoursController> logger, IUserInfo userInfo)
        {
            _dbContext = context;
            _logger = logger;
            _userInfo = userInfo;
        }


        [HttpGet("{deviceId}"), Authorize]
        public async Task<ActionResult> GetDeviceCompanyHours(int? deviceId, [FromQuery] string minDate, [FromQuery] string maxDate)
        {

            if (string.IsNullOrEmpty(minDate) || string.IsNullOrEmpty(maxDate) || deviceId == null)
            {
                return BadRequest("Device Id, minimum date and maximum date parameters are required.");
            }

            DateTime minDateTime;
            DateTime maxDateTime;

            if (!DateTime.TryParseExact(minDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out minDateTime) ||
                !DateTime.TryParseExact(maxDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out maxDateTime))
            {
                return BadRequest("Invalid date format. Please use 'yyyy-MM-dd'.");
            }

            if (minDateTime > maxDateTime)
            {
                return BadRequest("minimum ddate cannot be greater than maximum date.");
            }

            var companyDevice = await _dbContext.Device.Where(x => x.CompanyId == _userInfo.SelectedCompanyId && x.Id == deviceId).FirstOrDefaultAsync();

            if (companyDevice == null)
            {
                return NotFound();
            }      

            var deviceCompanyHours = await _dbContext.DeviceCompanyHours
                .Where(x => x.DeviceId == deviceId && x.CompanyId == _userInfo.SelectedCompanyId)
             
                .Include(x => x.SpotPrice)
                .Where(x => x.SpotPrice.Date >= DateOnly.FromDateTime(minDateTime) && x.SpotPrice.Date <= DateOnly.FromDateTime(maxDateTime))
                .OrderBy(x => x.SpotPrice.DateTime)
                .ToListAsync();


            if (deviceCompanyHours == null || deviceCompanyHours.Count == 0)
            {
                return NotFound();
            }

            return Ok(deviceCompanyHours);
        }

        [HttpPut("{deviceId}"), Authorize]
        public async Task<IActionResult> UpdateDeviceCompanyHours(int deviceId, [FromBody] UpdateDeviceCompanyHoursRequest request)
        {
            var selectedRecord = await _dbContext.DeviceCompanyHours
                .Where(x => x.DeviceId == deviceId && x.CompanyId == _userInfo.SelectedCompanyId && x.Id == request.Id)
                .FirstOrDefaultAsync();

            if (selectedRecord == null)
            {
                return NotFound();
            }

            selectedRecord.DeviceActionType = request.DeviceActionType;

            _dbContext.Entry(selectedRecord).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(selectedRecord);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest();

            }
        }
    }
}
