using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models.ThirdParty;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SmartElectricityAPI.Models;

public class BatteryControlHours : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int InverterBatteryId { get; set; }
    public InverterBattery InverterBattery { get; set; }
    [NotMapped]
    public SpotPrice? SpotPriceMin { get; set; }
    public SpotPrice SpotPriceMax { get; set; }
    public int SpotPriceMaxId { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan? MinPriceHour { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? MinPriceWithCost { get; set; }
    [Column(TypeName = "time(0)")]
    public TimeSpan? MaxPriceHour { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? MaxPriceWithCost { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? MaxMinPriceDifference { get; set; }
    public int? MinChargingPowerWhOriginal { get; set; }
    public int? MinChargingPowerWh { get; set; }
    public int? MinChargingPowerWhOriginalDiff { get; set; }
    public int? MaxAvgHourlyConsumptionOriginal { get; set; }
    public int? MaxAvgHourlyConsumption { get; set; }
    public int? WaveNumber { get; set; }
    public int? UsableWatts { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? UsableWattsProfit { get; set; }
    public int? AmountCharged { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? LineProfit { get; set; }
    [Column(TypeName = "decimal(10, 4)")]
    public double? HourProfit { get; set; }
    public int Rank { get; set; }
    public int GroupNumber { get; set; }

    public int? MaxAvgHourlyConsumptionOriginalDiff { get; set; }
    public ActionTypeCommand ActionTypeCommand { get; set; }
    public bool IsProcessed { get; set; } = false;

    [NotMapped]
    public ActionTypeCommand ActionTypeCommandOriginal { get; set; }
    [NotMapped]
    public ActionTypeCommand ActionTypeCommandFromWinter { get; set; }
    [NotMapped]
    public double RemainingBattery { get; set; }
    [NotMapped]
    public double PurchasePrice { get; set; }
    [NotMapped]
    public double SalePrice { get; set; }
    [NotMapped]
    public int ForecastedConsumption { get; set; }
    [NotMapped]
    public int ForecastedSolar { get; set; }
    [NotMapped]
    public int ForecastedSolarRemaining { get; set; }
    [NotMapped]
    public bool IsChanged { get; set; } = false;

    [NotMapped]
    public double ConsumeBatterySellPower { get; set; }
    [NotMapped]
    public bool IsForInitialChargeWithRemainingSun { get; set; } = false;
    [NotMapped]
    public bool SummerMediumUndo { get; set; } = false;
    [NotMapped]
    public bool ConsumeMaxProcessedForSummerMedium { get; set; } = false;

    [NotMapped]
    public bool IsNeededForSummerMaxCalculation { get; set; } = false;
    [NotMapped]
    public double SelfUseConsumeCombinedMaxPrice { get; set; }

    [NotMapped]
    public bool IsProcessedInCombine { get; set; } = false;

    [NotMapped]
    public bool IsChargeMaxRemovedAttempted { get; set; } = false;

}
