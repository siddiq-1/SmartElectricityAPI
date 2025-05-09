using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InvertTypeCommandsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InverterTypeCommands",
                columns: new[] { "Id", "ActionType", "CreatedAt", "InverterTypeId", "MqttTopic", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ThreePhaseAntiRefluxOn", null, 1, "/set/threephaselimit", null },
                    { 2, "ThreePhaseAntiRefluxOff", null, 1, "/set/threephaselimit", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
