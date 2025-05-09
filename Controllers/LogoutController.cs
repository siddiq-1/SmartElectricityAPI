using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class LogoutController : ControllerBase
{
    private MySQLDBContext _dbContext;
    public LogoutController(MySQLDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue("jwt", out string refreshToken))
        {
            User user = _dbContext.User.FirstOrDefault(x => x.RefreshToken == refreshToken);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.None,
            };

            if (user == null)
            {
                Response.Cookies.Delete("jwt", cookieOptions);
                return NoContent();
            }

            user.RefreshToken = null;
            _dbContext.SaveChanges();

            Response.Cookies.Delete("jwt", cookieOptions);
            return NoContent();

        }

        return NoContent();
    }
}
