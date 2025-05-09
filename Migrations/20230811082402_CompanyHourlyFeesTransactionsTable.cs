using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class CompanyHourlyFeesTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "CompanyHourlyFees",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "CompanyHourlyFees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "CompanyHourlyFees",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "CompanyHourlyFees",
                columns: new[] { "Id", "BrokerServiceFree", "CompanyId", "CreatedAt", "Discriminator", "NetworkServiceFree", "Time", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 0, 0, 0, 0), null },
                    { 2, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 1, 0, 0, 0), null },
                    { 3, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 2, 0, 0, 0), null },
                    { 4, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 3, 0, 0, 0), null },
                    { 5, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 4, 0, 0, 0), null },
                    { 6, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 5, 0, 0, 0), null },
                    { 7, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 6, 0, 0, 0), null },
                    { 8, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 7, 0, 0, 0), null },
                    { 9, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 8, 0, 0, 0), null },
                    { 10, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 9, 0, 0, 0), null },
                    { 11, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 10, 0, 0, 0), null },
                    { 12, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 11, 0, 0, 0), null },
                    { 13, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 12, 0, 0, 0), null },
                    { 14, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 13, 0, 0, 0), null },
                    { 15, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 14, 0, 0, 0), null },
                    { 16, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 15, 0, 0, 0), null },
                    { 17, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 16, 0, 0, 0), null },
                    { 18, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 17, 0, 0, 0), null },
                    { 19, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 18, 0, 0, 0), null },
                    { 20, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 19, 0, 0, 0), null },
                    { 21, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 20, 0, 0, 0), null },
                    { 23, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.086800000000000002, new TimeSpan(0, 21, 0, 0, 0), null },
                    { 24, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 22, 0, 0, 0), null },
                    { 25, 0.071999999999999995, 1, null, "CompanyHourlyFees", 0.050500000000000003, new TimeSpan(0, 23, 0, 0, 0), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "CompanyHourlyFees");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "CompanyHourlyFees");
        }
    }
}
