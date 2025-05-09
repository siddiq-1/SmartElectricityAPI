using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class CountryVatRange2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CountryVatRange",
                columns: new[] { "Id", "CountryId", "CreatedAt", "EndDate", "StartDate", "UpdatedAt", "VatRate" },
                values: new object[,]
                {
                    { 4, 3, null, new DateOnly(2024, 8, 31), new DateOnly(2024, 1, 1), null, 1.24 },
                    { 5, 3, null, new DateOnly(2035, 12, 31), new DateOnly(2024, 9, 1), null, 1.2549999999999999 },
                    { 6, 2, null, new DateOnly(2035, 12, 31), new DateOnly(2024, 1, 1), null, 1.25 },
                    { 7, 8, null, new DateOnly(2035, 12, 31), new DateOnly(2024, 1, 1), null, 21.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CountryVatRange",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CountryVatRange",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CountryVatRange",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CountryVatRange",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
