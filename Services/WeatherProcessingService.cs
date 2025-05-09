using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Models;
using System;

namespace SmartElectricityAPI.Services;

public class WeatherProcessingService
{
    private Company Company { get; set; }
    private Inverter Inverter { get; set; }
    private DateOnly Date { get; set; }
    private WeatherApiComService WeatherApiComService { get; set; }
    private List<SolarPanelCapacity> SolarPanelCapacity { get; set; }
    private List<CountrySolarCapacity> CountrySolarCapacity { get; set; }

    public WeatherProcessingService(Company company, Inverter inverter,
                                    DateOnly date, WeatherApiComService weatherApiComService,
                                    List<SolarPanelCapacity> solarPanelCapacity,
                                    List<CountrySolarCapacity> countrySolarCapacity)
    {
        Company = company;
        Date = date;
        WeatherApiComService = weatherApiComService;
        Inverter = inverter;
        SolarPanelCapacity = solarPanelCapacity;
        CountrySolarCapacity = countrySolarCapacity;
    }

    public async Task ProcessCurrenDateWeatherData()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var listOfForecastData = new List<WeatherForecastData>();

            DateTime currentDate = DateTime.Now;

            var currentDateData = await _dbContext.WeatherForecastData.Where(x => x.InverterId == Inverter.Id && x.Date == DateOnly.FromDateTime(currentDate)).ToListAsync();

            if (currentDateData == null || currentDateData.Count == 0)
            {
                var coordinates = Company.Latitude.ToString().Replace(",", ".") + "," + Company.Longitude.ToString().Replace(",", ".");

                try
                {
                    WeatherApiResponse WeatherApiResponse = await WeatherApiComService.GetWeatherData(coordinates, currentDate);

                    if (WeatherApiResponse != null
                        && WeatherApiResponse.Forecast.Forecastday.FirstOrDefault().Forecasthour.Count > 0)
                    {
                        foreach (var weatherRec in WeatherApiResponse.Forecast.Forecastday.FirstOrDefault().Forecasthour)
                        {
                            var solarPanelCapacity = SolarPanelCapacity.FirstOrDefault(x => x.Time == weatherRec.ForecastTime && x.SolarPanelsDirecation == Inverter.SolarPanelsDirecation);

                            if (solarPanelCapacity != null)
                            {
                                var weatherPercentage = CalcForecastWeatherPercentageV2(weatherRec);
                                listOfForecastData.Add(new WeatherForecastData
                                {
                                    InverterId = Inverter.Id,
                                    DateTime = weatherRec.DateTime,
                                    ConditionTxt = weatherRec.ConditionText,
                                    Humidity = weatherRec.Humidity,
                                    PrecipMM = weatherRec.PrecipMM,
                                    Uv = weatherRec.Uv,
                                    ChanceOfSnow = weatherRec.ChanceOfSnow,
                                    Date = weatherRec.ForecastDate,
                                    Time = weatherRec.ForecastTime,
                                    Cloud = weatherRec.Cloud,
                                    WeatherPercentage = Convert.ToInt32((weatherPercentage)),
                                    EstimatedSolarPower = Convert.ToInt32(CalcForecastWeatherResult(weatherPercentage, weatherRec))
                                });
                            }
                        }

                        if (listOfForecastData.Count > 0)
                        {
                            _dbContext.AddRange(listOfForecastData);
                            await _dbContext.SaveChangesAsync();
                        }

                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }

    public async Task<List<WeatherForecastData>> ProcessWeatherData()
    {
        using (var _dbContext = await new DatabaseService().CreateDbContextAsync())
        {
            var listOfForecastData = new List<WeatherForecastData>();


            if (await _dbContext.WeatherForecastData.AnyAsync(x => x.InverterId == Inverter.Id && x.Date == Date))
            {
                var existingRecords = await _dbContext.WeatherForecastData.Where(x => x.InverterId == Inverter.Id && x.Date == Date).ToListAsync();
                _dbContext.RemoveRange(existingRecords);
                await _dbContext.SaveChangesAsync();
            }
            var coordinates = Company.Latitude.ToString().Replace(",", ".") + "," + Company.Longitude.ToString().Replace(",", ".");

            try
            {
                WeatherApiResponse WeatherApiResponse = await WeatherApiComService.GetWeatherData(coordinates, DateTime.Now.AddDays(1));

                if (WeatherApiResponse != null
                    && WeatherApiResponse.Forecast.Forecastday.FirstOrDefault().Forecasthour.Count > 0)
                {


                    foreach (var weatherRec in WeatherApiResponse.Forecast.Forecastday.FirstOrDefault().Forecasthour)
                    {
                        var solarPanelCapacity = SolarPanelCapacity.FirstOrDefault(x => x.Time == weatherRec.ForecastTime && x.SolarPanelsDirecation == Inverter.SolarPanelsDirecation);

                        if (solarPanelCapacity != null)
                        {
                            var weatherPercentage = CalcForecastWeatherPercentageV2(weatherRec);
                            listOfForecastData.Add(new WeatherForecastData
                            {
                                InverterId = Inverter.Id,
                                DateTime = weatherRec.DateTime,
                                ConditionTxt = weatherRec.ConditionText,
                                Humidity = weatherRec.Humidity,
                                PrecipMM = weatherRec.PrecipMM,
                                Uv = weatherRec.Uv,
                                ChanceOfSnow = weatherRec.ChanceOfSnow,
                                Date = weatherRec.ForecastDate,
                                Time = weatherRec.ForecastTime,
                                Cloud = weatherRec.Cloud,
                                WeatherPercentage = Convert.ToInt32((weatherPercentage)),
                                EstimatedSolarPower = Convert.ToInt32(CalcForecastWeatherResult(weatherPercentage, weatherRec))
                            });
                        }
                    }

                    if (listOfForecastData.Count > 0)
                    {
                        _dbContext.AddRange(listOfForecastData);
                        await _dbContext.SaveChangesAsync();
                    }

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return listOfForecastData;
        }

    }

    private double CalcForecastWeatherPercentage(Forecasthour forecasthour)
    {
        double percentage = 0;

        if (forecasthour.Cloud > 0)
        {
            percentage = forecasthour.Cloud * 0.85;
        }
        return percentage;
    }

    private double CalcForecastWeatherPercentageV2(Forecasthour forecasthour)
    {
        double percentage = 0;
        double maxRainToGetSolar = 5;
        double rainProportion = 0.5;
        double cloudProportion = 0.5;
        double couldPercentage = forecasthour.Cloud;
        double rainPercentage = forecasthour.PrecipMM < maxRainToGetSolar ? forecasthour.PrecipMM / maxRainToGetSolar : 1;

        percentage = (rainPercentage * rainProportion) + (couldPercentage * cloudProportion);

        return percentage;
    }

    private double InverterPower()
    {

        if (Inverter.MaxPower > Inverter.SolarPanelsMaxPower)
        {
            return Inverter.SolarPanelsMaxPower * 1000;
        }
        else
        {
            return Inverter.MaxPower * 1000;
        }
    }

    private double CalcForecastWeatherResult(double weatherPercentage, Forecasthour forecasthour)
    {
        double weatherResult = 0;

        var solarPanelCapacity = SolarPanelCapacity.FirstOrDefault(x=> x.Time == forecasthour.ForecastTime && x.SolarPanelsDirecation == Inverter.SolarPanelsDirecation);

        var countrySolarCapacity = CountrySolarCapacity.FirstOrDefault(x => x.CountryId == Company.CountryId && x.Month == forecasthour.DateTime.Month);

        weatherResult = (((solarPanelCapacity.MaxPercentage * countrySolarCapacity.SolarCapacity)
            * (100 - weatherPercentage)) / 100) * InverterPower();    

        return weatherResult;
    }
}
