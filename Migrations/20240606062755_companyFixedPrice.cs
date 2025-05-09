using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyFixedPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseFixedPrices",
                table: "Company",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CompanyFixedPrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<double>(type: "double", nullable: false),
                    SalesPrice = table.Column<double>(type: "double", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFixedPrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFixedPrice_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompanyFixedPriceTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyFixedPriceId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PurchasePrice = table.Column<double>(type: "double", nullable: false),
                    SalesPrice = table.Column<double>(type: "double", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyFixedPriceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFixedPriceTransactions_CompanyFixedPrice_CompanyFixed~",
                        column: x => x.CompanyFixedPriceId,
                        principalTable: "CompanyFixedPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyFixedPriceTransactions_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                column: "UseFixedPrices",
                value: false);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                column: "UseFixedPrices",
                value: false);

            migrationBuilder.InsertData(
                table: "CompanyFixedPrice",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "PurchasePrice", "SalesPrice", "Time", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, null, 0.12, 0.16, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 2, 1, null, 0.12, 0.16, new TimeSpan(0, 1, 0, 0, 0), null },
                    { 3, 1, null, 0.12, 0.16, new TimeSpan(0, 2, 0, 0, 0), null },
                    { 4, 1, null, 0.12, 0.16, new TimeSpan(0, 3, 0, 0, 0), null },
                    { 5, 1, null, 0.12, 0.16, new TimeSpan(0, 4, 0, 0, 0), null },
                    { 6, 1, null, 0.12, 0.16, new TimeSpan(0, 5, 0, 0, 0), null },
                    { 7, 1, null, 0.12, 0.16, new TimeSpan(0, 6, 0, 0, 0), null },
                    { 8, 1, null, 0.12, 0.16, new TimeSpan(0, 7, 0, 0, 0), null },
                    { 9, 1, null, 0.12, 0.16, new TimeSpan(0, 8, 0, 0, 0), null },
                    { 10, 1, null, 0.12, 0.16, new TimeSpan(0, 9, 0, 0, 0), null },
                    { 11, 1, null, 0.12, 0.16, new TimeSpan(0, 10, 0, 0, 0), null },
                    { 12, 1, null, 0.12, 0.16, new TimeSpan(0, 11, 0, 0, 0), null },
                    { 13, 1, null, 0.12, 0.16, new TimeSpan(0, 12, 0, 0, 0), null },
                    { 14, 1, null, 0.12, 0.16, new TimeSpan(0, 13, 0, 0, 0), null },
                    { 15, 1, null, 0.12, 0.16, new TimeSpan(0, 14, 0, 0, 0), null },
                    { 16, 1, null, 0.12, 0.16, new TimeSpan(0, 15, 0, 0, 0), null },
                    { 17, 1, null, 0.12, 0.16, new TimeSpan(0, 16, 0, 0, 0), null },
                    { 18, 1, null, 0.12, 0.16, new TimeSpan(0, 17, 0, 0, 0), null },
                    { 19, 1, null, 0.12, 0.16, new TimeSpan(0, 18, 0, 0, 0), null },
                    { 20, 1, null, 0.12, 0.16, new TimeSpan(0, 19, 0, 0, 0), null },
                    { 21, 1, null, 0.12, 0.16, new TimeSpan(0, 20, 0, 0, 0), null },
                    { 22, 1, null, 0.12, 0.16, new TimeSpan(0, 21, 0, 0, 0), null },
                    { 23, 1, null, 0.12, 0.16, new TimeSpan(0, 22, 0, 0, 0), null },
                    { 24, 1, null, 0.12, 0.16, new TimeSpan(0, 23, 0, 0, 0), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFixedPrice_CompanyId",
                table: "CompanyFixedPrice",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFixedPriceTransactions_CompanyFixedPriceId",
                table: "CompanyFixedPriceTransactions",
                column: "CompanyFixedPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFixedPriceTransactions_CompanyId",
                table: "CompanyFixedPriceTransactions",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyFixedPriceTransactions");

            migrationBuilder.DropTable(
                name: "CompanyFixedPrice");

            migrationBuilder.DropColumn(
                name: "UseFixedPrices",
                table: "Company");
        }
    }
}
