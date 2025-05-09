using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateLatest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "grid_freq",
                table: "SofarState");

            migrationBuilder.DropColumn(
                name: "grid_voltage",
                table: "SofarState");

            migrationBuilder.DropColumn(
                name: "solarPV1Volt",
                table: "SofarState");

            migrationBuilder.DropColumn(
                name: "solarPV2Volt",
                table: "SofarState");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "SofarState",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateTime",
                value: new DateTime(2023, 5, 13, 20, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateTime",
                value: new DateTime(2023, 5, 13, 21, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 3,
                column: "DateTime",
                value: new DateTime(2023, 5, 13, 22, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 4,
                column: "DateTime",
                value: new DateTime(2023, 5, 13, 23, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 5,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 6,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 7,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 2, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 8,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 3, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 9,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 4, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 10,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 5, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 11,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 6, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 12,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 7, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 13,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 14,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 9, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 15,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 10, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 16,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 11, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 17,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 12, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 18,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 13, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 19,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 14, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 20,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 15, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 21,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 16, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 22,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 17, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 23,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 18, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 24,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 19, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 25,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 20, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 26,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 21, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 27,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 22, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 28,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 23, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 29,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 30,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 31,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 2, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 32,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 3, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 33,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 4, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 34,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 5, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 35,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 6, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 36,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 7, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 37,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 38,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 9, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 39,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 10, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 40,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 11, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 41,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 12, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 42,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 13, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 43,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 14, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 44,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 15, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 45,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 16, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 46,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 17, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 47,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 18, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 48,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 19, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 49,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 20, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "SofarState");

            migrationBuilder.AddColumn<double>(
                name: "grid_freq",
                table: "SofarState",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "grid_voltage",
                table: "SofarState",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "solarPV1Volt",
                table: "SofarState",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "solarPV2Volt",
                table: "SofarState",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateTime",
                value: new DateTime(2023, 5, 13, 23, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 2,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 3,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 4,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 2, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 5,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 3, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 6,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 4, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 7,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 5, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 8,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 6, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 9,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 7, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 10,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 11,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 9, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 12,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 10, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 13,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 11, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 14,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 12, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 15,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 13, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 16,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 14, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 17,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 15, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 18,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 16, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 19,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 17, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 20,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 18, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 21,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 19, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 22,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 20, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 23,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 21, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 24,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 22, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 25,
                column: "DateTime",
                value: new DateTime(2023, 5, 14, 23, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 26,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 27,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 28,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 2, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 29,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 3, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 30,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 4, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 31,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 5, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 32,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 6, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 33,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 7, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 34,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 8, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 35,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 9, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 36,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 10, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 37,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 11, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 38,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 12, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 39,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 13, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 40,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 14, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 41,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 15, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 42,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 16, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 43,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 17, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 44,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 18, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 45,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 19, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 46,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 20, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 47,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 21, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 48,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 22, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SpotPrice",
                keyColumn: "Id",
                keyValue: 49,
                column: "DateTime",
                value: new DateTime(2023, 5, 15, 23, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
