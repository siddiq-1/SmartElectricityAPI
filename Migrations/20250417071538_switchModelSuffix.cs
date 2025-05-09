using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class switchModelSuffix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MqttSuffix",
                table: "SwitchModel",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "SwitchModel",
                keyColumn: "Id",
                keyValue: 1,
                column: "MqttSuffix",
                value: null);

            migrationBuilder.UpdateData(
                table: "SwitchModel",
                keyColumn: "Id",
                keyValue: 2,
                column: "MqttSuffix",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MqttSuffix",
                table: "SwitchModel");
        }
    }
}
