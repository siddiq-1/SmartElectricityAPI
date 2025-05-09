using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyPricesUpdates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ExpectedProfitForSelfUseOnlyInCents", "ExpectedProfitFromChargeAndSellPriceInCents" },
                values: new object[] { 5.0, 10.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ExpectedProfitForSelfUseOnlyInCents", "ExpectedProfitFromChargeAndSellPriceInCents" },
                values: new object[] { null, null });
        }
    }
}
