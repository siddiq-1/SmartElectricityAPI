using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Extensions;
using SmartElectricityAPI.Helpers;

namespace SmartElectricityAPI.Engine.Helpers;

public static class PriceCostHelper
{
    public static double? CalculatePriceWithSalesMarginCosts(SpotPrice spotPrice, Company company)
    {
        if (spotPrice == null || company == null)
        {
            return null;
        }

        double networkFree;

        if (DateTimeHelper.IsWeekend(spotPrice.Date) && company.UseNightTimeFeeOnSaturdayAndSunday)
        {
            networkFree = company.NetworkServiceFeeNightTime;
        }
        else
        {
            if (spotPrice.Time.Between((TimeSpan)company.DayStartTime, (TimeSpan)company.DayEndTime))
            {
                networkFree = company.NetworkServiceFeeDayTime;
            }
            else
            {
                networkFree = company.NetworkServiceFeeNightTime;
            }
        }
     

        return spotPrice.PriceNoTax + networkFree + company.BrokerSalesMargin;
    }

    public static double? CalculateMaxPriceWithPurchaseMarginCosts(SpotPrice spotPrice, Company company)
    {
        if (spotPrice == null || company == null)
        {
            return null;
        }

        return spotPrice.PriceNoTax - company.BrokerPurchaseMargin;
    }
}
