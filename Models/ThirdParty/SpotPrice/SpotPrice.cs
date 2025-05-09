using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Enums;

namespace SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

[Microsoft.EntityFrameworkCore.Index(nameof(DateTime), IsUnique = true)]
public class SpotPrice: BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    private DateTime _dateTime;
    [Column(TypeName = "datetime(0)")]
    public DateTime DateTime
    {
        get => this._dateTime;

        set
        {
            DateTime dateTime = value;

            this._dateTime = dateTime; // dateTime.AddHours(TimeZoneInfo.Local.GetUtcOffset(dateTime).TotalHours);
        }
    }
    private DateOnly _date;
    public DateOnly Date
    {
        get => DateOnly.FromDateTime(_dateTime);

        set
        {
            _date = value;
        }
    }

    private TimeSpan _time;
    public TimeSpan Time
    {
        get => DateTime.TimeOfDay;

        set
        {
            _time = value;
        }
    }

    public int Rank { get; set; }
    public double PriceNoTax { get; set; }

    [NotMapped]
    public double ParentPriceNoTax
    {
        get
        {
            if (ParentRecord != null)
            {
                return ParentRecord.PriceNoTax;
            }
            return 0;
        }
    }
    public double PriceWithTax { get; set; }
    [NotMapped]
    public int SequenceNumber { get; set; }
    [NotMapped]
    public SpotPrice ParentRecord { get; set; }

    [NotMapped]
    public double PriceDifferenceWithParent
    {
        get
        {
            if (ParentRecord != null)
            {
                return ParentRecord.PriceNoTax - PriceNoTax;
            }
            return 0;
        }
    }

    public int RegionId { get; set; }
    public Region Region { get; set; }
    
    public Company? Company { get; set; }
    public int? CompanyId { get; set; }

    [NotMapped]
    public string GroupId { get; set; }
    [NotMapped]
    public int GroupNumber { get; set; }

    [NotMapped]
    public bool IsParent { get; set; } = false;
    
    [NotMapped]
    public double PercentageFall { get;set; }

    [NotMapped]
    public double AmountFall { get; set; }
    [NotMapped]
    public double TotalProfitEarning { get; set; }

    [NotMapped]
    public double GroupProfit { get; set; }

    [NotMapped]
    public bool IsLastInGroup { get; set; } = false;
    [NotMapped]
    public DeviceActionType deviceActionType { get; set; }

    [NotMapped]
    [Column(TypeName = "decimal(10, 4)")]
    public double? CostWithSalesMargin { get; set; }

    public void Update(double averagePriceNoTax, int maxRank)
    {
        this.PriceNoTax = averagePriceNoTax;
        this.Rank = maxRank;
    }
}
