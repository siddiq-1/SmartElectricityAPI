using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Services
{
    public class TokenService
    {
        public string CreateAccessToken(User user, List<CompanyUsers> companyUsers)
        {
            var expiration = DateTime.Now.AddMinutes(Constants.AccessTokenExpireInMinutes);
            var token = CreateJwtToken(
                CreateClaims(user, companyUsers),
                CreateSigningCredentialsAccessToken(),
                expiration
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public string CreateRefreshToken(User user, List<CompanyUsers> companyUsers)
        {
            var expiration = DateTime.Now.AddMinutes(Constants.RefreshTokenExpireInMinutes);
            var token = CreateJwtToken(
                CreateClaims(user, companyUsers),
                CreateSigningCredentialsRefreshToken(),
                expiration
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
            DateTime expiration) =>
            new(
                "TarkElekter",
                "TarkElekterCustomers",
                claims,
                expires: expiration,
                signingCredentials: credentials
            );

        private List<Claim> CreateClaims(User user, List<CompanyUsers> companyUsers)
        {
            var stes = companyUsers.Where(x => x.UserId == user.Id).Select(s => s.CompanyId).ToArray();
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(Constants.CompanyId, string.Join(",",companyUsers.Where(x=> x.UserId == user.Id).Select(s=> s.CompanyId).ToArray())),
                    new Claim(Constants.UserId, user.Id.ToString()),
                    new Claim(Constants.IsAdmin, user.IsAdmin.ToString()),
                    new Claim(Constants.SelectedCompanyId, user.SelectedCompanyId.ToString()!),
                    new Claim(Constants.UserPermission, companyUsers.FirstOrDefault(x=> x.CompanyId == user.SelectedCompanyId && x.UserId == user.Id).Permission.Level.ToString()),

                };
                return claims;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private SigningCredentials CreateSigningCredentialsAccessToken()
        {
            return new SigningCredentials(
                new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(Constants.Tokens.AccessTokenSecret)
                ),
                SecurityAlgorithms.HmacSha256
            );
        }

        private SigningCredentials CreateSigningCredentialsRefreshToken()
        {
            return new SigningCredentials(
                new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(Constants.Tokens.RefreshTokenSecret)
                ),
                SecurityAlgorithms.HmacSha256
            );
        }

        public bool IsTokenExpired(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Read and parse the token
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Check if the token's expiration claim exists and is valid
                if (jwtToken.ValidTo != null)
                {
                    // Get the expiration date and time
                    DateTime expirationDate = jwtToken.ValidTo.ToLocalTime();

                    // Check if the token is expired
                    if (expirationDate < DateTime.Now)
                    {
                        return true; // Token is expired
                    }
                }

                return false; // Token is not expired
            }
            catch (Exception)
            {
                // An exception occurred while parsing the token, which might indicate a malformed token
                // You can handle this as per your application's requirements
                return true;
            }
        }
    }
}
