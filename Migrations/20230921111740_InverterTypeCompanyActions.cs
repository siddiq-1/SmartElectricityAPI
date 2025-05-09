using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterTypeCompanyActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InverterTypeCompanyActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InverterTypeActionsId = table.Column<int>(type: "int", nullable: false),
                    InverterTypeId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ActionName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterTypeCompanyActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterTypeCompanyActions_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterTypeCompanyActions_InverterTypeActions_InverterTypeA~",
                        column: x => x.InverterTypeActionsId,
                        principalTable: "InverterTypeActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterTypeCompanyActions_InverterType_InverterTypeId",
                        column: x => x.InverterTypeId,
                        principalTable: "InverterType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InverterTypeCompanyActions",
                columns: new[] { "Id", "ActionName", "ActionType", "CompanyId", "CreatedAt", "InverterTypeActionsId", "InverterTypeId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Charge Max", 7, 1, null, 1, 1, null },
                    { 2, "Charge with Remaining sun", 7, 1, null, 2, 1, null },
                    { 3, "Keep battery level", 7, 1, null, 3, 1, null },
                    { 4, "Compensate missing energy", 8, 1, null, 4, 1, null },
                    { 5, "Consume battery with max power", 8, 1, null, 5, 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypeCompanyActions_CompanyId",
                table: "InverterTypeCompanyActions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypeCompanyActions_InverterTypeActionsId",
                table: "InverterTypeCompanyActions",
                column: "InverterTypeActionsId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypeCompanyActions_InverterTypeId",
                table: "InverterTypeCompanyActions",
                column: "InverterTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterTypeCompanyActions");
        }
    }
}
