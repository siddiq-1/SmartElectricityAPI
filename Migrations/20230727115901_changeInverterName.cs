using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class changeInverterName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "SofarMQTTJaamari20kw");

            migrationBuilder.UpdateData(
                table: "InverterSensor",
                keyColumn: "Id",
                keyValue: 1,
                column: "Topic",
                value: "SofarMQTTJaamari20kw/response/threephaselimit");

            migrationBuilder.UpdateData(
                table: "InverterSwitch",
                keyColumn: "Id",
                keyValue: 1,
                column: "Topic",
                value: "SofarMQTTJaamari20kw/set/threephaselimit");

            migrationBuilder.UpdateData(
                table: "InverterSwitch",
                keyColumn: "Id",
                keyValue: 2,
                column: "Topic",
                value: "SofarMQTTJaamari20kw/set/threephaselimit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Sofar");

            migrationBuilder.UpdateData(
                table: "InverterSensor",
                keyColumn: "Id",
                keyValue: 1,
                column: "Topic",
                value: "SofarMQTT/response/threephaselimit");

            migrationBuilder.UpdateData(
                table: "InverterSwitch",
                keyColumn: "Id",
                keyValue: 1,
                column: "Topic",
                value: "SofarMQTT/set/threephaselimit");

            migrationBuilder.UpdateData(
                table: "InverterSwitch",
                keyColumn: "Id",
                keyValue: 2,
                column: "Topic",
                value: "SofarMQTT/set/threephaselimit");
        }
    }
}
