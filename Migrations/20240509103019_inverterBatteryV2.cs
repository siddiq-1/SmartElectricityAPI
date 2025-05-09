using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class inverterBatteryV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChargingPowerKWh",
                table: "InverterBattery",
                newName: "ChargingPowerFromSolarKWh");

            migrationBuilder.AddColumn<double>(
                name: "ChargingPowerFromGridKWh",
                table: "InverterBattery",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ChargingPowerFromGridKWh", "ChargingPowerFromSolarKWh" },
                values: new object[] { 6.5, 0.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargingPowerFromGridKWh",
                table: "InverterBattery");

            migrationBuilder.RenameColumn(
                name: "ChargingPowerFromSolarKWh",
                table: "InverterBattery",
                newName: "ChargingPowerKWh");

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                column: "ChargingPowerKWh",
                value: 6.5);
        }
    }
}
