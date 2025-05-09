using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using SmartElectricityAPI.AutoMapper;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Engine.Helpers;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Helpers;
using SmartElectricityAPI.Infrastructure;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Services;
using SmartElectricityAPI.Utils;


namespace SmartElectricityAPI.Engine;

public class BatteryHoursCalculatorSummer
{
    private DateTime DateTimeMin { get; set; }
    private DateTime DateTimeMax { get; set; }
    private Region Region { get; set; }
    private InverterBattery InverterBattery { get; set; }
    private MySQLDBContext _dbContext;
    private IMapper Mapper { get; set; }
    private int CompanyId { get; set; }
    private int RegisteredInverterId { get; set; }
    private int TotalHours { get; set; }
    private Company Company { get; set; }   
    private Inverter Inverter { get; set; }
    private WeatherApiComService WeatherApiComService { get; set; }

    private List<BatteryControlHours> BatteryControlHours = new List<BatteryControlHours>();
    private List<BatteryControlHours> BatteryControlHoursLastNegativeBatteryLevel = new List<BatteryControlHours>();
    private List<BatteryHoursSummer> BatteryHoursSummer { get; set; }
    private List<BatteryHoursSummer> BatteryHoursSummerCopy { get; set; }
    private List<BatteryControlHours> BatteryHoursSummerResult { get; set; }
    private RedisCacheService RedisCacheService { get; set; }
    private DateTime CurrentDateTime { get; set; } = DateTime.Now; //  new DateTime(2025, 3, 28, 14, 0, 0); //Algu kuupäev ja kell 
    private double remainingBatteryLevel { get; set; } = 0;
    private double remainingBatteryForCalculation { get; set; } = 0;

    private List<BatteryWave> remainingBatteryLevelList = new List<BatteryWave>();
    private SofarState mqttLogFromRedisRegular;
    private double BatteryMaxUsableWatts { get; set; } = 0;
    private DateTime maxDateStartHour;

    private class BatteryWave
    {
        public int WaveNumber { get; set; }
        public double RemainingBattery { get; set; }
    }

    public BatteryHoursCalculatorSummer(DateTime dateTimeMin, DateTime dateTimeMax, Region region, InverterBattery inverterBattery, int companyId, int registeredInverterId, WeatherApiComService weatherApiComService)
    {
        Mapper = AutoMapperConfig.InitializeAutoMapper();
        DateTimeMin = dateTimeMin;
        DateTimeMax = dateTimeMax;
        Region = region;
        InverterBattery = inverterBattery;
        CompanyId = companyId;
        RegisteredInverterId = registeredInverterId;
        TotalHours = (int)(DateTimeMax - DateTimeMin).TotalHours;
        WeatherApiComService = weatherApiComService;
        RedisCacheService = new RedisCacheService(RedisConfiguration.GetConnection());
        maxDateStartHour = new DateTime(DateTimeMax.Year, DateTimeMax.Month, DateTimeMax.Day, 0, 0, 0);
    }

    public async Task<List<BatteryControlHours>> PopulateData()
    {
        _dbContext = await new DatabaseService().CreateDbContextAsync();

        Company = await _dbContext.Company.FirstOrDefaultAsync(x => x.Id == CompanyId);
        Inverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.RegisteredInverterId == RegisteredInverterId);

        maxDateStartHour = new DateTime(DateTimeMax.Year, DateTimeMax.Month, DateTimeMax.Day, 0, 0, 0);

        var batteryHoursSummers = BuildInitialData();

       // mqttLogFromRedisRegular = await RedisCacheService.GetKeyValue<SofarState>(RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset);
        // Fix for CS0029: Ensure the correct type is assigned to mqttLogFromRedisRegular.  
        // The RedisCacheService.GetKeyValue<T> method returns a List<T>, so we need to select the first item from the list.  

        mqttLogFromRedisRegular = (await RedisCacheService.GetKeyValue<SofarState>(RegisteredInverterId.ToString(), Constants.SofarStateUnixOffset))?.FirstOrDefault();
        /*
        if (mqttLogFromRedisRegular != null)
        {
            mqttLogFromRedisRegular.batterySOC = 14;
        }
        */
      
        var hasConsumptionData = await GetAverageConsumptionForDailyHours(CompanyId, RegisteredInverterId, 14);

        if (!hasConsumptionData)
        {
            return BatteryControlHours;
        }

        BatteryHoursSummer = await ProcessAverageConsumption(batteryHoursSummers);

        var weatherSuccessful = await ProcessWeatherRequest();

        if (!weatherSuccessful)
        {
           return BatteryControlHours;
        }

        BatteryHoursSummer = await ProcessWeatherData(batteryHoursSummers);

        BatteryHoursSummer = await ProcessPrices(batteryHoursSummers);

        BatteryHoursSummerCopy = DeepCopyUtility.DeepCopyList(BatteryHoursSummer);

        //SUMMER

        AssignMaxUsableWatts(); //AKU MAHTUVUS - mite aku täituvuse %

        ProcessInitialChargeWithRemainingSun(); //Esimsed Sharge remainig sun-id

        ProcessInitialSelfUse(); //Esimsed Self-used

        if (Inverter.MaxSalesPowerCapacity > 0)
        {
            await ProcessConsumeBatteryWithMaxPowerV2(); //Esimsed Consume maxid - Siin sees on Summer Medium, max Light (Ilma winterita sest Purchase from grid linnuke on all pool
        }

        AddMissingHoursFromInitialData(); //Lisab tühjadele tundidele charge remaining sunnud - ei pea modima!

        BatteryControlHours = BatteryControlHours.OrderBy(x => x.SpotPriceMax.DateTime).ToList();

        if (Inverter.CalculationFormula == CalculationFormula.Summer)
        {
            await ProcessSelfUseForRemainingBattery();
        }     

        ProcessChargeWithRemainingSunForNextDay();

        BatteryControlHours = BatteryControlHours.OrderBy(x => x.SpotPriceMax.DateTime).ToList();

        await AssignSelfUseForProfitableChargeWithRemainingSunHours();


        BatteryHoursSummerResult = DeepCopyUtility.DeepCopyList(BatteryControlHours);

        if (InverterBattery.AllowPurchasingFromGridInSummer)
        {
            AssignForecastedSolar();

            await CombineWithWinterFormula(); // Kui Purchase from grid on aktiveeritud - ehk ostude tegemine

            RemoveExpensiveChargeMaxFromWavesWhichAreNotNdeeded();

            BatteryControlHours = CalculateRemainingBateryWattsForRecords(BatteryControlHours);

            await CleanSummerUnderProfitSelfUses();

            CombineSelfUseAndConsumeMax();

            await OptimizeBatteryLevelUsageToPositiveValue(); //Aku alla 0 siis hakkame koristama Consume max-e ja Self-usesid

            await FirstDaySelfUsesIfIsBattAndChargingOptionNight();

            if (InverterBattery.LoadBatteryTo95PercentEnabled
                && GetBatteryMaxUsableWatts() - GetChargeMaxVolumeFromPlan() > 0)
            {
                BatteryControlHours = assignChargMaxForSuitablePurchPrices(BatteryControlHours);
            }

            BatteryControlHours = BatteryControlHours.OrderBy(x => x.SpotPriceMax.DateTime).ToList();
            BatteryControlHours = CalculateRemainingBateryWattsForRecords(BatteryControlHours);

            if (InverterBattery.ConsiderRemainingBatteryOnPurchase)
            {
                BatteryControlHours = OptimizeChargeMaxWithBatterLevel(BatteryControlHours, maxDateStartHour, Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000));

                BatteryControlHours = PreventBatterGoingBelowMinimum(BatteryControlHours);

                if (!BatteryControlHours.Any(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax))
                {
                    BatteryControlHours = BatteryHoursSummerResult;
                }
            }
        }

        if (Inverter.PVInverterIsSeparated)
        {
            await ReplaceChargeRemainingSunWithSelfUseWhenHasSolarEnergy();
        }   

        ProcessSelfUseType();

       // if (!Debugger.IsAttached)
      //  {
            await SaveToDB();
            await _dbContext.DisposeAsync();
      //  }
     
        return BatteryControlHours;       
    }

    private List<BatteryControlHours> PreventBatterGoingBelowMinimum(List<BatteryControlHours> batteryControlHours)
    {
        var firstChargeMax = batteryControlHours.OrderByDescending(x=> x.SalePrice).FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax && x.IsChargeMaxRemovedAttempted == false);

        if (firstChargeMax == null)
        {
            return batteryControlHours;
        }

        firstChargeMax = SetChargeWithRemainingSun(firstChargeMax);

        batteryControlHours = CalculateRemainingBateryWattsForRecords(batteryControlHours);

        if (batteryControlHours.Any(x => x.RemainingBattery < 0))
        {
            firstChargeMax = SetChargeMax(firstChargeMax);
            firstChargeMax.IsChargeMaxRemovedAttempted = true;            
        }

        batteryControlHours = PreventBatterGoingBelowMinimum(batteryControlHours);

        return batteryControlHours;
    }

    private void RemoveExpensiveChargeMaxFromWavesWhichAreNotNdeeded()
    {
        BatteryControlHours = CalculateRemainingBateryWattsForRecords(BatteryControlHours);
        List<int> processedIds = new();

        List<BatteryControlHours> BatteryControlHoursCopy = DeepCopyUtility.DeepCopyList(BatteryControlHours).OrderByDescending(x => x.SpotPriceMax.DateTime).ToList();

        processedIds = RemoveExpensiveChargeMaxFromWaves(processedIds, maxDateStartHour, BatteryControlHoursCopy);

        foreach (var id in processedIds)
        {
            var toChargeMax = BatteryControlHours.FirstOrDefault(x => x.SpotPriceMax.Id == id);

            toChargeMax = SetChargeMax(toChargeMax);
        }
    }

    private List<int> RemoveExpensiveChargeMaxFromWaves(List<int>processedIds, DateTime winterMaxDateTime,  List<BatteryControlHours> BatteryControlHoursCopy)
    {
       

        var firstSelfUseOrConsumeMaxBeforeChargeMax = FindFirstSelfUseOrConsumeMaxBeforeFirstChargeMax(winterMaxDateTime, BatteryControlHoursCopy);
     
        if (firstSelfUseOrConsumeMaxBeforeChargeMax == null)
        {
            return processedIds;
        }

        List<BatteryControlHours> groupedChargeMax = new();

        foreach (var rec in BatteryControlHoursCopy.Where(x=> x.SpotPriceMax.DateTime >= winterMaxDateTime && x.SpotPriceMax.DateTime < firstSelfUseOrConsumeMaxBeforeChargeMax.SpotPriceMax.DateTime))
        { 
            if (rec.ActionTypeCommandFromWinter != ActionTypeCommand.ChargeMax && (rec.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower || rec.ActionTypeCommand == ActionTypeCommand.SelfUse))
            {
                break;
            }

            if (rec.ActionTypeCommandFromWinter == ActionTypeCommand.ChargeMax)
            {
                groupedChargeMax.Add(rec);
            }
        }

        if (groupedChargeMax.Count == 0)
        {
            return processedIds;
        }

        var highestPriceChargeMax = groupedChargeMax.OrderBy(x => x.SalePrice).FirstOrDefault();


        var highestPriceChargeMaxCopy = DeepCopyUtility.DeepCopy(highestPriceChargeMax);

        highestPriceChargeMax = SetChargeMax(highestPriceChargeMax);

        BatteryControlHoursCopy = CalculateRemainingBateryWattsForRecords(BatteryControlHoursCopy);

        //  var hasRecordWithOverload = BatteryControlHoursCopy.Where(x => x.SpotPriceMax.DateTime > highestPriceChargeMax.SpotPriceMax.DateTime && x.SpotPriceMax.DateTime <= firstSelfUseOrConsumeMaxBeforeChargeMax.SpotPriceMax.DateTime).Any(x => x.RemainingBattery > (BatteryMaxUsableWatts * 1.5));
        var hasRecordWithOverload = BatteryControlHoursCopy.Where(x => x.SpotPriceMax.DateTime > highestPriceChargeMax.SpotPriceMax.DateTime).Any(x => x.RemainingBattery > (BatteryMaxUsableWatts * (1+ InverterBattery.AdditionalTimeForBatteryChargingPercentage)));
        if (hasRecordWithOverload)
        {
            highestPriceChargeMax.ActionTypeCommand = highestPriceChargeMaxCopy.ActionTypeCommand;
            highestPriceChargeMax.MinChargingPowerWhOriginal = highestPriceChargeMaxCopy.MinChargingPowerWhOriginal;
            highestPriceChargeMax.UsableWatts = highestPriceChargeMaxCopy.UsableWatts;
            highestPriceChargeMax.ActionTypeCommandFromWinter = ActionTypeCommand.None;
            BatteryControlHoursCopy = BatteryControlHoursCopy.OrderByDescending(x => x.SpotPriceMax.DateTime).ToList();
            return RemoveExpensiveChargeMaxFromWaves(processedIds, winterMaxDateTime, BatteryControlHoursCopy);

        }
        else
        {
            highestPriceChargeMax.ActionTypeCommandFromWinter = ActionTypeCommand.None;
            BatteryControlHoursCopy = BatteryControlHoursCopy.OrderByDescending(x=> x.SpotPriceMax.DateTime).ToList();
            processedIds.Add(highestPriceChargeMax.SpotPriceMax.Id);
            return RemoveExpensiveChargeMaxFromWaves(processedIds, winterMaxDateTime, BatteryControlHoursCopy);
        }   
    }

    public BatteryControlHours FindFirstSelfUseOrConsumeMaxBeforeFirstChargeMax(DateTime winterMaxDateTime, List<BatteryControlHours> batteryControlHours)
    {
        BatteryControlHours firstChargeMax = batteryControlHours.FirstOrDefault(x => x.SpotPriceMax.DateTime >= winterMaxDateTime && x.ActionTypeCommandFromWinter == ActionTypeCommand.ChargeMax);
        if (firstChargeMax == null)
        {
            return null; // No ChargeMax found
        }

        DateTime firstChargeMaxTime = firstChargeMax.SpotPriceMax.DateTime;

        BatteryControlHours firstSelfUseOrConsumeMax = batteryControlHours
            .Where(x =>  x.SpotPriceMax.DateTime >= winterMaxDateTime && x.SpotPriceMax.DateTime > firstChargeMaxTime && (x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower))
            .LastOrDefault();

        return firstSelfUseOrConsumeMax;
    }

    private List<BatteryControlHours> OptimizeChargeMaxWithBatterLevel(List<BatteryControlHours> batteryControlHours, DateTime startDateTime, int deductableValue = 0)
    {
        var dateCriteriaRecords = batteryControlHours.Where(x => x.SpotPriceMax.DateTime >= startDateTime).ToList();

        if (dateCriteriaRecords.Count == 0)
        {
            return batteryControlHours;
        }

        var firstChargeMax = dateCriteriaRecords.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax);

        if (firstChargeMax == null)
        {
            return batteryControlHours;
        }

        var firstHour = batteryControlHours.FirstOrDefault(x=> x.SpotPriceMax.DateTime == startDateTime);

        if (firstHour == null)
        {
            return batteryControlHours;
        }

        if (firstHour.RemainingBattery < deductableValue)
        {
            return batteryControlHours;
        }



        if (firstHour.RemainingBattery < Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000))
        {
            return batteryControlHours;
        } 


        dateCriteriaRecords = batteryControlHours.Where(x => x.SpotPriceMax.DateTime >= firstChargeMax.SpotPriceMax.DateTime).ToList();

        BatteryControlHours highestPriceChargeMax = new ();

        foreach (var item in dateCriteriaRecords)
        {
            if (item.ActionTypeCommand == ActionTypeCommand.ChargeMax)
            {
                if (highestPriceChargeMax.SalePrice < item.SalePrice)
                {
                    highestPriceChargeMax = item;
                }
            }

            if (item.ActionTypeCommand == ActionTypeCommand.SelfUse || item.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower)
            {
                break;
            }
        }

        var recordBeforeHighestPriceChargeMax = dateCriteriaRecords.FirstOrDefault(x => x.SpotPriceMax.DateTime == highestPriceChargeMax.SpotPriceMax.DateTime.AddHours(-1));

        if (recordBeforeHighestPriceChargeMax == null)
        {
            return batteryControlHours;
        }

        if (recordBeforeHighestPriceChargeMax.RemainingBattery - Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000)
            > InverterBattery.CapacityKWh * (InverterBattery.MinLevel / 100) * 1000)
        {
            highestPriceChargeMax = SetChargeWithRemainingSun(highestPriceChargeMax);

            batteryControlHours = CalculateRemainingBateryWattsForRecords(batteryControlHours);

            return OptimizeChargeMaxWithBatterLevel(batteryControlHours, startDateTime, Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000) + deductableValue);
        }

        return batteryControlHours;
    }

    private BatteryControlHours SetChargeWithRemainingSun(BatteryControlHours batteryControlHours)
    {
        batteryControlHours.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
        batteryControlHours.UsableWatts = 0;
        batteryControlHours.ConsumeBatterySellPower = 0;
        batteryControlHours.MinChargingPowerWhOriginal = 0;
        batteryControlHours.RemainingBattery = 0;

        return batteryControlHours;
    }

    private BatteryControlHours SetChargeMax(BatteryControlHours batteryControlHours)
    {
        batteryControlHours.ActionTypeCommand = ActionTypeCommand.ChargeMax;
        batteryControlHours.MinChargingPowerWhOriginal = Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000);
        batteryControlHours.UsableWatts = 0;

        return batteryControlHours;
    }

    private int GetChargeMaxVolumeFromPlan() => (int)BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax).Sum(s => s.MinChargingPowerWhOriginal)!;           //  summerRecordForHour.MinChargingPowerWhOriginal = Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000);

    private double GetBatteryMaxUsableWatts()
    {
        var chargingAdditionalPercentage = InverterBattery.AdditionalTimeForBatteryChargingPercentage > 0 ? 1 + (InverterBattery.AdditionalTimeForBatteryChargingPercentage) : 1;
        return  (InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)) * chargingAdditionalPercentage;
    }

    private List<BatteryControlHours> assignChargMaxForSuitablePurchPrices(List<BatteryControlHours> batteryControlHours)
    {
        if (batteryControlHours.Count == 0) return batteryControlHours;

          var firstSuitableRecord = batteryControlHours.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun && x.SalePrice < (Convert.ToDouble(InverterBattery.LoadBatteryTo95PercentPrice) / 100));

        if (firstSuitableRecord == null) return batteryControlHours;

        firstSuitableRecord = SetChargeMax(firstSuitableRecord);

        if (GetBatteryMaxUsableWatts() - GetChargeMaxVolumeFromPlan() > 0)
        {
            return assignChargMaxForSuitablePurchPrices(batteryControlHours);
        }

        return batteryControlHours;
    }


    private async Task CleanSummerUnderProfitSelfUses()
    {
        if (BatteryControlHours != null &&  BatteryControlHours.Count > 0)
        {
            var latestChargeMax = BatteryControlHours.LastOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax);

            if (latestChargeMax == null)
            {
                latestChargeMax = BatteryControlHours.MinBy(x => x.SalePrice);
            }

            foreach (var item in BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower))
            {
                if (item.PurchasePrice < latestChargeMax.SalePrice + (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {
                    item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                    item.UsableWatts = 0;
                    item.ConsumeBatterySellPower = 0;
                    item.MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal;
                    item.RemainingBattery = 0;

                    if (Inverter.UseFixedAvgHourlyWatts)
                    {
                        item.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
                    }
                    else
                    {
                        var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                            && x.DayOfWeek == item.SpotPriceMax.Date.DayOfWeek
                            && x.TimeHour == item.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                        item.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                    }
                }
            }

            foreach (var item in BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse))
            {
                if (item.SalePrice < latestChargeMax.SalePrice + (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {  
                    item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                    item.UsableWatts = 0;
                    item.ConsumeBatterySellPower = 0;
                    item.MinChargingPowerWhOriginal = 0;
                    item.RemainingBattery = 0;
                }
            }
        }
    }

    private async Task FirstDaySelfUsesIfIsBattAndChargingOptionNight()
    {
        if (mqttLogFromRedisRegular == null || BatteryControlHours == null ||  BatteryControlHours.Count == 0)
        {
            return;
        }

        var BatteryControlHoursCopy = DeepCopyUtility.DeepCopyList(BatteryControlHours);

        foreach (var item in BatteryControlHoursCopy)
        {
            item.IsChanged = false;
        }

        BatteryControlHoursCopy = BatteryControlHoursCopy.OrderBy(x => x.SpotPriceMax.DateTime).ToList();

        var firstChargingHour = BatteryControlHoursCopy.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax);

        if (firstChargingHour != null && mqttLogFromRedisRegular.batterySOC > InverterBattery.MinLevel)
        {
            BatteryControlHoursCopy = CalculateRemainingBateryWattsForRecords(BatteryControlHoursCopy);

            BatteryControlHoursCopy = await AddSelfUseWhenPossible(BatteryControlHoursCopy, firstChargingHour);

            var chargingPowerInWatts = Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000);

            bool addChargeMaxPossible = false;
            (BatteryControlHoursCopy, addChargeMaxPossible) = AddChargeMaxWhenPossible(BatteryControlHoursCopy, chargingPowerInWatts, 1);

            if (addChargeMaxPossible)
            {
                BatteryControlHours = BatteryControlHoursCopy;
            }           
        }
    }

    private async Task<List<BatteryControlHours>> AddSelfUseWhenPossible(List<BatteryControlHours> batteryControlHoursLocal, BatteryControlHours firstChargingHour, int numberOfIteration = 1)
    {
        if (numberOfIteration > 100)
        {
            return batteryControlHoursLocal;
        }

       

        var firstSuitableRecord = batteryControlHoursLocal.OrderByDescending(x=> x.PurchasePrice).FirstOrDefault(x => x.SpotPriceMax.DateTime < firstChargingHour.SpotPriceMax.DateTime
                && x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun
                && x.SalePrice > firstChargingHour.SalePrice + (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
                && x.RemainingBattery > 0
                && x.IsChanged == false);

        if (firstSuitableRecord == null)
        {

            return batteryControlHoursLocal;
        }
        else
        {
            firstSuitableRecord.IsChanged = true;
            firstSuitableRecord.ActionTypeCommand = ActionTypeCommand.SelfUse;
            firstSuitableRecord.UsableWatts = 0;
            firstSuitableRecord.ConsumeBatterySellPower = 0;
            firstSuitableRecord.MinChargingPowerWhOriginal = firstSuitableRecord.MinChargingPowerWhOriginal;
            firstSuitableRecord.RemainingBattery = 0;

            if (Inverter.UseFixedAvgHourlyWatts)
            {
                firstSuitableRecord.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
            }
            else
            {
                var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                    && x.DayOfWeek == firstSuitableRecord.SpotPriceMax.Date.DayOfWeek
                    && x.TimeHour == firstSuitableRecord.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                firstSuitableRecord.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
            }

            batteryControlHoursLocal = CalculateRemainingBateryWattsForRecords(batteryControlHoursLocal);

            if (firstSuitableRecord.RemainingBattery < 0)
            {
                return batteryControlHoursLocal;
            }

            return await AddSelfUseWhenPossible(batteryControlHoursLocal, firstChargingHour, numberOfIteration + 1);

        }

    }


    private (List<BatteryControlHours>, bool) AddChargeMaxWhenPossible(List<BatteryControlHours> batteryControlHoursLocal, int chargingPowerInWatts, int replaceIteration, bool isSuccessful = false, int numberOfIteration = 1)
    {
        if (numberOfIteration >= 10)
        {
            Console.WriteLine($"InverterId: {Inverter.Id} reached to max of 10");
            return (batteryControlHoursLocal, isSuccessful);
        }
        var recordsWithEnoughChargingPower = batteryControlHoursLocal.Where(x=> x.IsChanged).TakeUntilSumV2(x => Convert.ToDouble(x.MaxAvgHourlyConsumptionOriginal), Convert.ToInt32(InverterBattery.ChargingPowerFromGridKWh * 1000));

        if (recordsWithEnoughChargingPower == null || recordsWithEnoughChargingPower.Count == 0)
        {
            return (batteryControlHoursLocal, isSuccessful);
        }

        var cheapestChargeWithRemainingSun = batteryControlHoursLocal.Where(x => x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun).OrderBy(x=> x.SalePrice).FirstOrDefault();

        if (cheapestChargeWithRemainingSun == null)
        {
            return (batteryControlHoursLocal, isSuccessful);
        }

        var batteryControlHoursLocalCopy = DeepCopyUtility.DeepCopyList(batteryControlHoursLocal);

        if (recordsWithEnoughChargingPower.LastOrDefault()!.SalePrice > cheapestChargeWithRemainingSun.SalePrice + (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
        {
            cheapestChargeWithRemainingSun = SetChargeMax(cheapestChargeWithRemainingSun);

            batteryControlHoursLocal = CalculateRemainingBateryWattsForRecords(batteryControlHoursLocal);

            foreach (var item in batteryControlHoursLocal.Where(x=> x.SpotPriceMax.DateTime >= cheapestChargeWithRemainingSun.SpotPriceMax.DateTime).ToList())
            {
                if (item.ActionTypeCommand == ActionTypeCommand.ChargeMax)
                {
                    if (item.RemainingBattery > BatteryMaxUsableWatts && replaceIteration > 1)
                    {
                        return (batteryControlHoursLocalCopy, isSuccessful);
                    }
                }
                else
                {
                    break;
                }
            }

            foreach (var item in recordsWithEnoughChargingPower)
            {
                item.IsChanged = false;
            }
        }

        return AddChargeMaxWhenPossible(batteryControlHoursLocal, chargingPowerInWatts, replaceIteration + 1, true, numberOfIteration + 1);
    }



    private void CombineSelfUseAndConsumeMax()
    {
        foreach (var item in BatteryControlHours)
        {
            if (item.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower)
            {
                item.SelfUseConsumeCombinedMaxPrice = item.PurchasePrice;
            }
            else
            {
                item.SelfUseConsumeCombinedMaxPrice = item.SalePrice;
            }
        }

        BatteryControlHours = BatteryControlHours.OrderBy(x=> x.SelfUseConsumeCombinedMaxPrice).ToList();
    }
    private async Task OptimizeBatteryLevelUsageToPositiveValue()
    {
        var lowestBatteryLevel = BatteryControlHours.OrderBy(x => x.RemainingBattery).FirstOrDefault();

        BatteryControlHoursLastNegativeBatteryLevel = DeepCopyUtility.DeepCopyList(BatteryControlHours);

        if (lowestBatteryLevel != null
            && lowestBatteryLevel.RemainingBattery < 0)
        {
            var cheapestSelfUseOrConsumeMax = BatteryControlHours.Where(x => x.IsProcessedInCombine == false && (x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower)).OrderBy(x=> x.SelfUseConsumeCombinedMaxPrice).FirstOrDefault();

            if (cheapestSelfUseOrConsumeMax != null)
            {
                if (cheapestSelfUseOrConsumeMax.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower
                    && Math.Abs(lowestBatteryLevel.RemainingBattery) < Inverter.MaxSalesPowerCapacity * 1000 / 2)

                {
                    cheapestSelfUseOrConsumeMax.IsProcessedInCombine = true;
                }
                else
                {
                    var latestChargeMax = BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax).OrderBy(x => x.SalePrice).FirstOrDefault();
                    if (cheapestSelfUseOrConsumeMax.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower
                        || cheapestSelfUseOrConsumeMax.ActionTypeCommand == ActionTypeCommand.SelfUse
                        && cheapestSelfUseOrConsumeMax.SalePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
                        && latestChargeMax != null
                        && (cheapestSelfUseOrConsumeMax.SalePrice - latestChargeMax.SalePrice) > (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                    {
                        cheapestSelfUseOrConsumeMax.ActionTypeCommand = ActionTypeCommand.SelfUse;
                        cheapestSelfUseOrConsumeMax.MinChargingPowerWhOriginal = cheapestSelfUseOrConsumeMax.MinChargingPowerWhOriginal;
                        cheapestSelfUseOrConsumeMax.UsableWatts = 0;
                        cheapestSelfUseOrConsumeMax.ConsumeBatterySellPower = 0;
                        cheapestSelfUseOrConsumeMax.IsProcessedInCombine = true;

                        if (Inverter.UseFixedAvgHourlyWatts)
                        {
                            cheapestSelfUseOrConsumeMax.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
                        }
                        else
                        {
                            var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                            && x.DayOfWeek == cheapestSelfUseOrConsumeMax.SpotPriceMax.Date.DayOfWeek
                                            && x.TimeHour == cheapestSelfUseOrConsumeMax.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                            cheapestSelfUseOrConsumeMax.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                        }
                    }
                    else
                    {
                        cheapestSelfUseOrConsumeMax.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                        cheapestSelfUseOrConsumeMax.IsProcessedInCombine = true;
                    }
                }


                BatteryControlHours = CalculateRemainingBateryWattsForRecords(BatteryControlHours);

                await OptimizeBatteryLevelUsageToPositiveValue();
            }


        }
        else
        {
            BatteryControlHours = BatteryControlHoursLastNegativeBatteryLevel;
        }

    }

    private void AssignForecastedSolar()
    {
        foreach (var item in BatteryControlHours)
        {
            var matchingRecord = BatteryHoursSummerCopy.FirstOrDefault(b => b.DateTime == item.SpotPriceMax.DateTime);
            item.ForecastedSolar = matchingRecord.EstimatedSolar;
        }
    }

    private async Task ReplaceChargeRemainingSunWithSelfUseWhenHasSolarEnergy()
    {
        foreach (var item in BatteryControlHours.Where(x=> x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun))
        {
            var matchingRecord = BatteryHoursSummerCopy.FirstOrDefault(b => b.DateTime == item.SpotPriceMax.DateTime);

            if (matchingRecord != null && matchingRecord.EstimatedSolar > 100)
            {
                item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                item.MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal;
                item.RemainingBattery = item.RemainingBattery;
                item.UsableWatts = 0;
                item.RemainingBattery = 0;
                item.ConsumeBatterySellPower = 0;
  
                if (item.ActionTypeCommand == ActionTypeCommand.InverterSelfUse || item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                {
                    if (Inverter.UseFixedAvgHourlyWatts)
                    {
                        item.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
                    }
                    else
                    {
                        var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                        && x.DayOfWeek == item.SpotPriceMax.Date.DayOfWeek
                                        && x.TimeHour == item.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                        item.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                    }
                }
            }
        }
    }

    private async Task CombineWithWinterFormula()
    {
        // Arvestab ka jooksva päevaga
        // BatteryHoursCalculator batteryHoursCalculator = new BatteryHoursCalculator(new DateTime(DateTimeMin.Year, DateTimeMin.Month, DateTimeMin.Day, CurrentDateTime.Hour + 1, 0, 0), new DateTime(DateTimeMax.Year, DateTimeMax.Month, DateTimeMax.Day, 23, 0, 0), Region, InverterBattery, CompanyId, RegisteredInverterId);
        var _dbContext = await new DatabaseService().CreateDbContextAsync();
        BatteryHoursCalculator batteryHoursCalculator = new BatteryHoursCalculator
            (new DateTime(DateTimeMax.Year, DateTimeMax.Month, DateTimeMax.Day, 0, 0, 0),
            new DateTime(DateTimeMax.Year, DateTimeMax.Month, DateTimeMax.Day, 23, 0, 0),
            Region,
            InverterBattery,
            CompanyId,
            RegisteredInverterId,
            WeatherApiComService,
            _dbContext);

        var winterCalculationResult = await batteryHoursCalculator.MainCalculation();

        foreach (var item in winterCalculationResult)
        {
            if (item.ActionTypeCommand == ActionTypeCommand.InverterSelfUse
                || item.ActionTypeCommand == ActionTypeCommand.SelfUse)
            {
                var summerRecord = BatteryControlHours.FirstOrDefault(x => x.SpotPriceMax.DateTime == item.SpotPriceMax.DateTime);

                if (summerRecord != null
                    && summerRecord.ActionTypeCommand != ActionTypeCommand.InverterSelfUse
                    && summerRecord.ActionTypeCommand != ActionTypeCommand.SelfUse)
                {
                    summerRecord.ActionTypeCommand = item.ActionTypeCommand;
                    summerRecord.MinChargingPowerWhOriginal = item.MinChargingPowerWhOriginal;
                    summerRecord.RemainingBattery = item.RemainingBattery;
                    summerRecord.UsableWatts = 0;
                    summerRecord.RemainingBattery = 0;
                    summerRecord.ConsumeBatterySellPower = 0;
                    summerRecord.MaxAvgHourlyConsumption = item.MaxAvgHourlyConsumptionOriginal;

                    if (item.ActionTypeCommand == ActionTypeCommand.InverterSelfUse || item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                    {
                        if (Inverter.UseFixedAvgHourlyWatts)
                        {
                            summerRecord.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
                        }
                        else
                        {
                            var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                            && x.DayOfWeek == item.SpotPriceMax.Date.DayOfWeek
                                            && x.TimeHour == item.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                            summerRecord.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                        }
                    }
                }
            }

            if (item.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower)
            {
                var summerRecord = BatteryControlHours.FirstOrDefault(x => x.SpotPriceMax.DateTime == item.SpotPriceMax.DateTime);

                if (summerRecord != null
                    && summerRecord.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower)
                {
                    summerRecord.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                    summerRecord.MaxAvgHourlyConsumption = item.MaxAvgHourlyConsumptionOriginal;
                    summerRecord.RemainingBattery = 0;
                    summerRecord.ConsumeBatterySellPower = (InverterBattery.DischargingPowerToGridKWh > Inverter.MaxSalesPowerCapacity ?
                                                                                                            Inverter.MaxSalesPowerCapacity
                                                                                                            : InverterBattery.DischargingPowerToGridKWh)
                                                                                                            * 1000;
                    summerRecord.UsableWatts = await GetConsumeMaxSalesPower(summerRecord);

                }
            }
        }

        BatteryControlHours = CalculateRemainingBateryWattsForRecords(BatteryControlHours);

        foreach (var item in winterCalculationResult.Where(x => x.ActionTypeCommand == ActionTypeCommand.ChargeMax).OrderByDescending(x=> x.SpotPriceMax.DateTime))
        {
            List<BatteryControlHours> BatteryControlHoursCopy = DeepCopyUtility.DeepCopyList(BatteryControlHours);

            var summerRecordForHour = BatteryControlHoursCopy.FirstOrDefault(x => x.SpotPriceMax.DateTime == item.SpotPriceMax.DateTime);

            summerRecordForHour.ActionTypeCommandFromWinter = ActionTypeCommand.ChargeMax;
            BatteryControlHours = BatteryControlHoursCopy;         
        }
    }

    private List<BatteryControlHours> CalculateRemainingBateryWattsForRecords(List<BatteryControlHours> batteryControlHoursInput)
    {
        CalculateRemainingBatteryLevel();

        var batterControlHours = batteryControlHoursInput.OrderBy(x => x.SpotPriceMax.DateTime);

        foreach (var item in batterControlHours)
        {
            double deductableWatts = 0;

            if (item.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun)
            {
                if (item.ForecastedSolarRemaining > 0)
                {
                    deductableWatts = item.ForecastedSolarRemaining;
                }
                else
                {
                    deductableWatts = 0;
                }
                
            }
            else
            {
              //  if (item.ForecastedSolarRemaining > 0)
               // {
                    deductableWatts = Convert.ToDouble(item.ForecastedSolarRemaining) - Convert.ToDouble(item.ForecastedConsumption) - Convert.ToDouble(item.UsableWatts);
              //  }
              //  else
             //   {
                //    deductableWatts = Convert.ToDouble(item.ForecastedConsumption) - Convert.ToDouble(item.ForecastedSolar) - Convert.ToDouble(item.ForecastedConsumption) - Convert.ToDouble(item.UsableWatts);
              //  }
                
            }

            if (item.ActionTypeCommand == ActionTypeCommand.ChargeMax)
            {
                remainingBatteryLevel += Convert.ToDouble(item.MinChargingPowerWhOriginal);
            }
            else
            {
                remainingBatteryLevel+= deductableWatts;
            }

            item.RemainingBattery = remainingBatteryLevel;


        }

        return batterControlHours.OrderBy(x => x.SpotPriceMax.DateTime).ToList();
    } 

    private void AssignMaxUsableWatts()
    {
        var chargingAdditionalPercentage = InverterBattery.AdditionalTimeForBatteryChargingPercentage > 0 ? 1 + (InverterBattery.AdditionalTimeForBatteryChargingPercentage) : 1;
        BatteryMaxUsableWatts = (InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)) * chargingAdditionalPercentage; // - 20000; // - 50000;
    }

    private async Task AssignSelfUseForProfitableChargeWithRemainingSunHours()
    {
        foreach (var item in BatteryControlHours)
        {
            if (item.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun
                && item.SalePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
                && item.ForecastedSolarRemaining > 0)
            {
                if (Inverter.UseFixedAvgHourlyWatts)
                {
                    item.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
                }
                else
                {
                    var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                    && x.DayOfWeek == item.SpotPriceMax.Date.DayOfWeek
                                    && x.TimeHour == item.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                    item.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                }

                item.ActionTypeCommand = ActionTypeCommand.SelfUse;               
                item.WaveNumber = null;
            }
        }
    }
    private void ProcessSelfUseType()
    {
        if (Inverter.UseInverterSelfUse)
        {
            foreach (var item in BatteryControlHours)
            {
                if (item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                {
                    item.ActionTypeCommand = ActionTypeCommand.InverterSelfUse;
                }
            }
        }
    }

    private async Task ProcessSelfUseForRemainingBattery()
    {
        var firstRecord = BatteryControlHours.FirstOrDefault(x=> x.ForecastedSolarRemaining == 0 && x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun);

        if (firstRecord == null)
        {
            return;
        }

        var listOfSuitableRecords = BatteryControlHours.Where(x => x.SpotPriceMax.DateTime >= firstRecord.SpotPriceMax.DateTime && x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun).TakeWhile(x=> x.ForecastedSolarRemaining == 0).OrderByDescending(x=> x.SalePrice).ToList();

        if (listOfSuitableRecords.Count > 0)
        {
            if (BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).ToList().Count > 0)
            {
                remainingBatteryLevel = BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).Min(x => x.RemainingBattery);
                foreach (var item in listOfSuitableRecords)
                {
                    if (remainingBatteryLevel < item.ForecastedConsumption)
                    {
                        break;
                    }

                    if ((item.ActionTypeCommand != ActionTypeCommand.SelfUse || item.ActionTypeCommand != ActionTypeCommand.InverterSelfUse)
                        && item.SalePrice < (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                    {
                        continue;
                    }

                    item.ActionTypeCommand = ActionTypeCommand.SelfUse;

                    if (item.ActionTypeCommand == ActionTypeCommand.InverterSelfUse || item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                    {
                        if (Inverter.UseFixedAvgHourlyWatts)
                        {
                            item.MaxAvgHourlyConsumptionOriginal = Inverter.FixedAvgHourlyWatts;
                        }
                        else
                        {
                            var avgConsumptionRecord = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                            && x.DayOfWeek == item.SpotPriceMax.Date.DayOfWeek
                                            && x.TimeHour == item.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                            item.MaxAvgHourlyConsumptionOriginal = avgConsumptionRecord != null ? avgConsumptionRecord.AvgHourlyConsumption : 0;
                        }
                    }

                    remainingBatteryLevel -= item.ForecastedConsumption;
                }
            }
            else
            {
                return;
            }
        }
    }

    private async Task SaveToDB()
    {
        await ClearBatteryControlHoursScheduleForPeriod();

        foreach (var entity in BatteryControlHours.Where(x=> x.SpotPriceMax.DateTime >= maxDateStartHour).ToList())
        {
            _dbContext.Entry(entity).State = EntityState.Added;
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task ClearBatteryControlHoursScheduleForPeriod()
    {
        var scheduleRecordToDelete = await _dbContext.BatteryControlHours.Where(x => x.SpotPriceMax.DateTime >= maxDateStartHour && x.SpotPriceMax.DateTime <= DateTimeMax && x.InverterBatteryId == InverterBattery.Id).ToListAsync();

        if (scheduleRecordToDelete.Count > 0)
        {
            _dbContext.BatteryControlHours.RemoveRange(scheduleRecordToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }

    private void ProcessChargeWithRemainingSunForNextDay()
    {
        if (BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).ToList().Count > 0)
        {
            remainingBatteryLevel = BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).Min(x => x.RemainingBattery);
        }
        else
        {
            remainingBatteryLevel = 0;
        }      

        var chargingAdditionalPercentage = InverterBattery.AdditionalTimeForBatteryChargingPercentage > 0 ? 1 + (InverterBattery.AdditionalTimeForBatteryChargingPercentage) : 1;
        var maxUsableBatteryWatts = (((InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)))) * chargingAdditionalPercentage - remainingBatteryLevel; // - 50000;
        var remainingBatteryWatts = maxUsableBatteryWatts;
        var solarChargingPower = InverterBattery.ChargingPowerFromSolarKWh * 1000;

        var listOfChargeWithRemainingSunRecords = BatteryControlHours.Where(x => x.ForecastedSolarRemaining > 0
        && x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun
        && x.IsForInitialChargeWithRemainingSun == false)
            .TakeWhile(x=> x.ForecastedSolarRemaining > 0).OrderBy(x=> x.PurchasePrice).ToList();

        foreach (var item in listOfChargeWithRemainingSunRecords)
        {           
            if (remainingBatteryWatts > 0)
            {
                var powerToUseForCharging = item.ForecastedSolarRemaining > solarChargingPower ? solarChargingPower : item.ForecastedSolarRemaining;
                remainingBatteryWatts -= powerToUseForCharging;
                item.WaveNumber = 2;
            }
            else
            {
                if (item.PurchasePrice < (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {
                    item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                    item.WaveNumber = DateTime.Now.Date == item.SpotPriceMax.DateTime.Date ? 1 : 2;
                }
                else
                {
                    item.ActionTypeCommand = ActionTypeCommand.SellRemainingSunNoCharging;
                }              
            }

        }

        remainingBatteryLevelList.Add(new BatteryWave
        {
            RemainingBattery = remainingBatteryWatts,
            WaveNumber = 2

        });
    }
    private async Task ProcessConsumeBatteryWithMaxPowerV2()
    {
        var lastRecord = BatteryControlHours.LastOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse); // || x.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging);

        if (lastRecord == null)
        {
            foreach (var item in BatteryControlHours)
            {
                item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                item.WaveNumber = DateTime.Now.Date == item.SpotPriceMax.DateTime.Date ? 1 : 2;
            }

            return;
        }

        var batteryDischargingPowerToGridKWh = InverterBattery.DischargingPowerToGridKWh > Inverter.MaxSalesPowerCapacity ? Inverter.MaxSalesPowerCapacity : InverterBattery.DischargingPowerToGridKWh;
        double batteryConsumePower = batteryDischargingPowerToGridKWh * 1000;


        var maxSalesPriceForSelfUse = BatteryControlHours.Where(x =>
        x.IsForInitialChargeWithRemainingSun == false
        && x.PurchasePrice > (Company.ExpectedProfitForSelfUseOnlyInCents / 100)).OrderByDescending(o => o.PurchasePrice).FirstOrDefault();
        remainingBatteryLevel = lastRecord.RemainingBattery;

        if (maxSalesPriceForSelfUse == null)
        {
            return;
        }

        if (remainingBatteryLevel + maxSalesPriceForSelfUse.ForecastedConsumption > batteryConsumePower)
        {
            CheckPriceProfitability(PriceType.PurchasePrice);

            if (Inverter.PVInverterIsSeparated)
            {
                await ReduceBatteryRemaining();

           
            }
            else
            {
                if (Inverter.MaxSalesPowerCapacity > 0)
                {
                    //Summer Lighti arvutus
                    await ReduceBatteryRemaining();
                }
                //Summer medium arvutus
                await ProcessWhenSalesPriceIsHigherThanPurchasePrice();
            }

            /*
            if (Inverter.CalculationFormula == CalculationFormula.Summer)
            {
                if (Inverter.MaxSalesPowerCapacity > 0)
                {
                    //Summer Lighti arvutus
                    ReduceBatteryRemaining();
                }
                //Summer max arvutus
                ProcessWhenNotEnoughtBatterySalesMax();
            }
            else if (Inverter.CalculationFormula == CalculationFormula.SummerMedium)
            {
                if (Inverter.MaxSalesPowerCapacity > 0)
                {
                    //Summer Lighti arvutus
                    ReduceBatteryRemaining();
                }
                //Summer medium arvutus
                ProcessWhenSalesPriceIsHigherThanPurchasePrice();
            }
            else
            {
                if (Inverter.MaxSalesPowerCapacity > 0)
                {
                    ReduceBatteryRemaining();
                }
            }
            */
        }
        else
        {
            await ProcessWhenSalesPriceIsHigherThanPurchasePrice();
            /*
            if (Inverter.CalculationFormula == CalculationFormula.Summer)
            {
                ProcessWhenNotEnoughtBatterySalesMax();
            }

            if (Inverter.CalculationFormula == CalculationFormula.SummerMedium)
            {

                ProcessWhenSalesPriceIsHigherThanPurchasePrice();
            }
            */
        }      
    }

    private void RemoveInitialChargingFromSelfUse()
    {
        foreach (var item in BatteryControlHours)
        {
            if (item.ActionTypeCommand == ActionTypeCommand.SelfUse)
            {
                item.IsForInitialChargeWithRemainingSun = false;
            }
        }
    }

    private void CheckPriceProfitability(PriceType priceType)
    {
        if (priceType == PriceType.PurchasePrice)
        {
            foreach (var hour in BatteryControlHours)
            {
                if (hour.PurchasePrice < (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {
                    hour.IsForInitialChargeWithRemainingSun = true;
                }
            }
        }

        if (priceType == PriceType.SalesPrice)
        {
            foreach (var hour in BatteryControlHours)
            {
                if (hour.SalePrice < (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {
                    hour.IsForInitialChargeWithRemainingSun = true;
                }
            }
        }
    }

    private void AddMissingHoursFromInitialData()
    {
        foreach (var item in BatteryHoursSummer)
        {
            if (item.SpotPrice != null)
            {
                BatteryControlHours.Add(new BatteryControlHours
                {
                    ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                    InverterBatteryId = InverterBattery.Id,
                    SpotPriceMaxId = item.SpotPrice.Id,
                    SpotPriceMax = item.SpotPrice,
                    MaxPriceHour = item.SpotPrice.Time,
                    GroupNumber = 9999,
                    SalePrice = item.SalePrice,
                    PurchasePrice = item.PurchasePrice,
                    ForecastedConsumption = item.ForecastedConsumption,
                    ForecastedSolarRemaining = item.ForecastedSolarRemaining,
                    WaveNumber = DateTime.Now.Date == item.SpotPrice.DateTime.Date ? 1 : 2


                });
            }
           
        }

        BatteryHoursSummer.Clear();
    }

    private async Task ReduceBatteryRemaining()
    {
        var lastRecord = BatteryControlHours.LastOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse);
        var batteryConsumePower = (InverterBattery.DischargingPowerToGridKWh > Inverter.MaxSalesPowerCapacity ?
            Inverter.MaxSalesPowerCapacity
            : InverterBattery.DischargingPowerToGridKWh)
            * 1000;
      
        var maxSalesPriceForSelfUse = BatteryControlHours.Where(x => 
        x.PurchasePrice > (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
        && x.IsForInitialChargeWithRemainingSun == false).OrderByDescending(o => o.PurchasePrice).FirstOrDefault();

        if (maxSalesPriceForSelfUse == null)
        {
            return;
        }

        batteryConsumePower -= maxSalesPriceForSelfUse.ForecastedSolarRemaining;

        if (batteryConsumePower < 0)
        {
            batteryConsumePower = 0;
        }

        if (remainingBatteryLevel + maxSalesPriceForSelfUse.ForecastedConsumption > batteryConsumePower)
        {
            var selectedHourSalesPower = Convert.ToInt32(Inverter.MaxSalesPowerCapacity) * 1000;

            if (Inverter.UseFixedAvgHourlyWatts)
            {
                selectedHourSalesPower = selectedHourSalesPower + Inverter.FixedAvgHourlyWatts > Convert.ToInt32(Inverter.MaxPower) * 1000
                        ? Convert.ToInt32(Inverter.MaxPower) * 1000 : selectedHourSalesPower + Inverter.FixedAvgHourlyWatts;
            }
            else
            {
                var selectedHourAvgConsumption = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                                    && x.DayOfWeek == maxSalesPriceForSelfUse.SpotPriceMax.Date.DayOfWeek
                                                    && x.TimeHour == maxSalesPriceForSelfUse.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                if (selectedHourAvgConsumption != null)
                {
                    selectedHourSalesPower = selectedHourSalesPower + selectedHourAvgConsumption.AvgHourlyConsumption > Convert.ToInt32(Inverter.MaxPower) * 1000
                        ? Convert.ToInt32(Inverter.MaxPower) * 1000 : selectedHourSalesPower + selectedHourAvgConsumption.AvgHourlyConsumption;
                }
            }

            maxSalesPriceForSelfUse.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
            maxSalesPriceForSelfUse.ConsumeBatterySellPower = batteryConsumePower;
            maxSalesPriceForSelfUse.UsableWatts = await GetConsumeMaxSalesPower(maxSalesPriceForSelfUse);
            maxSalesPriceForSelfUse.IsForInitialChargeWithRemainingSun = true;


            remainingBatteryLevel -= batteryConsumePower - maxSalesPriceForSelfUse.ForecastedConsumption;
            maxSalesPriceForSelfUse.RemainingBattery = remainingBatteryLevel;
            await ReduceBatteryRemaining();
        }
    }

    private async Task<int> GetConsumeMaxSalesPower(BatteryControlHours batteryControlHours)
    {
        var selectedHourSalesPower = Convert.ToInt32(Inverter.MaxSalesPowerCapacity) * 1000;

        if (Inverter.UseFixedAvgHourlyWatts)
        {
            selectedHourSalesPower = selectedHourSalesPower + Inverter.FixedAvgHourlyWatts > Convert.ToInt32(Inverter.MaxPower) * 1000
                    ? Convert.ToInt32(Inverter.MaxPower) * 1000 : selectedHourSalesPower + Inverter.FixedAvgHourlyWatts;
        }
        else
        {
            var selectedHourAvgConsumption = await _dbContext.InverterHoursAvgConsumption.Where(x => x.InverterId == Inverter.Id
                                                && x.DayOfWeek == batteryControlHours.SpotPriceMax.Date.DayOfWeek
                                                && x.TimeHour == batteryControlHours.SpotPriceMax.Time).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

            if (selectedHourAvgConsumption != null)
            {
                selectedHourSalesPower = selectedHourSalesPower + selectedHourAvgConsumption.AvgHourlyConsumption > Convert.ToInt32(Inverter.MaxPower) * 1000
                    ? Convert.ToInt32(Inverter.MaxPower) * 1000 : selectedHourSalesPower + selectedHourAvgConsumption.AvgHourlyConsumption;
            }
        }

        return selectedHourSalesPower;
    }

    private async Task ProcessWhenSalesPriceIsHigherThanPurchasePrice()
    {
        RemoveInitialChargingFromSelfUse();
        //TODO:
        //Tuleb lisada muutuja, et võib vajada taastamist nii consume max kui ka self used
        //Teha lisaväli actiontypecommandile, et oleks näha millele taastada, kui vajadus on
        string initialListForComparingJSON = JsonConvert.SerializeObject(BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).ToList());
        var initialListForComparing = JsonConvert.DeserializeObject<List<BatteryControlHours>>(initialListForComparingJSON);


        // var batteryControlHoursWithSelfUse = BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).OrderByDescending(o => o.PurchasePrice).ToList();
        // var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse);
        var batteryControlHoursWithSelfUse = BatteryControlHours.Where(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower && x.ConsumeMaxProcessedForSummerMedium == false).OrderByDescending(o => o.PurchasePrice).ToList();
        var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x =>
         x.PurchasePrice > (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
         && x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower && x.ConsumeMaxProcessedForSummerMedium == false && x.PurchasePrice >= Company.ExpectedProfitForSelfUseOnlyInCents / 100);


        if (firstSuitableRecord != null)
        {
            firstSuitableRecord.ActionTypeCommandOriginal = firstSuitableRecord.ActionTypeCommand;
            firstSuitableRecord.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
            firstSuitableRecord.UsableWatts = Convert.ToInt32(Inverter.MaxPower) * 1000;
            firstSuitableRecord.SummerMediumUndo = true;
            firstSuitableRecord.ConsumeMaxProcessedForSummerMedium = true;
            firstSuitableRecord.ConsumeBatterySellPower = CalculateConsumeBatterySellPower(batteryControlHoursWithSelfUse, firstSuitableRecord);

            var calculatedList = await CalculateSelfConsumptionForMedium(batteryControlHoursWithSelfUse, initialListForComparing, firstSuitableRecord.ConsumeBatterySellPower, firstSuitableRecord, true);

            foreach (var calculatedRecord in calculatedList)
            {
                var existingRecord = BatteryControlHours.FirstOrDefault(r => r.SpotPriceMaxId == calculatedRecord.SpotPriceMaxId);
                if (existingRecord != null)
                {

                    int index = BatteryControlHours.IndexOf(existingRecord);
                    BatteryControlHours[index] = calculatedRecord;
                }
            }
        }
    }

    private async Task<List<BatteryControlHours>> CalculateSelfConsumptionSummerMedium(List<BatteryControlHours> inputList, List<BatteryControlHours> initialListForComparing, BatteryControlHours firstRecord, bool isFirst = false)
    {

        var outputList = inputList;

        if (remainingBatteryLevel <= 0)
        {
            return outputList;
        }

        foreach (var item in inputList)
        {
            if (item.IsForInitialChargeWithRemainingSun == false && item.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower
                && item.RemainingBattery <= 0)
            {
                item.ActionTypeCommandOriginal = item.ActionTypeCommand;
                item.SummerMediumUndo = true;
                item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                item.IsChanged = true;
            }
        }

        (double totalValueInitial, bool isSuccessfulInitial) = CalculateTotalValueMedium(initialListForComparing, firstRecord, isFirst);
        (double totalValueInput, bool isSuccessfulInput) = CalculateTotalValueMedium(inputList, firstRecord);

        if (isSuccessfulInitial == false
            || isSuccessfulInput == false)
        {
            foreach (var outputItem in outputList)
            {
                if (outputItem.SummerMediumUndo)
                {
                    outputItem.ActionTypeCommand = outputItem.ActionTypeCommandOriginal;
                    outputItem.SummerMediumUndo = false;
                }

            }

            firstRecord.ConsumeMaxProcessedForSummerMedium = true;

            return outputList;
        }


        while (totalValueInitial < totalValueInput
            || inputList.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse) == null)
        {

            foreach (var item in inputList)
            {
                item.IsChanged = false;
            }

            string initialListForComparingJSON = JsonConvert.SerializeObject(inputList);
            var initialListForComparingAfterChanges = JsonConvert.DeserializeObject<List<BatteryControlHours>>(initialListForComparingJSON);

            var batteryControlHoursWithSelfUse = inputList.Where(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower && x.ConsumeMaxProcessedForSummerMedium == false).OrderByDescending(o => o.PurchasePrice).ToList();
            var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower && x.ConsumeMaxProcessedForSummerMedium == false);

            if (firstSuitableRecord != null)
            {
                firstSuitableRecord.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                firstSuitableRecord.ConsumeMaxProcessedForSummerMedium = true;
                firstSuitableRecord.ConsumeBatterySellPower = CalculateConsumeBatterySellPower(batteryControlHoursWithSelfUse, firstSuitableRecord);
                firstSuitableRecord.UsableWatts = await GetConsumeMaxSalesPower(firstSuitableRecord);
                firstSuitableRecord.SummerMediumUndo = true;

                return await CalculateSelfConsumptionSummerMedium(batteryControlHoursWithSelfUse, initialListForComparingAfterChanges, firstSuitableRecord);
            }
            else
            {

                foreach (var item in inputList)
                {
                    if (item.IsChanged
                        && item.SalePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                    {
                        item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                    }

                }
                return inputList;
            }

        }

        return outputList;
    }

    private (double TotalValue, bool IsSuccessful) CalculateTotalValueMedium(List<BatteryControlHours> batteryControlHours, BatteryControlHours firstRecord, bool isFirst = false)
    {
        double totalValue = 0;
        double batteryConsumePower = Inverter.MaxSalesPowerCapacity * 1000;
        bool isSuccessful = true;

        batteryControlHours = batteryControlHours.OrderBy(x => x.SalePrice).ToList();

        foreach (var item in batteryControlHours)
        {
            if (!isFirst)
            {
                if (item.ActionTypeCommand == ActionTypeCommand.SelfUse && firstRecord.PurchasePrice > item.SalePrice)
                {
                    return (0, false);
                }

                batteryConsumePower -= remainingBatteryLevel;
                remainingBatteryLevel = 0;
                if (item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                {
                    batteryConsumePower -= item.ForecastedConsumption;
                    item.ActionTypeCommandOriginal = item.ActionTypeCommand;
                    item.SummerMediumUndo = true;
                    item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                    item.IsChanged = true;
                }


                if (batteryConsumePower < 0)
                {
                    break;
                }
            }

            switch (item.ActionTypeCommand)
            {
                case ActionTypeCommand.SelfUse:
                    totalValue += item.SalePrice * item.ForecastedConsumption;
                    break;

                case ActionTypeCommand.ConsumeBatteryWithMaxPower:
                    totalValue += item.PurchasePrice * item.ConsumeBatterySellPower;
                    break;
            }

        }

        return (totalValue / 1000, isSuccessful);
    }



    private async Task ProcessWhenNotEnoughtBatterySalesMax()
    {
        RemoveInitialChargingFromSelfUse();

        string initialListForComparingJSON = JsonConvert.SerializeObject(BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).ToList());

        //string initialListForComparingJSON = JsonConvert.SerializeObject(BatteryControlHours.Where(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand == ActionTypeCommand.ChargeWithRemainingSun || x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).ToList());
        var initialListForComparing = JsonConvert.DeserializeObject<List<BatteryControlHours>>(initialListForComparingJSON);


       // var batteryControlHoursWithSelfUse = BatteryControlHours.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse || x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower).OrderByDescending(o => o.PurchasePrice).ToList();
       // var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse);
          var batteryControlHoursWithSelfUse = BatteryControlHours.Where(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower).OrderByDescending(o => o.PurchasePrice).ToList();
          var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x =>
          x.PurchasePrice > (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
          && x.IsForInitialChargeWithRemainingSun == false 
          && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower);


        if (firstSuitableRecord != null)
        {
            firstSuitableRecord.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
            firstSuitableRecord.UsableWatts = await GetConsumeMaxSalesPower(firstSuitableRecord);
            firstSuitableRecord.ConsumeBatterySellPower = CalculateConsumeBatterySellPower(batteryControlHoursWithSelfUse, firstSuitableRecord);

            var calculatedList = await CalculateSelfConsumption(batteryControlHoursWithSelfUse, initialListForComparing, firstSuitableRecord.ConsumeBatterySellPower, true);

            foreach (var calculatedRecord in calculatedList)
            {
                var existingRecord = BatteryControlHours.FirstOrDefault(r => r.SpotPriceMaxId == calculatedRecord.SpotPriceMaxId);
                if (existingRecord != null)
                {

                    int index = BatteryControlHours.IndexOf(existingRecord);
                    BatteryControlHours[index] = calculatedRecord;
                }
            }
        }
    }

    private double CalculateConsumeBatterySellPower(List<BatteryControlHours> inputData, BatteryControlHours firstRecord)
    {
        var batteryDischargingPowerToGridKWh = InverterBattery.DischargingPowerToGridKWh > Inverter.MaxSalesPowerCapacity ? Inverter.MaxSalesPowerCapacity : InverterBattery.DischargingPowerToGridKWh;
        double batteryConsumePower = batteryDischargingPowerToGridKWh * 1000 + firstRecord.ForecastedConsumption - firstRecord.ForecastedSolarRemaining;
        var totalSelfUseConsumption = inputData.Where(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse).Sum(x => x.ForecastedConsumption);
        var lastConsumeBatteryWithMaxPower = inputData.LastOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.ConsumeBatteryWithMaxPower);

        if (lastConsumeBatteryWithMaxPower != null)
        {
            totalSelfUseConsumption += Convert.ToInt32(lastConsumeBatteryWithMaxPower.RemainingBattery);
        }


        if (totalSelfUseConsumption <= batteryConsumePower)
        {
            batteryConsumePower = totalSelfUseConsumption;
        }

        return batteryConsumePower;
    }

    private async Task<List<BatteryControlHours>> CalculateSelfConsumptionForMedium(List<BatteryControlHours> inputList, List<BatteryControlHours> initialListForComparing, double consumeBatterySellPower, BatteryControlHours consumeMaxRecord, bool isFirst = false)
    {
        var outputList = inputList;

        if (remainingBatteryLevel <= 0)
        {
            return outputList;
        }

        remainingBatteryForCalculation = remainingBatteryLevel - consumeBatterySellPower;
        // var remainingBatteryLevelForLoopCalc = remainingBatteryForCalculation;

        foreach (var item in inputList.OrderBy(x => x.SalePrice))
        {
            if (remainingBatteryForCalculation >= 0)
            {
                break;
            }

            if (item.ActionTypeCommand != ActionTypeCommand.ChargeWithRemainingSun && item.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower)
            //  && item.RemainingBattery <= 0)
            {
                //TODO: Vaja üle kanda ConsumeBatterySellPower algväärtus siia. Siis sellest lahutada remainingBatteryLevel ja vähendada self use seni kuni on arv 0

                if (item.SalePrice > consumeMaxRecord.PurchasePrice)
                {
                    return initialListForComparing;
                }

                item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                item.IsChanged = true;
                item.IsNeededForSummerMaxCalculation = true;
                remainingBatteryForCalculation += item.ForecastedConsumption;
            }
        }

        if (remainingBatteryForCalculation < 0)
        {
            return initialListForComparing;
        }


        double totalValueInitial = CalculateTotalValue(initialListForComparing, isFirst);

        foreach (var item in initialListForComparing)
        {
            if (!inputList.Any(x => x.MaxPriceHour == item.MaxPriceHour))
            {
                inputList.Add(item);
            }
        }

        double totalValueInput = CalculateTotalValue(inputList);


        while (remainingBatteryForCalculation > 0)
        {
            remainingBatteryLevel = remainingBatteryForCalculation;
            foreach (var item in inputList)
            {
                item.IsChanged = false;
            }

            string initialListForComparingJSON = JsonConvert.SerializeObject(inputList);
            var initialListForComparingAfterChanges = JsonConvert.DeserializeObject<List<BatteryControlHours>>(initialListForComparingJSON);

            var batteryControlHoursWithSelfUse = inputList.Where(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower).OrderByDescending(o => o.PurchasePrice).ToList();
            var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x =>
             x.PurchasePrice > (Company.ExpectedProfitForSelfUseOnlyInCents / 100)
             && x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower);

            if (firstSuitableRecord != null)
            {
                firstSuitableRecord.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                firstSuitableRecord.ConsumeBatterySellPower = CalculateConsumeBatterySellPower(batteryControlHoursWithSelfUse, firstSuitableRecord);
                firstSuitableRecord.UsableWatts = await GetConsumeMaxSalesPower(firstSuitableRecord);

                return await CalculateSelfConsumptionForMedium(batteryControlHoursWithSelfUse, initialListForComparingAfterChanges, firstSuitableRecord.ConsumeBatterySellPower, firstSuitableRecord, false);
            }
            else
            {

                foreach (var item in inputList)
                {
                    if (item.IsChanged
                        && item.SalePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                    {
                        item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                    }

                }
                return inputList;
            }

        }

        return outputList;
    }

    private async Task<List<BatteryControlHours>> CalculateSelfConsumption(List<BatteryControlHours> inputList, List<BatteryControlHours> initialListForComparing, double consumeBatterySellPower, bool isFirst = false)
    {
        var outputList = inputList;

        if (remainingBatteryLevel <= 0)
        {
            return outputList;
        }

        remainingBatteryForCalculation = remainingBatteryLevel - consumeBatterySellPower;
       // var remainingBatteryLevelForLoopCalc = remainingBatteryForCalculation;

        foreach (var item in inputList.OrderBy(x=> x.SalePrice))
        {
            if (remainingBatteryForCalculation >= 0)
            {
                break;
            }

            if (item.ActionTypeCommand != ActionTypeCommand.ChargeWithRemainingSun && item.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower)
              //  && item.RemainingBattery <= 0)
            {
                //TODO: Vaja üle kanda ConsumeBatterySellPower algväärtus siia. Siis sellest lahutada remainingBatteryLevel ja vähendada self use seni kuni on arv 0
                item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                item.IsChanged = true;
                item.IsNeededForSummerMaxCalculation = true;
                remainingBatteryForCalculation += item.ForecastedConsumption;
            }
        }

        if (remainingBatteryForCalculation < 0)
        {
            return initialListForComparing;
        }


        double totalValueInitial = CalculateTotalValue(initialListForComparing, isFirst);

        foreach (var item in initialListForComparing)
        {
            if (!inputList.Any(x=> x.MaxPriceHour == item.MaxPriceHour))
            {
                inputList.Add(item);
            }
        }

        double totalValueInput = CalculateTotalValue(inputList);


        while (totalValueInitial < totalValueInput
            || inputList.FirstOrDefault(x => x.ActionTypeCommand == ActionTypeCommand.SelfUse) == null)
        {
            remainingBatteryLevel = remainingBatteryForCalculation;

            foreach (var item in inputList)
            {
                item.IsChanged = false;
            }

            string initialListForComparingJSON = JsonConvert.SerializeObject(inputList);
            var initialListForComparingAfterChanges = JsonConvert.DeserializeObject<List<BatteryControlHours>>(initialListForComparingJSON);

            var batteryControlHoursWithSelfUse = inputList.Where(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower).OrderByDescending(o => o.PurchasePrice).ToList();
            var firstSuitableRecord = batteryControlHoursWithSelfUse.FirstOrDefault(x => x.IsForInitialChargeWithRemainingSun == false && x.ActionTypeCommand != ActionTypeCommand.ConsumeBatteryWithMaxPower);

            if (firstSuitableRecord != null)
            {
                firstSuitableRecord.ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower;
                firstSuitableRecord.ConsumeBatterySellPower = CalculateConsumeBatterySellPower(batteryControlHoursWithSelfUse, firstSuitableRecord);
                firstSuitableRecord.UsableWatts = await GetConsumeMaxSalesPower(firstSuitableRecord);

                return await CalculateSelfConsumption(batteryControlHoursWithSelfUse, initialListForComparingAfterChanges, firstSuitableRecord.ConsumeBatterySellPower, false);
            }
            else
            {

                foreach (var item in inputList)
                {
                    if (item.IsChanged
                        && item.SalePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                    {
                        item.ActionTypeCommand = ActionTypeCommand.SelfUse;
                    }
                   
                }
                return inputList;
            }

        }

        return outputList;
    }

    private double CalculateTotalValue(List<BatteryControlHours> batteryControlHours, bool isFirst = false)
    {
        double totalValue = 0;
        double batteryConsumePower = Inverter.MaxSalesPowerCapacity * 1000;

        batteryControlHours = batteryControlHours.OrderBy(x => x.SalePrice).ToList();

        foreach (var item in batteryControlHours)
        {
            /*
            if (!isFirst)
            {
                batteryConsumePower -= remainingBatteryLevel;
                remainingBatteryLevel = 0;
                if (item.ActionTypeCommand == ActionTypeCommand.SelfUse)
                {
                    batteryConsumePower -= item.ForecastedConsumption;
                    item.ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun;
                   // item.IsForInitialChargeWithRemainingSun = true;
                    item.IsChanged = true;                 
                }


                if (batteryConsumePower < 0)
                {
                    break;
                }
            }
            */

            switch (item.ActionTypeCommand)
            {
                case ActionTypeCommand.SelfUse:
                    totalValue += item.SalePrice * item.ForecastedConsumption;
                    break;

                case ActionTypeCommand.ConsumeBatteryWithMaxPower:
                    totalValue += item.PurchasePrice * item.ConsumeBatterySellPower;
                    break;

                case ActionTypeCommand.ChargeWithRemainingSun:
                    totalValue += item.SalePrice * item.ForecastedConsumption;
                    break;
            }

        }

        return totalValue / 1000;
    }

    private void CalculateRemainingBatteryLevel()
    {      

        var chargingAdditionalPercentage = InverterBattery.AdditionalTimeForBatteryChargingPercentage > 0 ? 1 + (InverterBattery.AdditionalTimeForBatteryChargingPercentage) : 1;
        var maxUsableBatteryWatts = (InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)) * chargingAdditionalPercentage; // - 20000; // - 50000;

 

        remainingBatteryLevel = mqttLogFromRedisRegular != null ?
            maxUsableBatteryWatts * ( (Convert.ToDouble(mqttLogFromRedisRegular.batterySOC) - Convert.ToDouble(InverterBattery.MinLevel)) / 100)
            : maxUsableBatteryWatts;       
    }

    private void ProcessInitialSelfUse()
    {
      //  CheckPriceProfitability(PriceType.SalesPrice);

        var maxTime = new TimeSpan(14, 0, 0);
        var maxDate = DateOnly.FromDateTime(DateTimeMax);
        var maxDateTimeForSelfUse = new DateTime(maxDate.Year, maxDate.Month, maxDate.Day, 23, 0, 0);

        if (BatteryControlHours != null && BatteryControlHours.Count > 0)
        {
            remainingBatteryLevel = BatteryControlHours.Max(x => x.RemainingBattery);
        }
        else
        {
            CalculateRemainingBatteryLevel();
        }      

        var selfUseRecords = BatteryHoursSummer.Where(x => x.DateTime <= maxDateTimeForSelfUse).OrderByDescending(o => o.PurchasePrice).ToList();

        foreach (var item in selfUseRecords)
        {
            if (item.SpotPrice != null)
            {
                if (item.ActionTypeCommand == ActionTypeCommand.SellRemainingSunNoCharging)
                {
                   

                    BatteryControlHours.Add(new BatteryControlHours
                    {
                        ActionTypeCommand = ActionTypeCommand.SellRemainingSunNoCharging,
                        InverterBatteryId = InverterBattery.Id,
                        SpotPriceMaxId = item.SpotPrice.Id,
                        SpotPriceMax = item.SpotPrice,
                        MaxPriceHour = item.SpotPrice.Time,
                        GroupNumber = 9999,
                        SalePrice = item.SalePrice,
                        PurchasePrice = item.PurchasePrice,
                        ForecastedConsumption = item.ForecastedConsumption,
                        ForecastedSolarRemaining = item.ForecastedSolarRemaining,
                        

                    });
                }
                else if (item.SalePrice < (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
                {
 
                    BatteryControlHours.Add(new BatteryControlHours
                    {
                        ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                        InverterBatteryId = InverterBattery.Id,
                        SpotPriceMaxId = item.SpotPrice.Id,
                        SpotPriceMax = item.SpotPrice,
                        MaxPriceHour = item.SpotPrice.Time,
                        GroupNumber = 9999,
                        SalePrice = item.SalePrice,
                        PurchasePrice = item.PurchasePrice,
                        ForecastedConsumption = item.ForecastedConsumption,
                        ForecastedSolarRemaining = item.ForecastedSolarRemaining,
                        WaveNumber = DateTime.Now.Date == item.SpotPrice.DateTime.Date ? 1 : 2
                    });
                }
                else
                {
                    if (remainingBatteryLevel > 0 && item.ForecastedSolarRemaining <= 0)
                    {
                        remainingBatteryLevel -= item.ForecastedConsumption;

                        BatteryControlHours.Add(new BatteryControlHours
                        {
                            ActionTypeCommand = ActionTypeCommand.SelfUse,
                            InverterBatteryId = InverterBattery.Id,
                            SpotPriceMaxId = item.SpotPrice.Id,
                            SpotPriceMax = item.SpotPrice,
                            MaxPriceHour = item.SpotPrice.Time,
                            GroupNumber = 9999,
                            RemainingBattery = remainingBatteryLevel,
                            SalePrice = item.SalePrice,
                            PurchasePrice = item.PurchasePrice,
                            ForecastedConsumption = item.ForecastedConsumption,
                            ForecastedSolarRemaining = item.ForecastedSolarRemaining,
                            MaxAvgHourlyConsumptionOriginal = item.AvgConsumption
                        });
                    }
                    else
                    {
                        BatteryControlHours.Add(new BatteryControlHours
                        {
                            ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                            InverterBatteryId = InverterBattery.Id,
                            SpotPriceMaxId = item.SpotPrice.Id,
                            SpotPriceMax = item.SpotPrice,
                            MaxPriceHour = item.SpotPrice.Time,
                            GroupNumber = 9999,
                            SalePrice = item.SalePrice,
                            PurchasePrice = item.PurchasePrice,
                            ForecastedConsumption = item.ForecastedConsumption,
                            ForecastedSolarRemaining = item.ForecastedSolarRemaining,
                            WaveNumber = DateTime.Now.Date == item.SpotPrice.DateTime.Date ? 1 : 2

                        });
                    }
                }

                BatteryHoursSummer.Remove(item);
            }

        }
    }

    private void ProcessInitialChargeWithRemainingSun()
    {
       
        var maxUsableBatteryWattsOriginal = (InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10));
        var chargingAdditionalPercentage = InverterBattery.AdditionalTimeForBatteryChargingPercentage > 0 ? 1 + (InverterBattery.AdditionalTimeForBatteryChargingPercentage) : 1;
        var maxUsableBatteryWatts = (InverterBattery.CapacityKWh * ((InverterBattery.MaxLevel - InverterBattery.MinLevel) * 10)) * chargingAdditionalPercentage; // - 20000; // - 50000;


         var remainingBatteryWatts = mqttLogFromRedisRegular != null ?
            maxUsableBatteryWatts * (Convert.ToDouble(mqttLogFromRedisRegular.batterySOC) / 100)
            : maxUsableBatteryWatts;

       // remainingBatteryWatts -= 35000; //Aku suurendamine

        var recordsWithRemainingSolar = new List<BatteryHoursSummer>();
      //  var currentDateTime = DateTime.Now;
        var currenRecord = BatteryHoursSummer.FirstOrDefault(x => x.ForecastedSolarRemaining > 0 && x.Date == DateOnly.FromDateTime(CurrentDateTime)
                && x.Time == new TimeSpan(CurrentDateTime.Hour + 1, 0, 0));

        if (currenRecord != null)
        {
            recordsWithRemainingSolar = BatteryHoursSummer.Where(x=> x.DateTime >= currenRecord.DateTime).TakeWhile(x => x.ForecastedSolarRemaining > 0).OrderBy(x => x.PurchasePrice).ToList();
          
        }
       
        var solarChargingPower = InverterBattery.ChargingPowerFromSolarKWh * 1000;

        while (recordsWithRemainingSolar.Count > 0)
        {
            var powerToUseForCharging = recordsWithRemainingSolar.FirstOrDefault().ForecastedSolarRemaining > solarChargingPower ? solarChargingPower : recordsWithRemainingSolar.FirstOrDefault().ForecastedSolarRemaining;

            
            if (remainingBatteryWatts < maxUsableBatteryWatts)
            {
                remainingBatteryWatts += powerToUseForCharging;

                if (remainingBatteryWatts > maxUsableBatteryWatts)
                {
                    remainingBatteryWatts = maxUsableBatteryWatts;
                }

                BatteryControlHours.Add(new BatteryControlHours
                {
                    ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun,
                    InverterBatteryId = InverterBattery.Id,
                    SpotPriceMaxId = recordsWithRemainingSolar.FirstOrDefault().SpotPrice.Id,
                    SpotPriceMax = recordsWithRemainingSolar.FirstOrDefault().SpotPrice,
                    MaxPriceHour = recordsWithRemainingSolar.FirstOrDefault().SpotPrice.Time,
                    GroupNumber = 9999,
                    RemainingBattery = remainingBatteryWatts,
                    SalePrice = recordsWithRemainingSolar.FirstOrDefault().SalePrice,
                    PurchasePrice = recordsWithRemainingSolar.FirstOrDefault().PurchasePrice,
                    ForecastedConsumption = recordsWithRemainingSolar.FirstOrDefault().ForecastedConsumption,
                    ForecastedSolarRemaining = recordsWithRemainingSolar.FirstOrDefault().ForecastedSolarRemaining,
                    IsForInitialChargeWithRemainingSun = recordsWithRemainingSolar.FirstOrDefault().PurchasePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100) ? false : true,
                    WaveNumber = 1

                });

                BatteryHoursSummer.Remove(recordsWithRemainingSolar.First());            

            }
            else if (recordsWithRemainingSolar.FirstOrDefault().PurchasePrice >= (Company.ExpectedProfitForSelfUseOnlyInCents / 100))
            {
                var batterySummerRecord = BatteryHoursSummer.FirstOrDefault(x => x.DateTime == recordsWithRemainingSolar.FirstOrDefault().DateTime);
                batterySummerRecord.ActionTypeCommand = ActionTypeCommand.SellRemainingSunNoCharging;
            }

            recordsWithRemainingSolar.Remove(recordsWithRemainingSolar.First());       
        }

        remainingBatteryLevelList.Add(new BatteryWave
        {
            RemainingBattery = remainingBatteryWatts,
            WaveNumber = 1

        });
    }

    private async Task<List<BatteryHoursSummer>> ProcessAverageConsumption(List<BatteryHoursSummer> batteryHoursSummers)
    {
        if (Inverter.UseFixedAvgHourlyWatts)
        {
            foreach (var item in batteryHoursSummers)
            {
                item.AvgConsumption = Inverter.FixedAvgHourlyWatts;
                item.ForecastedConsumption = item.AvgConsumption;
            }

            return batteryHoursSummers;
        }
        else
        {
            var inverterHoursAvgConsumption = await GetAverageConsumptionForWeekDays();

            if (inverterHoursAvgConsumption.Count > 0)
            {
                foreach (var item in batteryHoursSummers)
                {
                    if (inverterHoursAvgConsumption.Where(x => x.TimeHour == item.Time && x.DayOfWeek == item.Date.DayOfWeek).ToList().Count > 0)
                    {
                        item.AvgConsumption = (int)inverterHoursAvgConsumption.Where(x => x.TimeHour == item.Time && x.DayOfWeek == item.Date.DayOfWeek).ToList().Average(x => x.AvgHourlyConsumption);
                        item.ForecastedConsumption = item.AvgConsumption;
                    }
                    else
                    {
                        item.AvgConsumption = 0;
                        item.ForecastedConsumption = 0;
                    }

                }
            }
        }

        return batteryHoursSummers;
    }

    private async Task<bool> ProcessWeatherRequest()
    {
        var solarPanelCapacityRecordForDay = await _dbContext.SolarPanelCapacity.CountAsync(x => x.SolarPanelsDirecation == Inverter.SolarPanelsDirecation);
        TimeSpan difference = DateTimeMax - DateTimeMin;
        int numberOfWeatherRecordExpected = (difference.Days + 1) * solarPanelCapacityRecordForDay;

        var solarPanelCapacity = await _dbContext.SolarPanelCapacity.ToListAsync();
        var countrySolarCapacity = await _dbContext.CountrySolarCapacity.Where(x => x.CountryId == Company.CountryId).ToListAsync();

        WeatherProcessingService weatherProcessingService = new WeatherProcessingService(Company, Inverter, DateOnly.FromDateTime(DateTime.Now.AddDays(1)), WeatherApiComService, solarPanelCapacity, countrySolarCapacity);
        await weatherProcessingService.ProcessCurrenDateWeatherData();
        await weatherProcessingService.ProcessWeatherData();

        var numberOfWeatherDataRecords = await _dbContext.WeatherForecastData.CountAsync(x => x.InverterId == Inverter.Id && x.Date >= DateOnly.FromDateTime(DateTimeMin) && x.Date <= DateOnly.FromDateTime(DateTimeMax));

        if (numberOfWeatherDataRecords >= numberOfWeatherRecordExpected)
        {
            return true;
        }
        else
        {
            return false;
        }
     
    }

    private async Task<List<BatteryHoursSummer>> ProcessWeatherData(List<BatteryHoursSummer> batteryHoursSummers)
    {
        var weatherData = await _dbContext.WeatherForecastData.Where(x => x.InverterId == Inverter.Id && x.Date >= DateOnly.FromDateTime(DateTimeMin) && x.Date <= DateOnly.FromDateTime(DateTimeMax)).ToListAsync();

        foreach (var item in batteryHoursSummers)
        {
            var weatherItem = weatherData.FirstOrDefault(x => x.DateTime == item.DateTime);

            if (weatherItem != null)
            {
                var resultConsumption = item.AvgConsumption - weatherItem.EstimatedSolarPower;
                item.ForecastedConsumption = resultConsumption < 0 ? 0 : resultConsumption;

                if (resultConsumption < 0)
                {
                    item.ForecastedSolarRemaining = Math.Abs(resultConsumption);
                }

                item.EstimatedSolar = weatherItem.EstimatedSolarPower;
            }           
        }

        return batteryHoursSummers;
    }

    private async Task<List<BatteryHoursSummer>> ProcessPrices(List<BatteryHoursSummer> batteryHoursSummers)
    {
        var spotPrices = await _dbContext.SpotPrice.Where(x => x.RegionId == Company.RegionId && x.Date >= DateOnly.FromDateTime(DateTimeMin) && x.Date <= DateOnly.FromDateTime(DateTimeMax)).ToListAsync();

        foreach (var item in batteryHoursSummers)
        {
            var spotPrice = spotPrices.FirstOrDefault(x => x.DateTime == item.DateTime);

            if (spotPrice != null)
            {
                item.PurchasePrice = (double)PriceCostHelper.CalculateMaxPriceWithPurchaseMarginCosts(spotPrice, Company);
                item.SalePrice = (double)PriceCostHelper.CalculatePriceWithSalesMarginCosts(spotPrice, Company);
                item.SpotPrice = spotPrice;
            }
        }

        return batteryHoursSummers;
    }


    private List<BatteryHoursSummer> BuildInitialData()
    {
        var batteryHoursSummers = new List<BatteryHoursSummer>();

        var runningDateTime = DateTimeMin;

        while (runningDateTime <= DateTimeMax)
        {
            var batteryHoursSummer = new BatteryHoursSummer
            {
                DateTime = runningDateTime,
                AvgConsumption = 0,
                ForecastedConsumption = 0,
                PurchasePrice = 0,
                SalePrice = 0,
                ForecastedSolarRemaining = 0
            };

            batteryHoursSummers.Add(batteryHoursSummer);

            runningDateTime = runningDateTime.AddHours(1);
        }

        return batteryHoursSummers;
    }

    private async Task<List<InverterHoursAvgConsumption>> GetAverageConsumptionForWeekDays()
    {
        HashSet<DayOfWeek> daysOfWeekInPeriod = GetDaysOfWeekInPeriod();

        var inverterHoursAvgConsumption = await _dbContext.InverterHoursAvgConsumption.Where(x => daysOfWeekInPeriod.Contains(x.DayOfWeek) && x.InverterId == InverterBattery.InverterId).ToListAsync();

        inverterHoursAvgConsumption = inverterHoursAvgConsumption
                                       .GroupBy(x => new { x.DateCalculated, x.TimeHour, x.DayOfWeek })
                                        .Select(g => g.OrderByDescending(x => x.DateCalculated).FirstOrDefault())
                                        .ToList()!;

        return inverterHoursAvgConsumption;
    }

    private HashSet<DayOfWeek> GetDaysOfWeekInPeriod()
    {
        HashSet<DayOfWeek> daysOfWeekInPeriod = new HashSet<DayOfWeek>();

        for (DateTime date = DateTimeMin; date < DateTimeMax.AddDays(1); date = date.AddDays(1))
        {
            daysOfWeekInPeriod.Add(date.DayOfWeek);
        }

        return daysOfWeekInPeriod;
    }

    public async Task<bool> GetAverageConsumptionForDailyHours(int companyId, int registeredInverterId, int numberOfDaysBack)
    {
        _dbContext.Database.SetCommandTimeout(300);
        var inverter = await _dbContext.Inverter.FirstOrDefaultAsync(x => x.RegisteredInverterId == registeredInverterId);

        if (inverter == null)
        {
            return false;
        }


        if (inverter.UseFixedAvgHourlyWatts)
        {
            return true;
        }

        var dateCalculated = DateOnly.FromDateTime(DateTime.Now);
        DateTime fourWeeksAgo = DateTime.Now.AddDays(-numberOfDaysBack);

        List<TimeSpan> expectedTimeSpans = Enumerable.Range(0, TotalHours)
         .Select(hour => TimeSpan.FromHours(hour))
         .ToList();

        var subQuery = await _dbContext.SofarStateHourlyTemp
        .Where(x =>
            x.Date >= DateOnly.FromDateTime(fourWeeksAgo)
            && x.Date < DateOnly.FromDateTime(DateTime.Now.AddDays(-1))
            && x.CompanyId == companyId
            && x.RegisteredInverterId == registeredInverterId)
        .GroupBy(x => new { x.Date, x.TimeHour })
        .Select(g => new
        {
            Date = (DateOnly)g.Key.Date!,
            g.Key.TimeHour,
            HourlyConsumption = g.Average(x => x.consumption),
        })
        .ToListAsync();


        var result = subQuery
        .GroupBy(x => new { WeekDay = x.Date.DayOfWeek, x.TimeHour })
        .Select(g => new InverterHoursAvgConsumption
        {
            DayOfWeek = g.Key.WeekDay,
            TimeHour = g.Key.TimeHour,
            AvgHourlyConsumption = (int)g.Average(x => x.HourlyConsumption),
            CompanyId = companyId,
            RegisteredInverterId = registeredInverterId,
            DateCalculated = dateCalculated,
            InverterId = inverter.Id

        })
        
        .OrderBy(x => x.DayOfWeek)
        .ThenBy(x => x.TimeHour)
        .ToList();


        if (result.Count > 0)
        {
            var averageOfDay = result.Average(x => x.AvgHourlyConsumption);

            foreach (var mandatoryTime in expectedTimeSpans)
            {
                if (!result.Any(x => x.TimeHour == mandatoryTime))
                {
                    result.Add(new InverterHoursAvgConsumption
                    {
                        DayOfWeek = result.FirstOrDefault().DayOfWeek,
                        TimeHour = mandatoryTime,
                        AvgHourlyConsumption = (int)averageOfDay,
                        CompanyId = companyId,
                        RegisteredInverterId = registeredInverterId,
                        DateCalculated = dateCalculated,
                        InverterId = inverter.Id
                    });
                }
            }
        }

        if (result.Count < TotalHours)
        {
            return false;
        }

        foreach (var item in result)
        {
            var exists = await _dbContext.InverterHoursAvgConsumption.AnyAsync(e =>
            e.InverterId == item.InverterId &&
            e.RegisteredInverterId == item.RegisteredInverterId &&
            e.DateCalculated == item.DateCalculated &&
            e.DayOfWeek == item.DayOfWeek &&
            e.TimeHour == item.TimeHour);

            if (!exists)
            {
                _dbContext.InverterHoursAvgConsumption.Add(item);
            }

        }

        await _dbContext.SaveChangesAsync();

        return true;
    }
}
