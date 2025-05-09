using SmartElectricityAPI.Database;

namespace SmartElectricityAPI.Services;

public class UserService
{
    private readonly MySQLDBContext _dbContext;
    public UserService(MySQLDBContext context)
	{
        _dbContext = context;
    }
    /*
    public List<UserDto> GetUsersWithCompanies()
    {
      
        var result = (from u in _dbContext.User
                      join cu in _dbContext.CompanyUsers on u.Id equals cu.UserId into allIncluded
                      from subAllIncluded in allIncluded.DefaultIfEmpty()
                      join c in _dbContext.Company on subAllIncluded.CompanyId equals c.Id into allIncluded2
                      from subAllIncluded2 in allIncluded2.DefaultIfEmpty()
                      group subAllIncluded.CompanyId by new
                      {
                          u.Id,
                          u.Email,
                          u.Username,
                          u.PermissionId
               
                      } into g
                      select new UserDto
                      {
                          Id = g.Key.Id,
                          Email = g.Key.Email,
                          Username = g.Key.Username,
                          IsAdmin = g.Key.I,
                          Permission = _dbContext.Permission.Where(x => x.Id == g.Key.PermissionId).ToList(),
                          CompanyIds = g.Where(x=> x > 0).Select(x => new CompanyDto
                          {
                              CompanyId = x,
                              CompanyName = _dbContext.Company.FirstOrDefault(y => y.Id == x)!.Name
                          }
                          ).ToList() 

                      }).ToList();


        return result;
    }
    */

}
