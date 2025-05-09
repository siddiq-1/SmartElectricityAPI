using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class switchDataAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeviceActionType", "Payload", "Topic" },
                values: new object[] { "On", "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": true\r\n  }\r\n}", "shellyplus1-virgo1/rpc" });

            migrationBuilder.InsertData(
                table: "Switch",
                columns: new[] { "Id", "ActionWaitTimeInSeconds", "CompanyId", "CreatedAt", "DeviceActionType", "MqttDeviceId", "Payload", "SensorGroupId", "SensorId", "Topic", "UpdatedAt" },
                values: new object[] { 2, 0, 1, null, "Off", null, "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": false\r\n  }\r\n}", 1, 1, "shellyplus1-virgo1/rpc", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeviceActionType", "Payload", "Topic" },
                values: new object[] { "None", null, "mqttnet/samples/topic/4" });
        }
    }
}
