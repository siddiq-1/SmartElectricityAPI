using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Models;

namespace SmartElectricityAPI.Helpers;

public class InverterHelper
{
    public static double SofarThreePhaseAntiRefluxPayload(double maxPower) => maxPower * 10;
}
