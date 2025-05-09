using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class fuseboxPriceCheckBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HzMarketDischargeMinPrice", "HzMarketMinBatteryLevelOnDischargeCommand" },
                values: new object[] { 0.0, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "HzMarketDischargeMinPrice", "HzMarketMinBatteryLevelOnDischargeCommand" },
                values: new object[] { null, null });
        }
    }
}
