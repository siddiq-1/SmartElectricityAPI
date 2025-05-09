using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class removedBatteryAutomatedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutomatedBatteryEfficientUsage",
                table: "InverterBattery");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAutomatedBatteryEfficientUsage",
                table: "InverterBattery",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InverterBattery",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsAutomatedBatteryEfficientUsage",
                value: true);
        }
    }
}
