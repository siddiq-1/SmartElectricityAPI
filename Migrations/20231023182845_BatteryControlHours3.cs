using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class BatteryControlHours3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatteryControlHours_SpotPrice_SpotPriceMinId",
                table: "BatteryControlHours");

            migrationBuilder.DropIndex(
                name: "IX_BatteryControlHours_SpotPriceMinId",
                table: "BatteryControlHours");

            migrationBuilder.DropColumn(
                name: "SpotPriceMinId",
                table: "BatteryControlHours");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpotPriceMinId",
                table: "BatteryControlHours",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BatteryControlHours_SpotPriceMinId",
                table: "BatteryControlHours",
                column: "SpotPriceMinId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatteryControlHours_SpotPrice_SpotPriceMinId",
                table: "BatteryControlHours",
                column: "SpotPriceMinId",
                principalTable: "SpotPrice",
                principalColumn: "Id");
        }
    }
}
