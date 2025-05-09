using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class removedUnusedBatteryControlHoursSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatteryControlHoursSchedule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatteryControlHoursSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterBatteryId = table.Column<int>(type: "int", nullable: false),
                    SpotPriceId = table.Column<int>(type: "int", nullable: false),
                    AvgHourlyConsumption = table.Column<int>(type: "int", nullable: true),
                    ChargingPowerWh = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    SalesActionTypeCommand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SalesMaxMinPriceDifference = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SalesMaxPriceWithCost = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SalesMinPriceWithCost = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SelfUseActionTypeCommand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SelfUseMaxMinPriceDifference = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SelfUseMaxPriceWithCost = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    SelfUseMinPriceWithCost = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    TimeHour = table.Column<TimeSpan>(type: "time(0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatteryControlHoursSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatteryControlHoursSchedule_InverterBattery_InverterBatteryId",
                        column: x => x.InverterBatteryId,
                        principalTable: "InverterBattery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatteryControlHoursSchedule_SpotPrice_SpotPriceId",
                        column: x => x.SpotPriceId,
                        principalTable: "SpotPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryControlHoursSchedule_InverterBatteryId",
                table: "BatteryControlHoursSchedule",
                column: "InverterBatteryId");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryControlHoursSchedule_SpotPriceId",
                table: "BatteryControlHoursSchedule",
                column: "SpotPriceId");
        }
    }
}
