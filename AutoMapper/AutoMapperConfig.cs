using AutoMapper;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.AutoMapper;

public static class AutoMapperConfig
{
    public static IMapper InitializeAutoMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Add all your AutoMapper profiles here
            cfg.CreateMap<User, UserDto>();
            cfg.CreateMap<Company, CompanyDto>();
            cfg.CreateMap<SpotPrice, BatteryHoursCalculatorDto>();
            cfg.CreateMap<InverterTypeCompanyActions, InverterTypeCompanyActionsDto>();
            // ... Add other mappings as needed ...
        });

        return config.CreateMapper();
    }
}
