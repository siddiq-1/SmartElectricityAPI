using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Request;

public class UpdateDeviceCompanyHoursRequest
{
    public int Id { get; set; }
    public DeviceActionType DeviceActionType { get; set; }
}
