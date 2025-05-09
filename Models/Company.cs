
using SmartElectricityAPI.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SmartElectricityAPI.Models;

public class Company : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string Address { get; set; }
    public double NetworkServiceFeeNightTime { get; set; }
    public double NetworkServiceFeeDayTime { get; set; }
    public double BrokerSalesMargin { get; set; } //Elektrimüüja müügi marginaal
    public double BrokerPurchaseMargin { get; set; } //Elektri müüja ostumarginaal
    public bool UseNightTimeFeeOnSaturdayAndSunday { get; set; }
    public double? ExpectedProfitForSelfUseOnlyInCents { get; set; } 
    public int CountryId { get; set; }
    public Country? Country { get; set; }
    public int RegionId { get; set; }
    public Region? Region { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan? DayStartTime { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan? DayEndTime { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan? NightStartTime { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan? NightEndTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool UseFixedPrices { get; set; } = false;
    public double? DeviceMinProfitInCents { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsValid()
    {

        // Check for null values
        if (DayStartTime == null || DayEndTime == null || NightStartTime == null || NightEndTime == null)
        {
            return false;
        }

        if (!this.IsDayStartTimeBeforeEndTime())
        {
            return false;
        }
        if (!this.IsNightStartTimeBeforeEndTime())
        {
            return false;
        }

        if (DayStartTime < NightEndTime)
        {
            return false;
        }

        if (NightStartTime < DayEndTime)
        {
            return false;
        }
   

        return true;
    }

    private bool IsDayStartTimeBeforeEndTime()
    {
        // If start time is less than end time, it's the same day
        // If start time is greater, then end time is the next day
        return DayStartTime <= DayEndTime || (DayStartTime > DayEndTime && DayEndTime < TimeSpan.FromHours(12));
    }

    private bool IsNightStartTimeBeforeEndTime()
    {
        // If start time is less than end time, it's the same day
        // If start time is greater, then end time is the next day
        return NightStartTime <= NightEndTime || (NightStartTime > NightEndTime && NightEndTime < TimeSpan.FromHours(12));
    }



}
