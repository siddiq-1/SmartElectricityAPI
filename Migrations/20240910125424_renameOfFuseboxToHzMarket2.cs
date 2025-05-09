using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class renameOfFuseboxToHzMarket2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 9,
                column: "ActionType",
                value: "ExternalControlHzMarket");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 9,
                column: "ActionType",
                value: "ExternalControlFuseBox");
        }
    }
}
