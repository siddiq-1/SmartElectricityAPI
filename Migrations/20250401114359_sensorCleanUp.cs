using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sensorCleanUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MqttDeviceId",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "Payload",
                table: "Sensor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MqttDeviceId",
                table: "Sensor",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "Sensor",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MqttDeviceId", "Payload" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MqttDeviceId", "Payload" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MqttDeviceId", "Payload" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "MqttDeviceId", "Payload" },
                values: new object[] { null, null });
        }
    }
}
