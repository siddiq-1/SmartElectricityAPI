using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;

namespace SmartElectricityAPI.Services;

public class DatabaseService
{
    public async Task<MySQLDBContext> CreateDbContextAsync()
    {
        var builder = WebApplication.CreateBuilder();
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var dbContextOptions = new DbContextOptionsBuilder<MySQLDBContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return await Task.FromResult(new MySQLDBContext(dbContextOptions));
    }
}
