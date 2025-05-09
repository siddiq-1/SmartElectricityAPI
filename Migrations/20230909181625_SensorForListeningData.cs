using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class SensorForListeningData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InverterMark",
                table: "Inverter");

            migrationBuilder.InsertData(
                table: "Sensor",
                columns: new[] { "Id", "ActionType", "CompanyId", "CreatedAt", "Description", "DeviceId", "MqttDeviceId", "Name", "Payload", "Topic", "UpdatedAt" },
                values: new object[] { 4, "None", 1, null, "Õhk-Vesi Põrand voolutarve", 1, null, "Voolutarve", null, "shellies/shellyem3-485519DC6688/emeter/0/energy", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AddColumn<string>(
                name: "InverterMark",
                table: "Inverter",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "InverterMark",
                value: "HYD 5-20KTL-3PH");
        }
    }
}
