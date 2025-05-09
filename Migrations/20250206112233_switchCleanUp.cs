using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class switchCleanUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MqttDeviceId",
                table: "Switch");

            migrationBuilder.DropColumn(
                name: "Payload",
                table: "Switch");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Switch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MqttDeviceId",
                table: "Switch",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "Switch",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Switch",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MqttDeviceId", "Payload", "Topic" },
                values: new object[] { null, null, "shellyplus1-virgo1/rpc" });

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MqttDeviceId", "Payload", "Topic" },
                values: new object[] { null, null, "shellyplus1-virgo1/rpc" });
        }
    }
}
