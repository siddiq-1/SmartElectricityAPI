namespace SmartElectricityAPI.Models
{
    public class CompanyProfit
    {
        public int Month { get; set; }
        public double SumCostOfConsumpWithOutMygGid { get; set; }
        public double SumCostPurchaseMinusSellFromGrid { get; set; }
        public double SumWinOrLoseFromMyGridUsage { get; set; }
    }
}
