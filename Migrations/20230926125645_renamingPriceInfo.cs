using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class renamingPriceInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellToNetworkPriceLimit",
                table: "Inverter",
                newName: "BrokerPurchaseMargin");

            migrationBuilder.RenameColumn(
                name: "UseWeekendFeeOnSaturdayAndSunday",
                table: "Company",
                newName: "UseNightTimeFreeOnSaturdayAndSunday");

            migrationBuilder.RenameColumn(
                name: "NetworkServiceFeeWeekend",
                table: "Company",
                newName: "NetworkServiceFeeNightTime");

            migrationBuilder.RenameColumn(
                name: "BrokerServiceFeeWeekend",
                table: "Company",
                newName: "NetworkServiceFeeDayTime");

            migrationBuilder.AddColumn<double>(
                name: "BrokerSalesMargin",
                table: "Company",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BrokerSalesMargin", "NetworkServiceFeeDayTime" },
                values: new object[] { 0.071999999999999995, 0.086800000000000002 });

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BrokerSalesMargin", "NetworkServiceFeeDayTime" },
                values: new object[] { 0.082000000000000003, 0.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerSalesMargin",
                table: "Company");

            migrationBuilder.RenameColumn(
                name: "BrokerPurchaseMargin",
                table: "Inverter",
                newName: "SellToNetworkPriceLimit");

            migrationBuilder.RenameColumn(
                name: "UseNightTimeFreeOnSaturdayAndSunday",
                table: "Company",
                newName: "UseWeekendFeeOnSaturdayAndSunday");

            migrationBuilder.RenameColumn(
                name: "NetworkServiceFeeNightTime",
                table: "Company",
                newName: "NetworkServiceFeeWeekend");

            migrationBuilder.RenameColumn(
                name: "NetworkServiceFeeDayTime",
                table: "Company",
                newName: "BrokerServiceFeeWeekend");

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                column: "BrokerServiceFeeWeekend",
                value: 0.071999999999999995);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                column: "BrokerServiceFeeWeekend",
                value: 0.082000000000000003);
        }
    }
}
