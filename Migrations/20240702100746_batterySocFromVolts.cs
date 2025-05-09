using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class batterySocFromVolts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatteryVoltsMax",
                table: "InverterBattery",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BatteryVoltsMin",
                table: "InverterBattery",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CalculateBatterSocFromVolts",
                table: "InverterBattery",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BatteryVoltsMax", "BatteryVoltsMin", "CalculateBatterSocFromVolts" },
                values: new object[] { 0, 0, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatteryVoltsMax",
                table: "InverterBattery");

            migrationBuilder.DropColumn(
                name: "BatteryVoltsMin",
                table: "InverterBattery");

            migrationBuilder.DropColumn(
                name: "CalculateBatterSocFromVolts",
                table: "InverterBattery");
        }
    }
}
