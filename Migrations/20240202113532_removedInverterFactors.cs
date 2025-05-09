using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class removedInverterFactors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatteryPowerFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "BatteryVoltageFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "ConsumptionFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "GridPowerFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "InverterTempFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "TodayBatteryChargedFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "TodayBatteryDischargedFactor",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "TodayGenerationFactor",
                table: "Inverter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BatteryPowerFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BatteryVoltageFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ConsumptionFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GridPowerFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InverterTempFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TodayBatteryChargedFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TodayBatteryDischargedFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TodayGenerationFactor",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BatteryPowerFactor", "BatteryVoltageFactor", "ConsumptionFactor", "GridPowerFactor", "InverterTempFactor", "TodayBatteryChargedFactor", "TodayBatteryDischargedFactor", "TodayGenerationFactor" },
                values: new object[] { 1.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0, 1.0 });
        }
    }
}
