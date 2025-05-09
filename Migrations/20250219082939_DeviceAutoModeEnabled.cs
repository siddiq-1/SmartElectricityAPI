using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeviceAutoModeEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoModeEnabled",
                table: "Device",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Device",
                keyColumn: "Id",
                keyValue: 1,
                column: "AutoModeEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Device",
                keyColumn: "Id",
                keyValue: 2,
                column: "AutoModeEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Device",
                keyColumn: "Id",
                keyValue: 3,
                column: "AutoModeEnabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Device",
                keyColumn: "Id",
                keyValue: 4,
                column: "AutoModeEnabled",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoModeEnabled",
                table: "Device");
        }
    }
}
