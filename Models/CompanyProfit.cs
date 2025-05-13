using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace SmartElectricityAPI.Models
{
    public class CompanyProfit
    {
        public int Month { get; set; }
        public double SumCostOfConsumpWithOutMygGid { get; set; }
        public double SumCostPurchaseMinusSellFromGrid { get; set; }
        public double SumWinOrLoseFromMyGridUsage { get; set; }
    }
    public class CompanyProfitByKwh
    {
        public int Position { get; set; }
        public string CompanyName { get; set; }
        public double ProfitByKWh { get; set; } = 0.0d;
    }
}
