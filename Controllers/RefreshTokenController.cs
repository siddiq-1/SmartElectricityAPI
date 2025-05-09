using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Response;
using SmartElectricityAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RefreshTokenController : ControllerBase
{
    private MySQLDBContext _dbContext;
    private TokenService _tokenService;
    public RefreshTokenController(MySQLDBContext dbContext, TokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    [HttpGet] 
    public async Task<IActionResult> Validate()
    {
        if (Request.Cookies.TryGetValue("jwt", out string refreshToken))
        {
           User user = _dbContext.User.FirstOrDefault(x => x.RefreshToken == refreshToken);

            if (user == null)
            {
                return Forbid();
            }

            if (_tokenService.IsTokenExpired(refreshToken))
            {
                return Forbid();
            }


            var validateToken = new JwtSecurityTokenHandler().ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Constants.Tokens.RefreshTokenSecret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);



            if (validatedToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var companyUsers = await _dbContext.CompanyUsers.Include(i=> i.Permission).ToListAsync();

                var accessToken = _tokenService.CreateAccessToken(user, companyUsers);

                return Ok(new AuthResponse
                {
                    Username = user.Username,
                    Email = user.Email,
                    Token = accessToken,
                });
            }

            return Forbid();
                // Cookie found, do something with the value
              //  return Ok("Cookie value: " + refreshToken);
        }



        return Forbid();
    }
}
