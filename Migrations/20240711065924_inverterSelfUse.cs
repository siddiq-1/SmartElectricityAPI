using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class inverterSelfUse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseInverterSelfUse",
                table: "Inverter",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "UseInverterSelfUse",
                value: false);

            migrationBuilder.InsertData(
                table: "InverterTypeCommands",
                columns: new[] { "Id", "ActionType", "CreatedAt", "InverterTypeId", "IsPayloadFixed", "MqttTopic", "UpdatedAt" },
                values: new object[] { 5, "ModeControl", null, 1, false, "/set/mode_control", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "UseInverterSelfUse",
                table: "Inverter");
        }
    }
}
