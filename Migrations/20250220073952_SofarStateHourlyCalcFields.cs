using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class SofarStateHourlyCalcFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CostOfConsumpWithOutMygGid",
                table: "SofarStateHourly",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CostPurchaseMinusSellFromGrid",
                table: "SofarStateHourly",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WinOrLoseFromMyGridUsage",
                table: "SofarStateHourly",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostOfConsumpWithOutMygGid",
                table: "SofarStateHourly");

            migrationBuilder.DropColumn(
                name: "CostPurchaseMinusSellFromGrid",
                table: "SofarStateHourly");

            migrationBuilder.DropColumn(
                name: "WinOrLoseFromMyGridUsage",
                table: "SofarStateHourly");
        }
    }
}
