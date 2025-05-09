using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class BatteryControlHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatteryControlHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterBatteryId = table.Column<int>(type: "int", nullable: false),
                    SpotPriceMinId = table.Column<int>(type: "int", nullable: true),
                    SpotPriceMaxId = table.Column<int>(type: "int", nullable: false),
                    MinPriceHour = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    MinPriceWithCost = table.Column<double>(type: "double", nullable: true),
                    MaxPriceHour = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    MaxPriceWithCost = table.Column<double>(type: "double", nullable: true),
                    MaxMinPriceDifference = table.Column<double>(type: "double", nullable: true),
                    MinChargingPowerWhOriginal = table.Column<int>(type: "int", nullable: true),
                    MinChargingPowerWh = table.Column<int>(type: "int", nullable: true),
                    MinChargingPowerWhOriginalDiff = table.Column<int>(type: "int", nullable: true),
                    MaxAvgHourlyConsumptionOriginal = table.Column<int>(type: "int", nullable: true),
                    MaxAvgHourlyConsumption = table.Column<int>(type: "int", nullable: true),
                    WaveNumber = table.Column<int>(type: "int", nullable: true),
                    UsableWatts = table.Column<int>(type: "int", nullable: true),
                    UsableWattsProfit = table.Column<double>(type: "double", nullable: true),
                    AmountCharged = table.Column<int>(type: "int", nullable: true),
                    LineProfit = table.Column<double>(type: "double", nullable: true),
                    HourProfit = table.Column<double>(type: "double", nullable: true),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    GroupNumber = table.Column<int>(type: "int", nullable: false),
                    MaxAvgHourlyConsumptionOriginalDiff = table.Column<int>(type: "int", nullable: true),
                    ActionTypeCommand = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryControlHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatteryControlHours_InverterBattery_InverterBatteryId",
                        column: x => x.InverterBatteryId,
                        principalTable: "InverterBattery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatteryControlHours_SpotPrice_SpotPriceMaxId",
                        column: x => x.SpotPriceMaxId,
                        principalTable: "SpotPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatteryControlHours_SpotPrice_SpotPriceMinId",
                        column: x => x.SpotPriceMinId,
                        principalTable: "SpotPrice",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryControlHours_InverterBatteryId",
                table: "BatteryControlHours",
                column: "InverterBatteryId");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryControlHours_SpotPriceMaxId",
                table: "BatteryControlHours",
                column: "SpotPriceMaxId");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryControlHours_SpotPriceMinId",
                table: "BatteryControlHours",
                column: "SpotPriceMinId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatteryControlHours");
        }
    }
}
