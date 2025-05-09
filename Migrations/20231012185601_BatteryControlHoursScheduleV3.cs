using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class BatteryControlHoursScheduleV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SelfUseMinPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SelfUseMaxPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SelfUseMaxMinPriceDifference",
                table: "BatteryControlHoursSchedule",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesMinPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesMaxPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesMaxMinPriceDifference",
                table: "BatteryControlHoursSchedule",
                type: "decimal(10,4)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeHour",
                table: "BatteryControlHoursSchedule",
                type: "time(0)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeHour",
                table: "BatteryControlHoursSchedule");

            migrationBuilder.AlterColumn<double>(
                name: "SelfUseMinPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SelfUseMaxPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SelfUseMaxMinPriceDifference",
                table: "BatteryControlHoursSchedule",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SalesMinPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SalesMaxPriceWithCost",
                table: "BatteryControlHoursSchedule",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "SalesMaxMinPriceDifference",
                table: "BatteryControlHoursSchedule",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldNullable: true);
        }
    }
}
