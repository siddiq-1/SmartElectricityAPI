using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyPricesUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ExpectedProfitForSelfUseOnlyInCents",
                table: "Company",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ExpectedProfitFromChargeAndSellPriceInCents",
                table: "Company",
                type: "double",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ExpectedProfitForSelfUseOnlyInCents", "ExpectedProfitFromChargeAndSellPriceInCents" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ExpectedProfitForSelfUseOnlyInCents", "ExpectedProfitFromChargeAndSellPriceInCents" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedProfitForSelfUseOnlyInCents",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "ExpectedProfitFromChargeAndSellPriceInCents",
                table: "Company");
        }
    }
}
