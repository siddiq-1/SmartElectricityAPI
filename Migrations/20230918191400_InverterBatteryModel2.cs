using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterBatteryModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CapacityWh",
                table: "InverterBattery",
                newName: "LoadBatteryTo95PercentPrice");

            migrationBuilder.AddColumn<double>(
                name: "CapacityKWh",
                table: "InverterBattery",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ExpectedProfitProducedAndSoldDifference",
                table: "InverterBattery",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LoadBatteryTo95PercentEnabled",
                table: "InverterBattery",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CapacityKWh", "ExpectedProfitProducedAndSoldDifference", "LoadBatteryTo95PercentEnabled", "LoadBatteryTo95PercentPrice" },
                values: new object[] { 6.5, 10, true, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacityKWh",
                table: "InverterBattery");

            migrationBuilder.DropColumn(
                name: "ExpectedProfitProducedAndSoldDifference",
                table: "InverterBattery");

            migrationBuilder.DropColumn(
                name: "LoadBatteryTo95PercentEnabled",
                table: "InverterBattery");

            migrationBuilder.RenameColumn(
                name: "LoadBatteryTo95PercentPrice",
                table: "InverterBattery",
                newName: "CapacityWh");

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                column: "CapacityWh",
                value: 6000);
        }
    }
}
