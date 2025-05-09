using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.DBQueries;

public class InverterQry
{
    private MySQLDBContext _dbContext;

    public InverterQry(MySQLDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RegisteredInverterTopicDto>> GetRegisteredInventerTopics()
    {
        var result = from inv in _dbContext.Inverter
                     join itlt in _dbContext.InverterTypeListenTopics on inv.InverterTypeId equals itlt.InverterTypeId
                     join regIn in _dbContext.RegisteredInverter on inv.RegisteredInverterId equals regIn.Id
                     select new RegisteredInverterTopicDto
                     {
                         RegisteredInverterId = regIn.Id,
                         RegisteredInverterName = regIn.Name,
                         TopicName = itlt.TopicName,
                         CompanyId = inv.CompanyId,
                         RegisteredInverterAndTopic = $"{regIn.Name}{itlt.TopicName}"
                     };

        return await result.ToListAsync();
    }

    public async Task<List<Inverter>> GetInvertersWithRegisteredInverter()
    {
        return await _dbContext.Inverter.Include(x=> x.RegisteredInverter).ToListAsync();
    }

}
