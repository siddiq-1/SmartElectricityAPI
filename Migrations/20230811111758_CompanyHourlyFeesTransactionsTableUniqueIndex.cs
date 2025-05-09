using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class CompanyHourlyFeesTransactionsTableUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyHourlyFeesTransactions_CompanyId",
                table: "CompanyHourlyFeesTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyHourlyFeesTransactions_CompanyId_Date_Time",
                table: "CompanyHourlyFeesTransactions",
                columns: new[] { "CompanyId", "Date", "Time" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyHourlyFeesTransactions_CompanyId_Date_Time",
                table: "CompanyHourlyFeesTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyHourlyFeesTransactions_CompanyId",
                table: "CompanyHourlyFeesTransactions",
                column: "CompanyId");
        }
    }
}
