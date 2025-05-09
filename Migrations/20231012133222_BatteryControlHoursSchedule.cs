using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class BatteryControlHoursSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatteryControlHoursSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SpotPriceId = table.Column<int>(type: "int", nullable: false),
                    InverterBatteryId = table.Column<int>(type: "int", nullable: false),
                    SalesMinPriceWithCost = table.Column<double>(type: "double", nullable: true),
                    SalesMaxPriceWithCost = table.Column<double>(type: "double", nullable: true),
                    SalesMaxMinPriceDifference = table.Column<double>(type: "double", nullable: true),
                    SelfUseMinPriceWithCost = table.Column<double>(type: "double", nullable: true),
                    SelfUseMaxPriceWithCost = table.Column<double>(type: "double", nullable: true),
                    SelfUseMaxMinPriceDifference = table.Column<double>(type: "double", nullable: true),
                    AvgHourlyConsumption = table.Column<int>(type: "int", nullable: true),
                    SalesActionTypeCommand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SelfUseActionTypeCommand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatteryControlHoursSchedule");
        }
    }
}
