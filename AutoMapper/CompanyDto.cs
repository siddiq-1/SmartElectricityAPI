using SmartElectricityAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.AutoMapper;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public double NetworkServiceFeeNightTime { get; set; }
    public double NetworkServiceFeeDayTime { get; set; }
    public double BrokerSalesMargin { get; set; } //Elektrimüüja müügi marginaal
    public double BrokerPurchaseMargin { get; set; } //Elektri müüja ostumarginaal
    public bool UseNightTimeFreeOnSaturdayAndSunday { get; set; }
    public double? ExpectedProfitFromChargeAndSellPriceInCents { get; set; }
    public double? ExpectedProfitForSelfUseOnlyInCents { get; set; }
    public int CountryId { get; set; }
    public Country? Country { get; set; }
    public int RegionId { get; set; }
    public Region? Region { get; set; }

    public TimeSpan? DayStartTime { get; set; }

    public TimeSpan? DayEndTime { get; set; }

    public TimeSpan? NightStartTime { get; set; }
    public TimeSpan? NightEndTime { get; set; }
}
