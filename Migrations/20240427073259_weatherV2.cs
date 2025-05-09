using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class weatherV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountrySolarCapacity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    SolarCapacity = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountrySolarCapacity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountrySolarCapacity_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SolarPanelCapacity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Time = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    PanelTilt = table.Column<int>(type: "int", nullable: false),
                    SolarPanelsDirecation = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxPercentage = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarPanelCapacity", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "CountrySolarCapacity",
                columns: new[] { "Id", "CountryId", "CreatedAt", "Month", "SolarCapacity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, null, 1, 0.40000000000000002, null },
                    { 2, 1, null, 2, 0.5, null },
                    { 3, 1, null, 3, 0.59999999999999998, null },
                    { 4, 1, null, 4, 0.75, null },
                    { 5, 1, null, 5, 1.0, null },
                    { 6, 1, null, 6, 1.0, null },
                    { 7, 1, null, 7, 1.0, null },
                    { 8, 1, null, 8, 1.0, null },
                    { 9, 1, null, 9, 0.75, null },
                    { 10, 1, null, 10, 0.59999999999999998, null },
                    { 11, 1, null, 11, 0.5, null },
                    { 12, 1, null, 12, 0.40000000000000002, null }
                });

            migrationBuilder.InsertData(
                table: "SolarPanelCapacity",
                columns: new[] { "Id", "CreatedAt", "MaxPercentage", "PanelTilt", "SolarPanelsDirecation", "Time", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, 0.050000000000000003, 45, "S", new TimeOnly(7, 0, 0), null },
                    { 2, null, 0.10000000000000001, 45, "S", new TimeOnly(8, 0, 0), null },
                    { 3, null, 0.29999999999999999, 45, "S", new TimeOnly(9, 0, 0), null },
                    { 4, null, 0.55000000000000004, 45, "S", new TimeOnly(10, 0, 0), null },
                    { 5, null, 0.80000000000000004, 45, "S", new TimeOnly(11, 0, 0), null },
                    { 6, null, 1.0, 45, "S", new TimeOnly(12, 0, 0), null },
                    { 7, null, 1.1000000000000001, 45, "S", new TimeOnly(13, 0, 0), null },
                    { 8, null, 1.1000000000000001, 45, "S", new TimeOnly(14, 0, 0), null },
                    { 9, null, 1.0, 45, "S", new TimeOnly(15, 0, 0), null },
                    { 10, null, 0.80000000000000004, 45, "S", new TimeOnly(16, 0, 0), null },
                    { 11, null, 0.55000000000000004, 45, "S", new TimeOnly(17, 0, 0), null },
                    { 12, null, 0.29999999999999999, 45, "S", new TimeOnly(18, 0, 0), null },
                    { 13, null, 0.10000000000000001, 45, "S", new TimeOnly(19, 0, 0), null },
                    { 14, null, 0.050000000000000003, 45, "S", new TimeOnly(20, 0, 0), null },
                    { 15, null, 0.10000000000000001, 45, "SE", new TimeOnly(7, 0, 0), null },
                    { 16, null, 0.20000000000000001, 45, "SE", new TimeOnly(8, 0, 0), null },
                    { 17, null, 0.5, 45, "SE", new TimeOnly(9, 0, 0), null },
                    { 18, null, 0.69999999999999996, 45, "SE", new TimeOnly(10, 0, 0), null },
                    { 19, null, 1.0, 45, "SE", new TimeOnly(11, 0, 0), null },
                    { 20, null, 1.1000000000000001, 45, "SE", new TimeOnly(12, 0, 0), null },
                    { 21, null, 1.1000000000000001, 45, "SE", new TimeOnly(13, 0, 0), null },
                    { 22, null, 1.0, 45, "SE", new TimeOnly(14, 0, 0), null },
                    { 23, null, 0.84999999999999998, 45, "SE", new TimeOnly(15, 0, 0), null },
                    { 24, null, 0.69999999999999996, 45, "SE", new TimeOnly(16, 0, 0), null },
                    { 25, null, 0.5, 45, "SE", new TimeOnly(17, 0, 0), null },
                    { 26, null, 0.29999999999999999, 45, "SE", new TimeOnly(18, 0, 0), null },
                    { 27, null, 0.10000000000000001, 45, "SE", new TimeOnly(19, 0, 0), null },
                    { 28, null, 0.050000000000000003, 45, "SE", new TimeOnly(20, 0, 0), null },
                    { 29, null, 0.050000000000000003, 45, "SW", new TimeOnly(7, 0, 0), null },
                    { 30, null, 0.10000000000000001, 45, "SW", new TimeOnly(8, 0, 0), null },
                    { 31, null, 0.29999999999999999, 45, "SW", new TimeOnly(9, 0, 0), null },
                    { 32, null, 0.5, 45, "SW", new TimeOnly(10, 0, 0), null },
                    { 33, null, 0.69999999999999996, 45, "SW", new TimeOnly(11, 0, 0), null },
                    { 34, null, 0.84999999999999998, 45, "SW", new TimeOnly(12, 0, 0), null },
                    { 35, null, 1.0, 45, "SW", new TimeOnly(13, 0, 0), null },
                    { 36, null, 1.1000000000000001, 45, "SW", new TimeOnly(14, 0, 0), null },
                    { 37, null, 1.1000000000000001, 45, "SW", new TimeOnly(15, 0, 0), null },
                    { 38, null, 1.0, 45, "SW", new TimeOnly(16, 0, 0), null },
                    { 39, null, 0.69999999999999996, 45, "SW", new TimeOnly(17, 0, 0), null },
                    { 40, null, 0.5, 45, "SW", new TimeOnly(18, 0, 0), null },
                    { 41, null, 0.20000000000000001, 45, "SW", new TimeOnly(19, 0, 0), null },
                    { 42, null, 0.10000000000000001, 45, "SW", new TimeOnly(20, 0, 0), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountrySolarCapacity_CountryId",
                table: "CountrySolarCapacity",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountrySolarCapacity");

            migrationBuilder.DropTable(
                name: "SolarPanelCapacity");
        }
    }
}
