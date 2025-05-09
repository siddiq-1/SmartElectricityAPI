using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class inverterTypeActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InverterTypeActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterTypeId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterTypeActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterTypeActions_InverterType_InverterTypeId",
                        column: x => x.InverterTypeId,
                        principalTable: "InverterType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InverterTypeActions",
                columns: new[] { "Id", "ActionName", "ActionType", "CreatedAt", "InverterTypeId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Charge Max", "Charge", null, 1, null },
                    { 2, "Charge with Remaining sun", "Charge", null, 1, null },
                    { 3, "Keep battery level", "Charge", null, 1, null },
                    { 4, "Compensate missing energy", "Discharge", null, 1, null },
                    { 5, "Consume battery with max power", "Discharge", null, 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypeActions_InverterTypeId",
                table: "InverterTypeActions",
                column: "InverterTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterTypeActions");
        }
    }
}
