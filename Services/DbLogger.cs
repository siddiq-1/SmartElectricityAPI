using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Services;

public class DbLogger
{
    public static async Task PostLog(string level, string message, string origin = null)
    {

        MySQLDBContext _dbContext = await new DatabaseService().CreateDbContextAsync();

        _dbContext.Log.Add(new Log
        {
            Level = level,
            Message = message,
            Origin = origin
        });

        await _dbContext.SaveChangesAsync();
    }
}
