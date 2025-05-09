using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class removingInverterSensorAndInverterSwitch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterSwitch");

            migrationBuilder.DropTable(
                name: "InverterSensor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InverterSensor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InverterId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MqttDeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Topic = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterSensor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterSensor_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterSensor_Inverter_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InverterSwitch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InverterSensorId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionWaitTimeInSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    MqttDeviceId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Payload = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Topic = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterSwitch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterSwitch_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterSwitch_InverterSensor_InverterSensorId",
                        column: x => x.InverterSensorId,
                        principalTable: "InverterSensor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InverterSensor",
                columns: new[] { "Id", "ActionType", "CompanyId", "CreatedAt", "Description", "InverterId", "MqttDeviceId", "Name", "Payload", "Topic", "UpdatedAt" },
                values: new object[] { 1, "None", 1, null, "Jäämari sofar inverter", 1, "devId01", "Sofar Virgo", "", "SofarMQTTJaamari20kw/response/threephaselimit", null });

            migrationBuilder.InsertData(
                table: "InverterSwitch",
                columns: new[] { "Id", "ActionType", "ActionWaitTimeInSeconds", "CompanyId", "CreatedAt", "InverterSensorId", "MqttDeviceId", "Payload", "Topic", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "ThreePhaseAntiRefluxOn", 10, 1, null, 1, "devId1", "150", "SofarMQTTJaamari20kw/set/threephaselimit", null },
                    { 2, "ThreePhaseAntiRefluxOff", 10, 1, null, 1, "devId2", "0", "SofarMQTTJaamari20kw/set/threephaselimit", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterSensor_CompanyId",
                table: "InverterSensor",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSensor_InverterId",
                table: "InverterSensor",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSwitch_CompanyId",
                table: "InverterSwitch",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterSwitch_InverterSensorId",
                table: "InverterSwitch",
                column: "InverterSensorId");
        }
    }
}
