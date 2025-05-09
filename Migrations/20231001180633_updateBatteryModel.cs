using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateBatteryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ChargingPowerKWh",
                table: "InverterBattery",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                column: "BrokerPurchaseMargin",
                value: 0.0080000000000000002);

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CapacityKWh", "ChargingPowerKWh" },
                values: new object[] { 10.0, 6.5 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargingPowerKWh",
                table: "InverterBattery");

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                column: "BrokerPurchaseMargin",
                value: 0.080000000000000002);

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                column: "CapacityKWh",
                value: 6.5);
        }
    }
}
