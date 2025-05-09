using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateHourlyNewField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoOfGroupedTransactions",
                table: "SofarStateHourlyTemp",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NoOfGroupedTransactions",
                table: "SofarStateHourly",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoOfGroupedTransactions",
                table: "SofarStateHourlyTemp");

            migrationBuilder.DropColumn(
                name: "NoOfGroupedTransactions",
                table: "SofarStateHourly");
        }
    }
}
