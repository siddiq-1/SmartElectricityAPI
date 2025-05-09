using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class CountryVatRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryVatRange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    VatRate = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryVatRange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryVatRange_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "CountryVatRange",
                columns: new[] { "Id", "CountryId", "CreatedAt", "EndDate", "StartDate", "UpdatedAt", "VatRate" },
                values: new object[,]
                {
                    { 1, 1, null, new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1), null, 1.0 },
                    { 2, 1, null, new DateOnly(2025, 6, 30), new DateOnly(2025, 1, 1), null, 1.22 },
                    { 3, 1, null, new DateOnly(2035, 12, 31), new DateOnly(2025, 1, 7), null, 1.24 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryVatRange_CountryId",
                table: "CountryVatRange",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryVatRange");
        }
    }
}
