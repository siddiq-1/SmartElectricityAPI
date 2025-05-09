using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Request.Auth;
using SmartElectricityAPI.Response;
using SmartElectricityAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using SmartElectricityAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartElectricityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private TokenService _tokenService;
        private MySQLDBContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly InverterApiService _inverterApiService;

        public AuthController(TokenService tokenService, MySQLDBContext context, IMemoryCache memoryCache, InverterApiService inverterApiService)
        {
            _tokenService = tokenService;
            _dbContext = context;
            _memoryCache = memoryCache;
            _inverterApiService = inverterApiService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            User user = await _dbContext.User.Where(x=> x.Email == request.Email).FirstOrDefaultAsync()!;

            if (user == null)
            {
                return Unauthorized();
            }


            //if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            //{
            //    return Unauthorized();
            //}

            _memoryCache.Set(Constants.CacheKeys.DbPermissions, await _dbContext.Permission.ToListAsync());

            var companyUsers = await _dbContext.CompanyUsers.Include(i=> i.Permission).ToListAsync();

            var accessToken = _tokenService.CreateAccessToken(user, companyUsers);

            var refreshToken = _tokenService.CreateRefreshToken(user, companyUsers);

            user.RefreshToken = refreshToken;

            await _dbContext.SaveChangesAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(7),
            };

            Response.Cookies.Append("jwt", refreshToken, cookieOptions);



            return Ok(new AuthResponse
            {
                Username = user.Username,
                Email = user.Email,
                Token = accessToken,
            });
        }
    }
}
