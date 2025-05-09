using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyFixedAvgHourly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixedAvgHourlyWatts",
                table: "Inverter",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseFixedAvgHourlyWatts",
                table: "Inverter",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FixedAvgHourlyWatts", "UseFixedAvgHourlyWatts" },
                values: new object[] { 0, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedAvgHourlyWatts",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "UseFixedAvgHourlyWatts",
                table: "Inverter");
        }
    }
}
