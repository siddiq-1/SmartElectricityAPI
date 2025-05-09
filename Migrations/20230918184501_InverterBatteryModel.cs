using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterBatteryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPayloadFixed",
                table: "InverterTypeCommands",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InverterBattery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterId = table.Column<int>(type: "int", nullable: false),
                    CapacityWh = table.Column<int>(type: "int", nullable: false),
                    MinLevel = table.Column<int>(type: "int", nullable: false),
                    MaxLevel = table.Column<int>(type: "int", nullable: false),
                    ConsiderBadWeather75PercentFactor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsAutomatedBatteryEfficientUsage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterBattery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterBattery_Inverter_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InverterBattery",
                columns: new[] { "Id", "CapacityWh", "ConsiderBadWeather75PercentFactor", "CreatedAt", "InverterId", "IsAutomatedBatteryEfficientUsage", "MaxLevel", "MinLevel", "UpdatedAt" },
                values: new object[] { 1, 6000, false, null, 1, true, 80, 20, null });

            migrationBuilder.UpdateData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsPayloadFixed",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsPayloadFixed",
                value: true);

            migrationBuilder.InsertData(
                table: "InverterTypeCommands",
                columns: new[] { "Id", "ActionType", "CreatedAt", "InverterTypeId", "IsPayloadFixed", "MqttTopic", "UpdatedAt" },
                values: new object[,]
                {
                    { 3, "Charge", null, 1, false, "/set/charge", null },
                    { 4, "Discharge", null, 1, false, "/set/discharge", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterBattery_InverterId",
                table: "InverterBattery",
                column: "InverterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterBattery");

            migrationBuilder.DeleteData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InverterTypeCommands",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "IsPayloadFixed",
                table: "InverterTypeCommands");
        }
    }
}
