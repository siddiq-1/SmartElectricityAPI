using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class CompanyHourlyFeesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyHourlyFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Time = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    NetworkServiceFree = table.Column<double>(type: "double", nullable: false),
                    BrokerServiceFree = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyHourlyFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyHourlyFees_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "CompanyHourlyFees",
                columns: new[] { "Id", "BrokerServiceFree", "CompanyId", "CreatedAt", "NetworkServiceFree", "Time", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 2, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 1, 0, 0, 0), null },
                    { 3, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 2, 0, 0, 0), null },
                    { 4, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 3, 0, 0, 0), null },
                    { 5, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 4, 0, 0, 0), null },
                    { 6, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 5, 0, 0, 0), null },
                    { 7, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 6, 0, 0, 0), null },
                    { 8, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 7, 0, 0, 0), null },
                    { 9, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 8, 0, 0, 0), null },
                    { 10, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 9, 0, 0, 0), null },
                    { 11, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 10, 0, 0, 0), null },
                    { 12, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 11, 0, 0, 0), null },
                    { 13, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 12, 0, 0, 0), null },
                    { 14, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 13, 0, 0, 0), null },
                    { 15, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 14, 0, 0, 0), null },
                    { 16, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 15, 0, 0, 0), null },
                    { 17, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 16, 0, 0, 0), null },
                    { 18, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 17, 0, 0, 0), null },
                    { 19, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 18, 0, 0, 0), null },
                    { 20, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 19, 0, 0, 0), null },
                    { 21, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 20, 0, 0, 0), null },
                    { 23, 0.071999999999999995, 1, null, 0.086800000000000002, new TimeSpan(0, 21, 0, 0, 0), null },
                    { 24, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 22, 0, 0, 0), null },
                    { 25, 0.071999999999999995, 1, null, 0.050500000000000003, new TimeSpan(0, 23, 0, 0, 0), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyHourlyFees_CompanyId",
                table: "CompanyHourlyFees",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyHourlyFees");
        }
    }
}
