using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using System;

namespace SmartElectricityAPI.Models;

public class BatteryHoursSummer
{
    public DateTime DateTime { get; set; }
    public int AvgConsumption { get; set; }
    public int ForecastedConsumption { get; set; }
    public int EstimatedSolar { get; set; }
    public double PurchasePrice { get; set; }
    public double SalePrice { get; set; }
    public int ForecastedSolarRemaining { get; set; }
    public SpotPrice SpotPrice { get; set; }
    public double RemainingBattery { get; set; }
    public ActionTypeCommand ActionTypeCommand { get; set; }

    private TimeSpan _time;
    public TimeSpan Time
    {
        get => DateTime.TimeOfDay;

        set
        {
            _time = value;
        }
    }

    private DateOnly _date;
    public DateOnly Date
    {
        get => DateOnly.FromDateTime(DateTime);

        set
        {
            _date = value;
        }
    }

}
