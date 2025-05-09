using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class batterycontrolhoursmodelfixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "UsableWattsProfit",
                table: "BatteryControlHours",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinPriceWithCost",
                table: "BatteryControlHours",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxPriceWithCost",
                table: "BatteryControlHours",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "MaxPriceHour",
                table: "BatteryControlHours",
                type: "time(0)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxMinPriceDifference",
                table: "BatteryControlHours",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineProfit",
                table: "BatteryControlHours",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourProfit",
                table: "BatteryControlHours",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionTypeCommand",
                table: "BatteryControlHours",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "UsableWattsProfit",
                table: "BatteryControlHours",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MinPriceWithCost",
                table: "BatteryControlHours",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MaxPriceWithCost",
                table: "BatteryControlHours",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "MaxPriceHour",
                table: "BatteryControlHours",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MaxMinPriceDifference",
                table: "BatteryControlHours",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "LineProfit",
                table: "BatteryControlHours",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "HourProfit",
                table: "BatteryControlHours",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ActionTypeCommand",
                table: "BatteryControlHours",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
