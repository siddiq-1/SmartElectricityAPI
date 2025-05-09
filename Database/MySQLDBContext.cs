using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartElectricityAPI.Dto;
using SmartElectricityAPI.Engine;
using SmartElectricityAPI.Enums;
using SmartElectricityAPI.Models;
using SmartElectricityAPI.Models.Fusebox;
using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.Database
{
    public class MySQLDBContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<CompanyUsers> CompanyUsers { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<Sensor> Sensor { get; set; }
        public DbSet<Switch> Switch { get; set; }
        public DbSet<SwitchGroup> SwitchGroup { get; set; }
        public DbSet<SpotPrice> SpotPrice { get; set; }
        public DbSet<DeviceHoursNoCalculation> DeviceHoursNoCalculation { get; set; }
        public DbSet<DeviceCompanyHours> DeviceCompanyHours { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Inverter> Inverter { get; set; }
        public DbSet<InverterCompanyHours> InverterCompanyHours { get; set; }
        public DbSet<InverterType> InverterType { get; set; }
        public DbSet<InverterTypeCommands> InverterTypeCommands { get; set; }
        public DbSet<InverterCompanyCommandsPayload> InverterCompanyCommandsPayload { get; set; }
        public DbSet<CompanyHourlyFees> CompanyHourlyFees { get; set; }
        public DbSet<CompanyHourlyFeesTransactions> CompanyHourlyFeesTransactions { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Region> Region { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<ErrorLog> ErrorLog { get; set; }
        public DbSet<RegisteredInverter> RegisteredInverter { get; set; }
        public DbSet<InverterTypeListenTopics> InverterTypeListenTopics { get; set; }
        public DbSet<InverterPublishedMessages> InverterPublishedMessages { get; set; }
        public DbSet<SofarState> SofarState { get; set; }
        public DbSet<InverterBattery> InverterBattery { get; set; }
        public DbSet<InverterTypeActions> InverterTypeActions { get; set; }
        public DbSet<InverterTypeCompanyActions> InverterTypeCompanyActions { get; set; }
        public DbSet<InverterHoursAvgConsumption> InverterHoursAvgConsumption { get; set; }
        public DbSet<BatteryControlHours> BatteryControlHours { get; set; }
        public DbSet<MqttMessageLog> MqttMessageLog { get; set; }
        public DbSet<SofarStateBuffer> SofarStateBuffer { get; set; }
        public DbSet<MqttUsers> MqttUsers { get; set; }
        public DbSet<SofarStateHourly> SofarStateHourly { get; set; }
        public DbSet<SofarStateHourlyTemp> SofarStateHourlyTemp { get; set; }
        public DbSet<SolarPanelCapacity> SolarPanelCapacity { get; set; }
        public DbSet<CountrySolarCapacity> CountrySolarCapacity { get; set; }
        public DbSet<WeatherForecastData> WeatherForecastData { get; set; }
        public DbSet<CompanyFixedPrice> CompanyFixedPrice { get; set; }
        public DbSet<CompanyFixedPriceTransactions> CompanyFixedPriceTransactions { get; set; }
        public DbSet<FuseBoxMessageHeader> FuseBoxMessageHeader { get; set; }
        public DbSet<FuseBoxSchedRegMsg> FuseBoxSchedRegMsg { get; set; }
        public DbSet<FuseBoxRealTimeMsg> FuseBoxRealTimeMsg { get; set; }
        public DbSet<FuseBoxMsgLogger> FuseBoxMsgLogger { get; set; }
        public DbSet<FuseBoxPowSet> FuseBoxPowSet { get; set; }
        public DbSet<SwitchModel> SwitchModel { get; set; }
        public DbSet<SwitchModelParameters> SwitchModelParameters { get; set; }
        public DbSet<CountryVatRange> CountryVatRange { get; set; }
        
        public MySQLDBContext(DbContextOptions<MySQLDBContext> options)
: base(options)
        {

        }

        private void processCreatedModifiedFields()
        {
            var entries = ChangeTracker
        .Entries()
        .Where(e => e.Entity is BaseEntity && (
            e.State == EntityState.Added
            || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.Now;
                }

                if (entityEntry.Entity.GetType() == typeof(SpotPrice))
                {
                    ((SpotPrice)entityEntry.Entity).Date = DateOnly.FromDateTime(((SpotPrice)entityEntry.Entity).DateTime);
                    ((SpotPrice)entityEntry.Entity).Time = ((SpotPrice)entityEntry.Entity).DateTime.TimeOfDay;
                }

            }
        }

        public override int SaveChanges()
        {
            processCreatedModifiedFields();

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            processCreatedModifiedFields();
            int result = 0;
            try
            {
                result = await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    await Console.Out.WriteLineAsync($"SQL: {ex.InnerException.Message}");
                }
              

                /*
                if (ex.InnerException != null)
                {
                    DbLogger.PostLog("Medium", $"SQL: {ex.InnerException.Message}");
                }
                */
            }
           

            return result;
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithOne()
                .HasForeignKey<User>(u => u.SelectedCompanyId);

            modelBuilder.Entity<Permission>()
            .HasData(
             new Permission { Id = 1, Level = Level.Administrator },
             new Permission { Id = 2, Level = Level.User },
             new Permission { Id = 3, Level = Level.Moderator });

            modelBuilder.Entity<Country>()
            .HasData(
             new Country { Id = 1, Name = "Estonia" },
             new Country { Id = 2, Name = "Sweden" },
             new Country { Id = 3, Name = "Finland" },
             new Country { Id = 4, Name = "Norway" },
             new Country { Id = 5, Name = "Denmark" },
             new Country { Id = 6, Name = "Latvia" },
             new Country { Id = 7, Name = "Lithuania" },
             new Country { Id = 8, Name = "Netherlands" });

            modelBuilder.Entity<Region>()
            .HasData(
             new Region { Id = 1, CountryId = 1, Abbreviation = "EE", OffsetHoursFromEstonianTime = 0 },
             new Region { Id = 2, CountryId = 2, Abbreviation = "SE1", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 3, CountryId = 2, Abbreviation = "SE2", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 4, CountryId = 2, Abbreviation = "SE3", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 5, CountryId = 2, Abbreviation = "SE4", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 6, CountryId = 3, Abbreviation = "FI", OffsetHoursFromEstonianTime = 0 },
             new Region { Id = 7, CountryId = 4, Abbreviation = "NO1", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 8, CountryId = 4, Abbreviation = "NO2", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 9, CountryId = 4, Abbreviation = "NO3", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 10, CountryId = 4, Abbreviation = "NO4", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 11, CountryId = 4, Abbreviation = "NO5", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 12, CountryId = 5, Abbreviation = "DK1", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 13, CountryId = 5, Abbreviation = "DK2", OffsetHoursFromEstonianTime = 1 },
             new Region { Id = 14, CountryId = 6, Abbreviation = "LV", OffsetHoursFromEstonianTime = 0 },
             new Region { Id = 15, CountryId = 7, Abbreviation = "LT", OffsetHoursFromEstonianTime = 0 },
             new Region { Id = 16, CountryId = 8, Abbreviation = "NL", OffsetHoursFromEstonianTime = 1 });


            modelBuilder.Entity<User>()
            .HasData(
             new User { Id = 1, Username = "Risto", Password = "$2a$12$P.UjQjBRvyWpLFQmdrvNa.UHkbA8s2fw6mMD27NIrMivaQ9BKNrOm", Email = "risto.roots@gmail.com", SelectedCompanyId = 1, IsAdmin = true },
             new User { Id = 2, Username = "Virgo", Password = "$2a$12$ZCj.JC9gyEjorrDypRllcu.822dPZAx/KVrkkAift.oF4EiYhj6YW", Email = "virgo.tuul@gmail.com", SelectedCompanyId = 1, IsAdmin = true });


            modelBuilder.Entity<Company>()
            .HasData(
             new Company {
                 Id = 1,
                 Name = "Jäämari",
                 Address = "Posti",
                 NetworkServiceFeeNightTime = 0.0505,
                 BrokerPurchaseMargin = 0.008,
                 NetworkServiceFeeDayTime = 0.0868,
                 BrokerSalesMargin = 0.072,
                 CountryId = 1,
                 RegionId = 1,
                 ExpectedProfitForSelfUseOnlyInCents = 5,
                 DayStartTime = new TimeSpan(7,0,0),
                 DayEndTime = new TimeSpan(22,0,0),
                 NightStartTime = new TimeSpan(22,0,0),
                 NightEndTime = new TimeSpan(7,0,0)},
             new Company {
                 Id = 2,
                 Name = "TM ERP Solutions OÜ",
                 Address = "Kiriku tee",
                 NetworkServiceFeeNightTime = 0.0605,
                 BrokerSalesMargin = 0.082,
                 CountryId = 1,
                 RegionId = 1});

            modelBuilder.Entity<CompanyUsers>()
            .HasData(
             new CompanyUsers { Id = 1, CompanyId = 1, UserId = 1, PermissionId = 1 },
             new CompanyUsers { Id = 2, CompanyId = 1, UserId = 2, PermissionId = 1 },
             new CompanyUsers { Id = 3, CompanyId = 2, UserId = 2, PermissionId = 1 },
             new CompanyUsers { Id = 4, CompanyId = 2, UserId = 1, PermissionId = 1 });

            modelBuilder.Entity<Device>()
            .HasData(
             new Device
             {
                 Id = 1,
                 CompanyId = 1,
                 Name = "Õhk-Vesi Põrand",
                 FuseboxForcedOff = true,
                 FuseboxForcedOn = true,
                 MaxStopHoursIn24h = 8,
                 MaxStopHoursConsecutive = 8,
                 MaxForcedOnHoursIn24h = 10,
                 ForcedOnPercentageForComingHourToEnable = 50,
                 ForcedOn = true,
                 ForcedOff = true,
                 MediumOn = false,
                 TemperatureInStandardMode = null,
                 TemperatureInForcedOnMode = null,
                 FirstHourPercentageKwPriceRequirementBeforeHeating = 10
             },
            new Device
            {
                Id = 2,
                CompanyId = 1,
                Name = "Õhk-Vesi Boiler",
                FuseboxForcedOff = true,
                FuseboxForcedOn = true,
                MaxStopHoursIn24h = 6,
                MaxStopHoursConsecutive = 2,
                MaxForcedOnHoursIn24h = 10,
                ForcedOnPercentageForComingHourToEnable = 50,
                ForcedOn = true,
                ForcedOff = true,
                MediumOn = false,
                TemperatureInStandardMode = null,
                TemperatureInForcedOnMode = null,
                FirstHourPercentageKwPriceRequirementBeforeHeating = 20
            },
            new Device
            {
                 Id = 3,
                 CompanyId = 1,
                 Name = "Tava boiler",
                 FuseboxForcedOff = true,
                 FuseboxForcedOn = true,
                 MaxStopHoursIn24h = 6,
                 MaxStopHoursConsecutive = 3,
                 MaxForcedOnHoursIn24h = 10,
                 ForcedOnPercentageForComingHourToEnable = 100,
                 ForcedOn = true,
                 ForcedOff = true,
                 MediumOn = false,
                 TemperatureInStandardMode = 55,
                 TemperatureInForcedOnMode = 65,
                 FirstHourPercentageKwPriceRequirementBeforeHeating = 43
             },
            new Device
            {
                Id = 4,
                CompanyId = 1,
                Name = "Külmik",
                FuseboxForcedOff = false,
                FuseboxForcedOn = true,
                MaxStopHoursIn24h = 0,
                MaxStopHoursConsecutive = 0,
                MaxForcedOnHoursIn24h = 2,
                ForcedOnPercentageForComingHourToEnable = 200,
                ForcedOn = true,
                ForcedOff = false,
                MediumOn = false,
                TemperatureInStandardMode = null,
                TemperatureInForcedOnMode = null,
                FirstHourPercentageKwPriceRequirementBeforeHeating = 30
            }
            );

            modelBuilder.
            Entity<Sensor>
            (entity => {
                entity.Property(s => s.DeviceActionType)
              .HasConversion<string>();
            });

            modelBuilder.Entity<SwitchModel>()
            .HasIndex(u => u.Name)
            .IsUnique(true);

            modelBuilder.Entity<Sensor>()
.HasData(
 new Sensor { Id = 1, CompanyId = 1, DeviceId = 2, SwitchModelId = 1, Name = "Normal ON", Description = "Klemmid 9-10", Topic = "mqttnet/samples/topic/2", BroadcastToFusebox = false },
 new Sensor { Id = 2, CompanyId = 1, DeviceId = 2, SwitchModelId = 1, Name = "Forced ON", Description = "Klemmid 5-6", Topic = "mqttnet/samples/topic/3", BroadcastToFusebox = false },
 new Sensor { Id = 3, CompanyId = 1, DeviceId = 2, SwitchModelId = 1, Name = "Boiler", Description = "Relee Õhk-Vesi 2 seadmes", BroadcastToFusebox = false },
 new Sensor { Id = 4, CompanyId = 1, DeviceId = 1, SwitchModelId = 1, Name = "Õhk-Vesi Põrand Off", Description = "Õhk-Vesi Põrand voolutarve", Topic = "shellyplus1-virgo1/rpc", DeviceActionType = DeviceActionType.Off, BroadcastToFusebox = false });

            modelBuilder.Entity<SwitchGroup>()
            .HasData(
             new SwitchGroup { Id = 1, CompanyId = 1, Name = "Forced Off" });

            modelBuilder.
            Entity<Switch>
            (entity => {
                entity.Property(s => s.DeviceActionType)
              .HasConversion<string>();
            });

            modelBuilder.Entity<SwitchModel>()
                .HasData(
                 new SwitchModel { Id = 1, Name = "Shelly Plus 1 ver 000" },
                 new SwitchModel { Id = 2, Name = "Shelly Plug S ver 000" });

            modelBuilder.
                Entity<SwitchModelParameters>
                (entity => {
                    entity.Property(s => s.DeviceActionType)
                  .HasConversion<string>();
                });

            modelBuilder.Entity<SwitchModelParameters>()
    .HasData(
      new SwitchModelParameters { Id = 1, SwitchModelId = 1, DeviceActionType = DeviceActionType.On, Payload = "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": true\r\n  }\r\n}" },
      new SwitchModelParameters { Id = 2, SwitchModelId = 1, DeviceActionType = DeviceActionType.Off, Payload = "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": true\r\n  }\r\n}" },
      new SwitchModelParameters { Id = 3, SwitchModelId = 1, DeviceActionType = DeviceActionType.None, Payload = "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": false\r\n  }\r\n}" });



            modelBuilder.Entity<Switch>()
            .HasData(
             new Switch {
                 Id = 1,
                 CompanyId = 1,
                 DeviceId = 1,
                 SensorId = 4,
                 SwitchModelId = 1
             },
              new Switch
              {
                  Id = 2,
                  CompanyId = 1,
                  DeviceId = 1,
                  SensorId = 4,
                  SwitchModelId = 1
              });



            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(true);  

            modelBuilder.
                Entity<Permission>
                (entity => {
                    entity.Property(s => s.Level)
                  .HasConversion<string>();
                });

            modelBuilder.
                Entity<DeviceCompanyHours>
                (entity => {
                    entity.Property(s => s.DeviceActionType)
                  .HasConversion<string>();
                });

            modelBuilder.Entity<DeviceCompanyHours>(b =>
            {
                b.HasIndex(e => new { e.DeviceId, e.SpotPriceId, e.CompanyId, e.DeviceActionType }).IsUnique();
            });

            modelBuilder.
                Entity<Inverter>
                (entity => {
                    entity.Property(s => s.SolarPanelsDirecation)
                  .HasConversion<string>();
                    entity.Property(s => s.CalculationFormula)
                   .HasConversion<string>();
                });

            modelBuilder.
                Entity<SolarPanelCapacity>
                (entity => {
                    entity.Property(s => s.SolarPanelsDirecation)
                  .HasConversion<string>();
                });

            modelBuilder.
            Entity<InverterCompanyHours>
            (entity => {
                entity.Property(s => s.ActionType)
              .HasConversion<string>();
            });

            modelBuilder.
            Entity<InverterTypeCommands>
            (entity => {
                entity.Property(s => s.ActionType)
              .HasConversion<string>();
            });

            modelBuilder.
                Entity<InverterTypeActions>
                (entity => {
                    entity.Property(s => s.ActionType)
                  .HasConversion<string>();
                    entity.Property(s => s.ActionTypeCommand)
                  .HasConversion<string>();
                });

            modelBuilder.
            Entity<InverterTypeCompanyActions>
            (entity => {
                entity.Property(s => s.ActionType)
              .HasConversion<string>();
                entity.Property(s => s.ActionTypeCommand)
              .HasConversion<string>();
            });

            modelBuilder.
            Entity<InverterHoursAvgConsumption>
            (entity => {
                entity.Property(s => s.DayOfWeek)
              .HasConversion<string>();
            });

            modelBuilder.
                Entity<BatteryControlHours>
                (entity => {
                    entity.Property(s => s.ActionTypeCommand)
                  .HasConversion<string>();
                });

            modelBuilder.
                Entity<MqttMessageLog>
                (entity => {
                    entity.Property(s => s.ActionTypeCommand)
                  .HasConversion<string>();
                    entity.Property(s => s.MqttMessageOrigin)
                 .HasConversion<string>();
                    entity.Property(s => s.Direction)
                .HasConversion<string>();
                    entity.Property(s => s.MQttMessageType)
                    .HasConversion<string>();

                });

            modelBuilder.
                Entity<FuseBoxMessageHeader>
                (entity => {
                    entity.Property(s => s.m_type)
                  .HasConversion<string>();
                });

            modelBuilder.Entity<InverterHoursAvgConsumption>(b =>
            {
                b.HasIndex(e => new { e.InverterId, e.RegisteredInverterId, e.DateCalculated, e.DayOfWeek, e.TimeHour }).IsUnique();
            });

            modelBuilder.Entity<InverterCompanyHours>(b =>
            {
                b.HasIndex(e => new { e.InverterId, e.SpotPriceId, e.CompanyId, e.ActionType }).IsUnique();
            });

            modelBuilder.
            Entity<DeviceHoursNoCalculation>
            (entity => {
                entity.Property(s => s.Time)
                .HasColumnType("time");
            });


            modelBuilder.Entity<DeviceHoursNoCalculation>(b =>
            {
                b.HasIndex(e => new { e.DeviceId, e.Time }).IsUnique();
            });

            modelBuilder.
                Entity<SpotPrice>
                (entity => {
                    entity.Property(s => s.Time)
                    .HasColumnType("time");
                });

            modelBuilder.Entity<CompanyHourlyFeesTransactions>(b =>
            {
                b.HasIndex(e => new { e.CompanyId, e.Date, e.Time }).IsUnique();
            });

            modelBuilder.Entity<Region>(b =>
            {
                b.HasIndex(e => new {e.CountryId, e.Abbreviation }).IsUnique();
            });

            modelBuilder.Entity<SpotPrice>(b =>
            {
                b.HasIndex(e => new { e.DateTime, e.RegionId }).IsUnique();
            });

            modelBuilder.Entity<RegisteredInverter>()
                 .HasIndex(e => e.Name)
                 .IsUnique();

            modelBuilder.Entity<SpotPrice>()
        .HasData(
         new SpotPrice {Id = 1, RegionId = 1, DateTime = DateTime.Parse("2023-05-13 23:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.7000, PriceWithTax = 0.7000 },
         new SpotPrice {Id = 2, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 00:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 3, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 01:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 4, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 02:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 5, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 03:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 6, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 04:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 7, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 05:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.2251, PriceWithTax = 1.2251 },
         new SpotPrice {Id = 8, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 06:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 9, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 07:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.9226, PriceWithTax = 0.9226 },
         new SpotPrice {Id = 10, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 08:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.9413, PriceWithTax = 0.9413 },
         new SpotPrice {Id = 11, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 09:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 12, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 10:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.2274, PriceWithTax = 0.2274 },
         new SpotPrice {Id = 13, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 11:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.2099, PriceWithTax = 0.2099 },
         new SpotPrice {Id = 14, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 12:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.1494, PriceWithTax = 0.1494 },
         new SpotPrice {Id = 15, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 13:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.1162, PriceWithTax = 0.1162 },
         new SpotPrice {Id = 16, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 14:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.0895, PriceWithTax = 0.0895 },
         new SpotPrice {Id = 17, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 15:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.1192, PriceWithTax = 0.1192 },
         new SpotPrice {Id = 18, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 16:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.1426, PriceWithTax = 0.1426 },
         new SpotPrice {Id = 19, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 17:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.1739, PriceWithTax = 0.1739 },
         new SpotPrice {Id = 20, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 18:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 21, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 19:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 22, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 20:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.6149, PriceWithTax = 1.6149 },
         new SpotPrice {Id = 23, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 21:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.2407, PriceWithTax = 1.2407 },
         new SpotPrice {Id = 24, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 22:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 25, RegionId = 1, DateTime = DateTime.Parse("2023-05-14 23:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.4000, PriceWithTax = 1.4000 },
         new SpotPrice {Id = 26, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 00:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.7559, PriceWithTax = 0.7559 },
         new SpotPrice {Id = 27, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 01:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.4934, PriceWithTax = 0.4934 },
         new SpotPrice {Id = 28, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 02:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.4953, PriceWithTax = 0.4953 },
         new SpotPrice {Id = 29, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 03:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.4968, PriceWithTax = 0.4968 },
         new SpotPrice {Id = 30, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 04:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.6432, PriceWithTax = 0.6432 },
         new SpotPrice {Id = 31, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 05:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.6915, PriceWithTax = 0.6915 },
         new SpotPrice {Id = 32, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 06:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.1664, PriceWithTax = 1.1664 },
         new SpotPrice {Id = 33, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 07:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.1891, PriceWithTax = 1.1891 },
         new SpotPrice {Id = 34, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 08:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.1954, PriceWithTax = 1.1954 },
         new SpotPrice {Id = 35, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 09:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.0332, PriceWithTax = 1.0332 },
         new SpotPrice {Id = 36, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 10:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8109, PriceWithTax = 0.8109 },
         new SpotPrice {Id = 37, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 11:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.6710, PriceWithTax = 0.6710 },
         new SpotPrice {Id = 38, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 12:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.9881, PriceWithTax = 0.9881 },
         new SpotPrice {Id = 39, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 13:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8558, PriceWithTax = 0.8558 },
         new SpotPrice {Id = 40, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 14:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.2227, PriceWithTax = 0.2227 },
         new SpotPrice {Id = 41, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 15:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.1229, PriceWithTax = 0.1229 },
         new SpotPrice {Id = 42, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 16:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 43, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 17:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 44, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 18:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8559, PriceWithTax = 0.8559 },
         new SpotPrice {Id = 45, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 19:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.1465, PriceWithTax = 1.1465 },
         new SpotPrice {Id = 46, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 20:00:00").AddHours(-3), Rank = 5, PriceNoTax = 1.6528, PriceWithTax = 1.6528 },
         new SpotPrice {Id = 47, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 21:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8560, PriceWithTax = 0.8560 },
         new SpotPrice {Id = 48, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 22:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.8570, PriceWithTax = 0.8570 },
         new SpotPrice {Id = 49, RegionId = 1, DateTime = DateTime.Parse("2023-05-15 23:00:00").AddHours(-3), Rank = 5, PriceNoTax = 0.5004, PriceWithTax = 0.5004 }
         );



            modelBuilder.Entity<DeviceHoursNoCalculation>()
            .HasData(
             new DeviceHoursNoCalculation { Id = 1, DeviceId = 3, Time = TimeSpan.Parse("11:00:00") },
             new DeviceHoursNoCalculation { Id = 2, DeviceId = 3, Time = TimeSpan.Parse("12:00:00") },
             new DeviceHoursNoCalculation { Id = 3, DeviceId = 3, Time = TimeSpan.Parse("13:00:00") });

            modelBuilder.Entity<InverterType>()
            .HasData(
             new InverterType { Id = 1, Name = "HYD 5-20KTL-3PH", LossInWatts = 300 });

            modelBuilder.Entity<InverterTypeActions>()
            .HasData(
             new InverterTypeActions { Id = 1, InverterTypeId = 1, ActionType = ActionType.Charge, ActionTypeCommand = ActionTypeCommand.ChargeMax, ActionName = "Charge Max", IsClickable = true},
             new InverterTypeActions { Id = 2, InverterTypeId = 1, ActionType = ActionType.Charge, ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun, ActionName = "Charge with Remaining sun", IsClickable = true },
             new InverterTypeActions { Id = 4, InverterTypeId = 1, ActionType = ActionType.Discharge, ActionTypeCommand = ActionTypeCommand.SelfUse, ActionName = "Self use", IsClickable = true },
             new InverterTypeActions { Id = 5, InverterTypeId = 1, ActionType = ActionType.Discharge, ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower, ActionName = "Consume battery with max power", IsClickable = true},
             new InverterTypeActions { Id = 6, InverterTypeId = 1, ActionType = ActionType.Automode, ActionTypeCommand = ActionTypeCommand.AutoMode, ActionName = "Automatic control", IsClickable = true},
             new InverterTypeActions { Id = 7, InverterTypeId = 1, ActionType = ActionType.Charge, ActionTypeCommand = ActionTypeCommand.SellRemainingSunNoCharging, ActionName = "Sell remaining sun (no charging)", IsClickable = true},
             new InverterTypeActions { Id = 8, InverterTypeId = 1, ActionType = ActionType.ModeControl, ActionTypeCommand = ActionTypeCommand.InverterSelfUse, ActionName = "Inverter Self use", IsClickable = true},
             new InverterTypeActions { Id = 9, InverterTypeId = 1, ActionType = ActionType.ExternalControlHzMarket, ActionTypeCommand = ActionTypeCommand.HzMarket, ActionName = "Hz market control", IsClickable = false});


            modelBuilder.Entity<RegisteredInverter>()
                .HasData(
                 new RegisteredInverter { Id = 1, Name = "SofarMQTTJaamari20kw" });

            modelBuilder.Entity<Inverter>()
            .HasData(
             new Inverter { Id = 1, CompanyId = 1,InverterTypeId = 1, MaxPower = 20, MaxSalesPowerCapacity = 15, RegisteredInverterId = 1, CalculationFormula = CalculationFormula.Winter });



            modelBuilder.Entity<InverterTypeCompanyActions>()
            .HasData(
             new InverterTypeCompanyActions { Id = 1, InverterId = 1, InverterTypeActionsId = 1, CompanyId = 1, InverterTypeId = 1, ActionType = ActionType.Charge, ActionTypeCommand = ActionTypeCommand.ChargeMax, ActionName = "Charge Max" },
             new InverterTypeCompanyActions { Id = 2, InverterId = 1, InverterTypeActionsId = 2, CompanyId = 1, InverterTypeId = 1, ActionType = ActionType.Charge, ActionTypeCommand = ActionTypeCommand.ChargeWithRemainingSun, ActionName = "Charge with Remaining sun" },
             new InverterTypeCompanyActions { Id = 4, InverterId = 1, InverterTypeActionsId = 4, CompanyId = 1, InverterTypeId = 1, ActionType = ActionType.Discharge, ActionTypeCommand = ActionTypeCommand.SelfUse, ActionName = "Self use" },
             new InverterTypeCompanyActions { Id = 5, InverterId = 1, InverterTypeActionsId = 5, CompanyId = 1, InverterTypeId = 1, ActionType = ActionType.Discharge, ActionTypeCommand = ActionTypeCommand.ConsumeBatteryWithMaxPower, ActionName = "Consume battery with max power" },
             new InverterTypeCompanyActions { Id = 11, InverterId = 1, InverterTypeActionsId = 6, CompanyId = 1, InverterTypeId = 1, ActionType = ActionType.Automode, ActionTypeCommand = ActionTypeCommand.AutoMode, ActionName = "Automatic control" });

            modelBuilder.Entity<InverterBattery>()
            .HasData(
             new InverterBattery
             {
                 Id = 1,
                 InverterId = 1,
                 CapacityKWh = 10d,
                 ChargingPowerFromGridKWh = 6.5d,
                 MinLevel = 20,
                 MaxLevel = 80,
                 ConsiderBadWeather75PercentFactor = false,
                 LoadBatteryTo95PercentEnabled = true,
                 LoadBatteryTo95PercentPrice = 1
             });

            modelBuilder.Entity<InverterTypeCommands>()
            .HasData(
             new InverterTypeCommands { Id = 1, InverterTypeId = 1, ActionType = ActionType.ThreePhaseAntiRefluxOn, MqttTopic = "/set/threephaselimit", IsPayloadFixed = true },
             new InverterTypeCommands { Id = 2, InverterTypeId = 1, ActionType = ActionType.ThreePhaseAntiRefluxOff, MqttTopic = "/set/threephaselimit",IsPayloadFixed = true },
             new InverterTypeCommands { Id = 3, InverterTypeId = 1, ActionType = ActionType.Charge, MqttTopic = "/set/charge", IsPayloadFixed = false },
             new InverterTypeCommands { Id = 4, InverterTypeId = 1, ActionType = ActionType.Discharge, MqttTopic = "/set/discharge", IsPayloadFixed = false },
             new InverterTypeCommands { Id = 5, InverterTypeId = 1, ActionType = ActionType.ModeControl, MqttTopic = "/set/mode_control", IsPayloadFixed = false }
             );

            modelBuilder.Entity<InverterCompanyCommandsPayload>()
            .HasData(
             new InverterCompanyCommandsPayload { Id = 1, InverterTypeCommandsId = 1, InverterId = 1, Payload = "150" },
             new InverterCompanyCommandsPayload { Id = 2, InverterTypeCommandsId = 2, InverterId = 1, Payload = "0" });

            modelBuilder.Entity<CompanyHourlyFees>()
            .HasData(
             new CompanyHourlyFees { Id = 1, CompanyId = 1, Time = new TimeSpan(0, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 2, CompanyId = 1, Time = new TimeSpan(1, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 3, CompanyId = 1, Time = new TimeSpan(2, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 4, CompanyId = 1, Time = new TimeSpan(3, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 5, CompanyId = 1, Time = new TimeSpan(4, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 6, CompanyId = 1, Time = new TimeSpan(5, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 7, CompanyId = 1, Time = new TimeSpan(6, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 8, CompanyId = 1, Time = new TimeSpan(7, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 9, CompanyId = 1, Time = new TimeSpan(8, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 10, CompanyId = 1, Time = new TimeSpan(9, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 11, CompanyId = 1, Time = new TimeSpan(10, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 12, CompanyId = 1, Time = new TimeSpan(11, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 13, CompanyId = 1, Time = new TimeSpan(12, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 14, CompanyId = 1, Time = new TimeSpan(13, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 15, CompanyId = 1, Time = new TimeSpan(14, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 16, CompanyId = 1, Time = new TimeSpan(15, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 17, CompanyId = 1, Time = new TimeSpan(16, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 18, CompanyId = 1, Time = new TimeSpan(17, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 19, CompanyId = 1, Time = new TimeSpan(18, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 20, CompanyId = 1, Time = new TimeSpan(19, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 21, CompanyId = 1, Time = new TimeSpan(20, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 23, CompanyId = 1, Time = new TimeSpan(21, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0868 },
             new CompanyHourlyFees { Id = 24, CompanyId = 1, Time = new TimeSpan(22, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 },
             new CompanyHourlyFees { Id = 25, CompanyId = 1, Time = new TimeSpan(23, 0, 0), BrokerServiceFee = 0.072, NetworkServiceFee = 0.0505 });

            modelBuilder.Entity<InverterTypeListenTopics>()
            .HasData(
             new InverterTypeListenTopics { Id = 1, InverterTypeId = 1, TopicName = "/state" });

            modelBuilder.Entity<SolarPanelCapacity>()
            .HasData(
             new SolarPanelCapacity { Id = 1, Time = new TimeSpan(7, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.05 },
             new SolarPanelCapacity { Id = 2, Time = new TimeSpan(8, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.1 },
             new SolarPanelCapacity { Id = 3, Time = new TimeSpan(9, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.3 },
             new SolarPanelCapacity { Id = 4, Time = new TimeSpan(10, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.55 },
             new SolarPanelCapacity { Id = 5, Time = new TimeSpan(11, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.8 },
             new SolarPanelCapacity { Id = 6, Time = new TimeSpan(12, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 1 },
             new SolarPanelCapacity { Id = 7, Time = new TimeSpan(13, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 1.1 },
             new SolarPanelCapacity { Id = 8, Time = new TimeSpan(14, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 1.1 },
             new SolarPanelCapacity { Id = 9, Time = new TimeSpan(15, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 1 },
             new SolarPanelCapacity { Id = 10, Time = new TimeSpan(16, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.8 },
             new SolarPanelCapacity { Id = 11, Time = new TimeSpan(17, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.55 },
             new SolarPanelCapacity { Id = 12, Time = new TimeSpan(18, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.3 },
             new SolarPanelCapacity { Id = 13, Time = new TimeSpan(19, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.1 },
             new SolarPanelCapacity { Id = 14, Time = new TimeSpan(20, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.S, MaxPercentage = 0.05 },
             new SolarPanelCapacity { Id = 15, Time = new TimeSpan(7, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.1 },
             new SolarPanelCapacity { Id = 16, Time = new TimeSpan(8, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.2 },
             new SolarPanelCapacity { Id = 17, Time = new TimeSpan(9, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.5 },
             new SolarPanelCapacity { Id = 18, Time = new TimeSpan(10, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.7 },
             new SolarPanelCapacity { Id = 19, Time = new TimeSpan(11, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 1 },
             new SolarPanelCapacity { Id = 20, Time = new TimeSpan(12, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 1.1 },
             new SolarPanelCapacity { Id = 21, Time = new TimeSpan(13, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 1.1 },
             new SolarPanelCapacity { Id = 22, Time = new TimeSpan(14, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 1 },
             new SolarPanelCapacity { Id = 23, Time = new TimeSpan(15, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.85 },
             new SolarPanelCapacity { Id = 24, Time = new TimeSpan(16, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.7},
             new SolarPanelCapacity { Id = 25, Time = new TimeSpan(17, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.5 },
             new SolarPanelCapacity { Id = 26, Time = new TimeSpan(18, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.3 },
             new SolarPanelCapacity { Id = 27, Time = new TimeSpan(19, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.1 },
             new SolarPanelCapacity { Id = 28, Time = new TimeSpan(20, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SE, MaxPercentage = 0.05 },
             new SolarPanelCapacity { Id = 29, Time = new TimeSpan(7, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.05 },
             new SolarPanelCapacity { Id = 30, Time = new TimeSpan(8, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.1 },
             new SolarPanelCapacity { Id = 31, Time = new TimeSpan(9, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.3 },
             new SolarPanelCapacity { Id = 32, Time = new TimeSpan(10, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.5 },
             new SolarPanelCapacity { Id = 33, Time = new TimeSpan(11, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.7 },
             new SolarPanelCapacity { Id = 34, Time = new TimeSpan(12, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.85 },
             new SolarPanelCapacity { Id = 35, Time = new TimeSpan(13, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 1 },
             new SolarPanelCapacity { Id = 36, Time = new TimeSpan(14, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 1.1 },
             new SolarPanelCapacity { Id = 37, Time = new TimeSpan(15, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 1.1 },
             new SolarPanelCapacity { Id = 38, Time = new TimeSpan(16, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 1 },
             new SolarPanelCapacity { Id = 39, Time = new TimeSpan(17, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.7 },
             new SolarPanelCapacity { Id = 40, Time = new TimeSpan(18, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.5 },
             new SolarPanelCapacity { Id = 41, Time = new TimeSpan(19, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.2 },
             new SolarPanelCapacity { Id = 42, Time = new TimeSpan(20, 0, 0), PanelTilt = 45, SolarPanelsDirecation = SolarPanelsDirecation.SW, MaxPercentage = 0.1 });

            modelBuilder.Entity<CountrySolarCapacity>()
            .HasData(
             new CountrySolarCapacity { Id = 1, CountryId = 1, Month = 1, SolarCapacity = 0.4 },
             new CountrySolarCapacity { Id = 2, CountryId = 1, Month = 2, SolarCapacity = 0.5 },
             new CountrySolarCapacity { Id = 3, CountryId = 1, Month = 3, SolarCapacity = 0.6 },
             new CountrySolarCapacity { Id = 4, CountryId = 1, Month = 4, SolarCapacity = 0.75 },
             new CountrySolarCapacity { Id = 5, CountryId = 1, Month = 5, SolarCapacity = 1 },
             new CountrySolarCapacity { Id = 6, CountryId = 1, Month = 6, SolarCapacity = 1 },
             new CountrySolarCapacity { Id = 7, CountryId = 1, Month = 7, SolarCapacity = 1 },
             new CountrySolarCapacity { Id = 8, CountryId = 1, Month = 8, SolarCapacity = 1 },
             new CountrySolarCapacity { Id = 9, CountryId = 1, Month = 9, SolarCapacity = 0.75 },
             new CountrySolarCapacity { Id = 10, CountryId = 1, Month = 10, SolarCapacity = 0.6 },
             new CountrySolarCapacity { Id = 11, CountryId = 1, Month = 11, SolarCapacity = 0.5 },
             new CountrySolarCapacity { Id = 12, CountryId = 1, Month = 12, SolarCapacity = 0.4 });


            modelBuilder.Entity<CompanyFixedPrice>()
                .HasData(
             new CompanyFixedPrice { Id = 1, CompanyId = 1, Time = new TimeSpan(0, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 2, CompanyId = 1, Time = new TimeSpan(1, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 3, CompanyId = 1, Time = new TimeSpan(2, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 4, CompanyId = 1, Time = new TimeSpan(3, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 5, CompanyId = 1, Time = new TimeSpan(4, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 6, CompanyId = 1, Time = new TimeSpan(5, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 7, CompanyId = 1, Time = new TimeSpan(6, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 8, CompanyId = 1, Time = new TimeSpan(7, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 9, CompanyId = 1, Time = new TimeSpan(8, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 10, CompanyId = 1, Time = new TimeSpan(9, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 11, CompanyId = 1, Time = new TimeSpan(10, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 12, CompanyId = 1, Time = new TimeSpan(11, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 13, CompanyId = 1, Time = new TimeSpan(12, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 14, CompanyId = 1, Time = new TimeSpan(13, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 15, CompanyId = 1, Time = new TimeSpan(14, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 16, CompanyId = 1, Time = new TimeSpan(15, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 17, CompanyId = 1, Time = new TimeSpan(16, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 18, CompanyId = 1, Time = new TimeSpan(17, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 19, CompanyId = 1, Time = new TimeSpan(18, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 20, CompanyId = 1, Time = new TimeSpan(19, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 21, CompanyId = 1, Time = new TimeSpan(20, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 22, CompanyId = 1, Time = new TimeSpan(21, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 23, CompanyId = 1, Time = new TimeSpan(22, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 },
             new CompanyFixedPrice { Id = 24, CompanyId = 1, Time = new TimeSpan(23, 0, 0), PurchasePrice = 0.12, SalesPrice = 0.16 });

            modelBuilder.Entity<CountryVatRange>()
                .HasData(
                new CountryVatRange { Id = 1, CountryId = 1, StartDate = DateOnly.FromDateTime(new DateTime(2024,1,1)), EndDate = DateOnly.FromDateTime(new DateTime(2024, 12, 31)), VatRate = 1},
                new CountryVatRange { Id = 2, CountryId = 1, StartDate = DateOnly.FromDateTime(new DateTime(2025, 1, 1)), EndDate = DateOnly.FromDateTime(new DateTime(2025, 6, 30)), VatRate = 1.22},
                new CountryVatRange { Id = 3, CountryId = 1, StartDate = DateOnly.FromDateTime(new DateTime(2025, 1, 7)), EndDate = DateOnly.FromDateTime(new DateTime(2035, 12, 31)), VatRate = 1.24 },
                new CountryVatRange { Id = 4, CountryId = 3, StartDate = DateOnly.FromDateTime(new DateTime(2024, 1, 1)), EndDate = DateOnly.FromDateTime(new DateTime(2024, 8, 31)), VatRate = 1.24 },
                new CountryVatRange { Id = 5, CountryId = 3, StartDate = DateOnly.FromDateTime(new DateTime(2024, 9, 1)), EndDate = DateOnly.FromDateTime(new DateTime(2035, 12, 31)), VatRate = 1.255},
                new CountryVatRange { Id = 6, CountryId = 2, StartDate = DateOnly.FromDateTime(new DateTime(2024, 1, 1)), EndDate = DateOnly.FromDateTime(new DateTime(2035, 12, 31)), VatRate = 1.25},
                new CountryVatRange { Id = 7, CountryId = 8, StartDate = DateOnly.FromDateTime(new DateTime(2024, 1, 1)), EndDate = DateOnly.FromDateTime(new DateTime(2035, 12, 31)), VatRate = 21 });


            modelBuilder.Entity<SofarState>().UseTpcMappingStrategy();
            modelBuilder.Entity<SofarStateHourly>().UseTpcMappingStrategy();


        }
    }
}
