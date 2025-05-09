using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Interfaces;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static SmartElectricityAPI.Services.Constants;

namespace SmartElectricityAPI.Middleware;
// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class CustomAuthMiddleware
{
    private readonly RequestDelegate _next;

    public CustomAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, IUserInfo userInfo, MySQLDBContext dbContext, IMemoryCache memoryCache)
    {

        string? authHeader = httpContext.Request.Headers["Authorization"];

        if (authHeader != null && authHeader.StartsWith("Bearer"))
        {
            string token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var companyId = httpContext.User.Claims.Where(x => x.Type == Constants.CompanyId).FirstOrDefault();
                var companies = Array.ConvertAll(companyId.Value.Split(','), int.Parse).ToList();
             //   int userPermissionId = int.Parse(httpContext.User.Claims.Where(x => x.Type == Constants.UserPermissionId).FirstOrDefault().Value);

                List<Permission> permissions = new List<Permission>();
                memoryCache.TryGetValue(CacheKeys.DbPermissions, out permissions!);

                if (permissions == null)
                {
                    memoryCache.Set(CacheKeys.DbPermissions, await dbContext.Permission.ToListAsync());
                    memoryCache.TryGetValue(CacheKeys.DbPermissions, out permissions!);
                }

                userInfo.Companies = companies;
                userInfo.UserLevel = permissions.FirstOrDefault(permissions => permissions.Level.ToString() == httpContext.User.Claims.Where(x => x.Type == Constants.UserPermission).FirstOrDefault()!.Value).Level;
                //   userInfo.UserLevel = permissions!.Where(x=> x.Id == userPermissionId).FirstOrDefault()!.Level;
                userInfo.Id = int.Parse(httpContext.User.Claims.Where(x => x.Type == Constants.UserId).FirstOrDefault()!.Value);
                userInfo.IsAdmin   = bool.Parse(httpContext.User.Claims.Where(x => x.Type == Constants.IsAdmin).FirstOrDefault()!.Value);
                userInfo.SelectedCompanyId = int.Parse(httpContext.User.Claims.Where(x => x.Type == Constants.SelectedCompanyId).FirstOrDefault()!.Value);
            }
            catch
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
        }

        await _next(httpContext);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class CustomAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomAuthMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomAuthMiddleware>();
    }
}
