using AutoMapper;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Middleware;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Company, CompanyDto>();
        CreateMap<InverterTypeCompanyActions, InverterTypeCompanyActionsDto>();
        // Add mappings for other models as needed
    }
}
