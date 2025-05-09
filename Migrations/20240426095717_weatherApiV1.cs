using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class weatherApiV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SolarPanelsDirecation",
                table: "Inverter",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "SolarPanelsMaxPower",
                table: "Inverter",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "UseWeatherForecast",
                table: "Inverter",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SolarPanelsDirecation", "SolarPanelsMaxPower", "UseWeatherForecast" },
                values: new object[] { "S", 0.0, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolarPanelsDirecation",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "SolarPanelsMaxPower",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "UseWeatherForecast",
                table: "Inverter");
        }
    }
}
