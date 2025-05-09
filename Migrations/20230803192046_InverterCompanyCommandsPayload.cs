using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterCompanyCommandsPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MqttTopic",
                table: "InverterTypeCommands",
                type: "varchar(384)",
                maxLength: 384,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InverterCompanyCommandsPayload",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterTypeCommandsId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InverterId = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterCompanyCommandsPayload", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterCompanyCommandsPayload_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterCompanyCommandsPayload_InverterTypeCommands_Inverter~",
                        column: x => x.InverterTypeCommandsId,
                        principalTable: "InverterTypeCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterCompanyCommandsPayload_Inverter_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InverterCompanyCommandsPayload",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "InverterId", "InverterTypeCommandsId", "Payload", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, null, 1, 1, "150", null },
                    { 2, 1, null, 1, 2, "0", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyCommandsPayload_CompanyId",
                table: "InverterCompanyCommandsPayload",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyCommandsPayload_InverterId",
                table: "InverterCompanyCommandsPayload",
                column: "InverterId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyCommandsPayload_InverterTypeCommandsId",
                table: "InverterCompanyCommandsPayload",
                column: "InverterTypeCommandsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterCompanyCommandsPayload");

            migrationBuilder.AlterColumn<string>(
                name: "MqttTopic",
                table: "InverterTypeCommands",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(384)",
                oldMaxLength: 384)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
