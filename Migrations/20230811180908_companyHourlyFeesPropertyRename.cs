using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyHourlyFeesPropertyRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NetworkServiceFree",
                table: "CompanyHourlyFees",
                newName: "NetworkServiceFee");

            migrationBuilder.RenameColumn(
                name: "BrokerServiceFree",
                table: "CompanyHourlyFees",
                newName: "BrokerServiceFee");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyHourlyFeesTransactions_Company_CompanyId",
                table: "CompanyHourlyFeesTransactions",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyHourlyFeesTransactions_Company_CompanyId",
                table: "CompanyHourlyFeesTransactions");

            migrationBuilder.RenameColumn(
                name: "NetworkServiceFee",
                table: "CompanyHourlyFees",
                newName: "NetworkServiceFree");

            migrationBuilder.RenameColumn(
                name: "BrokerServiceFee",
                table: "CompanyHourlyFees",
                newName: "BrokerServiceFree");
        }
    }
}
