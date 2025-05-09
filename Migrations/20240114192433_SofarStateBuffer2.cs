using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class SofarStateBuffer2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SofarStateLatest");

            migrationBuilder.CreateTable(
                name: "SofarStateBuffer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MqttMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofarStateBuffer", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SofarStateBuffer");

            migrationBuilder.CreateTable(
                name: "SofarStateLatest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    RegisteredInverterId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    TimeHour = table.Column<TimeSpan>(type: "time(0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    batterySOC = table.Column<int>(type: "int", nullable: false),
                    battery_current = table.Column<double>(type: "double", nullable: false),
                    battery_cycles = table.Column<double>(type: "double", nullable: false),
                    battery_power = table.Column<double>(type: "double", nullable: false),
                    battery_temp = table.Column<double>(type: "double", nullable: false),
                    battery_voltage = table.Column<double>(type: "double", nullable: false),
                    consumption = table.Column<int>(type: "int", nullable: false),
                    deviceName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    grid_power = table.Column<int>(type: "int", nullable: false),
                    inverter_HStemp = table.Column<double>(type: "double", nullable: false),
                    inverter_power = table.Column<int>(type: "int", nullable: false),
                    inverter_temp = table.Column<double>(type: "double", nullable: false),
                    running_state = table.Column<int>(type: "int", nullable: false),
                    solarPV = table.Column<int>(type: "int", nullable: false),
                    solarPV1 = table.Column<int>(type: "int", nullable: false),
                    solarPV1Current = table.Column<double>(type: "double", nullable: false),
                    solarPV2 = table.Column<int>(type: "int", nullable: false),
                    solarPV2Current = table.Column<double>(type: "double", nullable: false),
                    today_charged = table.Column<double>(type: "double", nullable: false),
                    today_consumption = table.Column<double>(type: "double", nullable: false),
                    today_discharged = table.Column<double>(type: "double", nullable: false),
                    today_exported = table.Column<double>(type: "double", nullable: false),
                    today_generation = table.Column<double>(type: "double", nullable: false),
                    today_purchase = table.Column<double>(type: "double", nullable: false),
                    total_charged = table.Column<double>(type: "double", nullable: false),
                    total_consumption = table.Column<double>(type: "double", nullable: false),
                    total_discharged = table.Column<double>(type: "double", nullable: false),
                    total_exported = table.Column<double>(type: "double", nullable: false),
                    total_generation = table.Column<double>(type: "double", nullable: false),
                    total_purchase = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofarStateLatest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SofarStateLatest_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SofarStateLatest_RegisteredInverter_RegisteredInverterId",
                        column: x => x.RegisteredInverterId,
                        principalTable: "RegisteredInverter",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateLatest_CompanyId",
                table: "SofarStateLatest",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateLatest_RegisteredInverterId",
                table: "SofarStateLatest",
                column: "RegisteredInverterId");
        }
    }
}
