using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class batteryFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AdditionalTimeForBatteryChargingPercentage",
                table: "InverterBattery",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BatteryPowerFactor", "BatteryVoltageFactor", "ConsumptionFactor", "TodayBatteryChargedFactor", "TodayBatteryDischargedFactor", "TodayGenerationFactor" },
                values: new object[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 });

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                column: "AdditionalTimeForBatteryChargingPercentage",
                value: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalTimeForBatteryChargingPercentage",
                table: "InverterBattery");

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BatteryPowerFactor", "BatteryVoltageFactor", "ConsumptionFactor", "TodayBatteryChargedFactor", "TodayBatteryDischargedFactor", "TodayGenerationFactor" },
                values: new object[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
        }
    }
}
