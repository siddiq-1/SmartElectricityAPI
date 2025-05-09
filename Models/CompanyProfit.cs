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
        public int Rank { get; set; }
        public int Month { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public double Capacity { get; set; }
        public double SumWinOrLoseFromMyGridUsage { get; set; }
        public double ProfitPerKwh { get; set; } = 0.0d;
        public bool IsLoggedInUser { get; set; }
    }
}
