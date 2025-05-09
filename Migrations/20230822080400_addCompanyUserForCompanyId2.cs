using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class addCompanyUserForCompanyId2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CompanyUsers",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "PermissionId", "UpdatedAt", "UserId" },
                values: new object[] { 4, 2, null, 1, null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyUsers",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
