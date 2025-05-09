using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NetworkFeeDayTime = table.Column<double>(type: "double", nullable: false),
                    NetworkFeeNightTime = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Level = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(784)", maxLength: 784, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Level = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpotPrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DateTime = table.Column<DateTime>(type: "datetime(0)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    PriceNoTax = table.Column<double>(type: "double", nullable: false),
                    PriceWithTax = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotPrice", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FuseboxForcedOff = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FuseboxForcedOn = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxStopHoursIn24h = table.Column<double>(type: "double", nullable: false),
                    MaxStopHoursConsecutive = table.Column<double>(type: "double", nullable: false),
                    MaxForcedOnHoursIn24h = table.Column<double>(type: "double", nullable: false),
                    ForcedOnPercentageForComingHourToEnable = table.Column<double>(type: "double", nullable: false),
                    ForcedOn = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ForcedOff = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MediumOn = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TemperatureInStandardMode = table.Column<double>(type: "double", nullable: true),
                    TemperatureInForcedOnMode = table.Column<double>(type: "double", nullable: true),
                    FirstHourPercentageKwPriceRequirementBeforeHeating = table.Column<double>(type: "double", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inverter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SellToNetworkPriceLimit = table.Column<double>(type: "double", nullable: false),
                    MaxSalesPowerCapacity = table.Column<double>(type: "double", nullable: false),
                    MaxPower = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inverter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inverter_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SwitchGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwitchGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwitchGroup_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceCompanyHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpotPriceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCompanyHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceCompanyHours_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceCompanyHours_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceCompanyHours_SpotPrice_SpotPriceId",
                        column: x => x.SpotPriceId,
                        principalTable: "SpotPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeviceHoursNoCalculation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceHoursNoCalculation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceHoursNoCalculation_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sensor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Topic = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MqttDeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensor_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sensor_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InverterCompanyHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpotPriceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterCompanyHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterCompanyHours_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterCompanyHours_Inverter_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterCompanyHours_SpotPrice_SpotPriceId",
                        column: x => x.SpotPriceId,
                        principalTable: "SpotPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InverterSensor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Topic = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MqttDeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterSensor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterSensor_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterSensor_Inverter_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompanyUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyUsers_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Switch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SensorId = table.Column<int>(type: "int", nullable: false),
                    SensorGroupId = table.Column<int>(type: "int", nullable: false),
                    ActionWaitTimeInSeconds = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MqttDeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Switch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Switch_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Switch_Sensor_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Switch_SwitchGroup_SensorGroupId",
                        column: x => x.SensorGroupId,
                        principalTable: "SwitchGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InverterSwitch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InverterSensorId = table.Column<int>(type: "int", nullable: false),
                    ActionWaitTimeInSeconds = table.Column<int>(type: "int", nullable: false),
                    Topic = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MqttDeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterSwitch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterSwitch_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterSwitch_InverterSensor_InverterSensorId",
                        column: x => x.InverterSensorId,
                        principalTable: "InverterSensor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "Id", "Address", "CreatedAt", "Name", "NetworkFeeDayTime", "NetworkFeeNightTime", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Posti", null, "Jäämari", 9.0, 5.0, null },
                    { 2, "Kiriku tee", null, "TM ERP Solutions OÜ", 8.0, 4.0, null }
                });

            migrationBuilder.InsertData(
                table: "Permission",
                columns: new[] { "Id", "CreatedAt", "Level", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Administrator", null },
                    { 2, null, "User", null }
                });

            migrationBuilder.InsertData(
                table: "SpotPrice",
                columns: new[] { "Id", "CreatedAt", "Date", "DateTime", "PriceNoTax", "PriceWithTax", "Rank", "Time", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 13, 23, 0, 0, 0, DateTimeKind.Unspecified), 0.69999999999999996, 0.69999999999999996, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 2, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 3, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 1, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 4, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 2, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 5, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 3, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 6, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 4, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 7, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 5, 0, 0, 0, DateTimeKind.Unspecified), 1.2251000000000001, 1.2251000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 8, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 6, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 9, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 7, 0, 0, 0, DateTimeKind.Unspecified), 0.92259999999999998, 0.92259999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 10, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 8, 0, 0, 0, DateTimeKind.Unspecified), 0.94130000000000003, 0.94130000000000003, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 11, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 9, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 12, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 10, 0, 0, 0, DateTimeKind.Unspecified), 0.22739999999999999, 0.22739999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 13, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 11, 0, 0, 0, DateTimeKind.Unspecified), 0.2099, 0.2099, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 14, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 12, 0, 0, 0, DateTimeKind.Unspecified), 0.14940000000000001, 0.14940000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 15, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 13, 0, 0, 0, DateTimeKind.Unspecified), 0.1162, 0.1162, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 16, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 14, 0, 0, 0, DateTimeKind.Unspecified), 0.089499999999999996, 0.089499999999999996, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 17, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 15, 0, 0, 0, DateTimeKind.Unspecified), 0.1192, 0.1192, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 18, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 16, 0, 0, 0, DateTimeKind.Unspecified), 0.1426, 0.1426, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 19, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 17, 0, 0, 0, DateTimeKind.Unspecified), 0.1739, 0.1739, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 20, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 18, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 21, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 19, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 22, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 20, 0, 0, 0, DateTimeKind.Unspecified), 1.6149, 1.6149, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 23, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 21, 0, 0, 0, DateTimeKind.Unspecified), 1.2406999999999999, 1.2406999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 24, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 22, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 25, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 14, 23, 0, 0, 0, DateTimeKind.Unspecified), 1.3999999999999999, 1.3999999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 26, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 0.75590000000000002, 0.75590000000000002, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 27, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 1, 0, 0, 0, DateTimeKind.Unspecified), 0.49340000000000001, 0.49340000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 28, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 2, 0, 0, 0, DateTimeKind.Unspecified), 0.49530000000000002, 0.49530000000000002, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 29, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 3, 0, 0, 0, DateTimeKind.Unspecified), 0.49680000000000002, 0.49680000000000002, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 30, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 4, 0, 0, 0, DateTimeKind.Unspecified), 0.64319999999999999, 0.64319999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 31, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 5, 0, 0, 0, DateTimeKind.Unspecified), 0.6915, 0.6915, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 32, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 6, 0, 0, 0, DateTimeKind.Unspecified), 1.1664000000000001, 1.1664000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 33, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 7, 0, 0, 0, DateTimeKind.Unspecified), 1.1891, 1.1891, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 34, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 8, 0, 0, 0, DateTimeKind.Unspecified), 1.1954, 1.1954, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 35, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), 1.0331999999999999, 1.0331999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 36, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), 0.81089999999999995, 0.81089999999999995, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 37, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 11, 0, 0, 0, DateTimeKind.Unspecified), 0.67100000000000004, 0.67100000000000004, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 38, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 12, 0, 0, 0, DateTimeKind.Unspecified), 0.98809999999999998, 0.98809999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 39, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 13, 0, 0, 0, DateTimeKind.Unspecified), 0.85580000000000001, 0.85580000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 40, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 14, 0, 0, 0, DateTimeKind.Unspecified), 0.22270000000000001, 0.22270000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 41, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 15, 0, 0, 0, DateTimeKind.Unspecified), 0.1229, 0.1229, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 42, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 16, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 43, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 17, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 44, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 18, 0, 0, 0, DateTimeKind.Unspecified), 0.85589999999999999, 0.85589999999999999, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 45, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 19, 0, 0, 0, DateTimeKind.Unspecified), 1.1465000000000001, 1.1465000000000001, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 46, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 20, 0, 0, 0, DateTimeKind.Unspecified), 1.6528, 1.6528, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 47, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 21, 0, 0, 0, DateTimeKind.Unspecified), 0.85599999999999998, 0.85599999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 48, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 22, 0, 0, 0, DateTimeKind.Unspecified), 0.85699999999999998, 0.85699999999999998, 5, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 49, null, new DateOnly(1, 1, 1), new DateTime(2023, 5, 15, 23, 0, 0, 0, DateTimeKind.Unspecified), 0.50039999999999996, 0.50039999999999996, 5, new TimeSpan(0, 0, 0, 0, 0), null }
                });

            migrationBuilder.InsertData(
                table: "Device",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "FirstHourPercentageKwPriceRequirementBeforeHeating", "ForcedOff", "ForcedOn", "ForcedOnPercentageForComingHourToEnable", "FuseboxForcedOff", "FuseboxForcedOn", "MaxForcedOnHoursIn24h", "MaxStopHoursConsecutive", "MaxStopHoursIn24h", "MediumOn", "Name", "TemperatureInForcedOnMode", "TemperatureInStandardMode", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, null, 10.0, true, true, 50.0, true, true, 10.0, 8.0, 8.0, false, "Õhk-Vesi Põrand", null, null, null },
                    { 2, 1, null, 20.0, true, true, 50.0, true, true, 10.0, 2.0, 6.0, false, "Õhk-Vesi Boiler", null, null, null },
                    { 3, 1, null, 43.0, true, true, 100.0, true, true, 10.0, 3.0, 6.0, false, "Tava boiler", 65.0, 55.0, null },
                    { 4, 1, null, 30.0, false, true, 200.0, false, true, 2.0, 0.0, 0.0, false, "Külmik", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Inverter",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "MaxPower", "MaxSalesPowerCapacity", "Name", "SellToNetworkPriceLimit", "UpdatedAt" },
                values: new object[] { 1, 1, null, 20.0, 15.0, "Sofar", 0.080000000000000002, null });

            migrationBuilder.InsertData(
                table: "SwitchGroup",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { 1, 1, null, "Forces ON", null });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "CreatedAt", "Email", "Password", "PermissionId", "RefreshToken", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, null, "risto.roots@gmail.com", "$2a$12$TW4G6r9o4NgK1X.YQakuauvUKEeeOKWgKSzRvH.JtmBwPtHjC7xMq", 1, null, null, "Risto" },
                    { 2, null, "virgo.tuul@gmail.com", "$2a$12$ZCj.JC9gyEjorrDypRllcu.822dPZAx/KVrkkAift.oF4EiYhj6YW", 1, null, null, "Virgo" }
                });

            migrationBuilder.InsertData(
                table: "CompanyUsers",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, 1, null, null, 1 },
                    { 2, 1, null, null, 2 },
                    { 3, 2, null, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "DeviceHoursNoCalculation",
                columns: new[] { "Id", "CreatedAt", "DeviceId", "Time", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, 3, new TimeSpan(0, 11, 0, 0, 0), null },
                    { 2, null, 3, new TimeSpan(0, 12, 0, 0, 0), null },
                    { 3, null, 3, new TimeSpan(0, 13, 0, 0, 0), null }
                });

            migrationBuilder.InsertData(
                table: "InverterSensor",
                columns: new[] { "Id", "ActionType", "CompanyId", "CreatedAt", "Description", "InverterId", "MqttDeviceId", "Name", "Payload", "Topic", "UpdatedAt" },
                values: new object[] { 1, "None", 1, null, "Jäämari sofar inverter", 1, "devId01", "Sofar Virgo", "", "SofarMQTT/response/threephaselimit", null });

            migrationBuilder.InsertData(
                table: "Sensor",
                columns: new[] { "Id", "ActionType", "CompanyId", "CreatedAt", "Description", "DeviceId", "MqttDeviceId", "Name", "Payload", "Topic", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Off", 1, null, "Klemmid 9-10", 2, null, "Normal ON", null, "mqttnet/samples/topic/2", null },
                    { 2, "Off", 1, null, "Klemmid 5-6", 2, null, "Forced ON", null, "mqttnet/samples/topic/3", null },
                    { 3, "Off", 1, null, "Relee Õhk-Vesi 2 seadmes", 2, null, "Boiler", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "InverterSwitch",
                columns: new[] { "Id", "ActionType", "ActionWaitTimeInSeconds", "CompanyId", "CreatedAt", "InverterSensorId", "MqttDeviceId", "Payload", "Topic", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ThreePhaseAntiRefluxOn", 10, 1, null, 1, "devId1", "150", "SofarMQTT/set/threephaselimit", null },
                    { 2, "ThreePhaseAntiRefluxOff", 10, 1, null, 1, "devId2", "0", "SofarMQTT/set/threephaselimit", null }
                });

            migrationBuilder.InsertData(
                table: "Switch",
                columns: new[] { "Id", "ActionType", "ActionWaitTimeInSeconds", "CompanyId", "CreatedAt", "MqttDeviceId", "Payload", "SensorGroupId", "SensorId", "Topic", "UpdatedAt" },
                values: new object[] { 1, "Off", 0, 1, null, null, null, 1, 1, "mqttnet/samples/topic/4", null });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_CompanyId",
                table: "CompanyUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyUsers_UserId",
                table: "CompanyUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_CompanyId",
                table: "Device",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCompanyHours_CompanyId",
                table: "DeviceCompanyHours",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCompanyHours_DeviceId_SpotPriceId_CompanyId_ActionType",
                table: "DeviceCompanyHours",
                columns: new[] { "DeviceId", "SpotPriceId", "CompanyId", "ActionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCompanyHours_SpotPriceId",
                table: "DeviceCompanyHours",
                column: "SpotPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceHoursNoCalculation_DeviceId_Time",
                table: "DeviceHoursNoCalculation",
                columns: new[] { "DeviceId", "Time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inverter_CompanyId",
                table: "Inverter",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyHours_CompanyId",
                table: "InverterCompanyHours",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyHours_InverterId_SpotPriceId_CompanyId_Action~",
                table: "InverterCompanyHours",
                columns: new[] { "InverterId", "SpotPriceId", "CompanyId", "ActionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyHours_SpotPriceId",
                table: "InverterCompanyHours",
                column: "SpotPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSensor_CompanyId",
                table: "InverterSensor",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSensor_InverterId",
                table: "InverterSensor",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSwitch_CompanyId",
                table: "InverterSwitch",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSwitch_InverterSensorId",
                table: "InverterSwitch",
                column: "InverterSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_CompanyId",
                table: "Sensor",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_DeviceId",
                table: "Sensor",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotPrice_DateTime",
                table: "SpotPrice",
                column: "DateTime",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Switch_CompanyId",
                table: "Switch",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Switch_SensorGroupId",
                table: "Switch",
                column: "SensorGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Switch_SensorId",
                table: "Switch",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchGroup_CompanyId",
                table: "SwitchGroup",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PermissionId",
                table: "User",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyUsers");

            migrationBuilder.DropTable(
                name: "DeviceCompanyHours");

            migrationBuilder.DropTable(
                name: "DeviceHoursNoCalculation");

            migrationBuilder.DropTable(
                name: "InverterCompanyHours");

            migrationBuilder.DropTable(
                name: "InverterSwitch");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Switch");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "SpotPrice");

            migrationBuilder.DropTable(
                name: "InverterSensor");

            migrationBuilder.DropTable(
                name: "Sensor");

            migrationBuilder.DropTable(
                name: "SwitchGroup");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Inverter");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Company");
        }
    }
}
