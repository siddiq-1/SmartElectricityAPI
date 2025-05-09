using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class moveBrokerPurchaseMargin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerPurchaseMargin",
                table: "Inverter");

            migrationBuilder.AddColumn<double>(
                name: "BrokerPurchaseMargin",
                table: "Company",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                column: "BrokerPurchaseMargin",
                value: 0.080000000000000002);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                column: "BrokerPurchaseMargin",
                value: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokerPurchaseMargin",
                table: "Company");

            migrationBuilder.AddColumn<double>(
                name: "BrokerPurchaseMargin",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "BrokerPurchaseMargin",
                value: 0.080000000000000002);
        }
    }
}
