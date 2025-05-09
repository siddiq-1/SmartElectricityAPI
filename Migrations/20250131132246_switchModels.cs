using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class switchModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SwitchModelId",
                table: "Switch",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SwitchModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwitchModel", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SwitchModelParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SwitchModelId = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SwitchModelParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SwitchModelParameters_SwitchModel_SwitchModelId",
                        column: x => x.SwitchModelId,
                        principalTable: "SwitchModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                column: "SwitchModelId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 2,
                column: "SwitchModelId",
                value: 1);

            migrationBuilder.InsertData(
                table: "SwitchModel",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Shelly Plus 1 ver 000", null },
                    { 2, null, "Shelly Plug S ver 000", null }
                });

            migrationBuilder.InsertData(
                table: "SwitchModelParameters",
                columns: new[] { "Id", "CreatedAt", "DeviceActionType", "Payload", "SwitchModelId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "On", "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": true\r\n  }\r\n}", 1, null },
                    { 2, null, "Off", "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": true\r\n  }\r\n}", 1, null },
                    { 3, null, "None", "{\r\n  \"id\":0,\r\n  \"src\": \"HTTP_in\",\r\n  \"method\": \"Switch.Set\",\r\n  \"params\": {\r\n    \"id\": 0,\r\n    \"on\": false\r\n  }\r\n}", 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Switch_SwitchModelId",
                table: "Switch",
                column: "SwitchModelId");

            migrationBuilder.CreateIndex(
                name: "IX_SwitchModel_Name",
                table: "SwitchModel",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SwitchModelParameters_SwitchModelId",
                table: "SwitchModelParameters",
                column: "SwitchModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Switch_SwitchModel_SwitchModelId",
                table: "Switch",
                column: "SwitchModelId",
                principalTable: "SwitchModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Switch_SwitchModel_SwitchModelId",
                table: "Switch");

            migrationBuilder.DropTable(
                name: "SwitchModelParameters");

            migrationBuilder.DropTable(
                name: "SwitchModel");

            migrationBuilder.DropIndex(
                name: "IX_Switch_SwitchModelId",
                table: "Switch");

            migrationBuilder.DropColumn(
                name: "SwitchModelId",
                table: "Switch");
        }
    }
}
