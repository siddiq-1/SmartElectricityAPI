using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateLatest5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SofarStateLatest_SofarState_Id",
                table: "SofarStateLatest");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SofarStateLatest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SofarStateLatest",
                type: "datetime(0)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "SofarStateLatest",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegisteredInverterId",
                table: "SofarStateLatest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SofarStateId",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeHour",
                table: "SofarStateLatest",
                type: "time(0)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SofarStateLatest",
                type: "datetime(0)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "batterySOC",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "battery_current",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "battery_cycles",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "battery_power",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "battery_temp",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "battery_voltage",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "consumption",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "deviceName",
                table: "SofarStateLatest",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "grid_power",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "inverter_HStemp",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "inverter_power",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "inverter_temp",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "running_state",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "solarPV",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "solarPV1",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "solarPV1Current",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "solarPV2",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "solarPV2Current",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "today_charged",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "today_consumption",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "today_discharged",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "today_exported",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "today_generation",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "today_purchase",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_charged",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_consumption",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_discharged",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_exported",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_generation",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total_purchase",
                table: "SofarStateLatest",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateLatest_CompanyId",
                table: "SofarStateLatest",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateLatest_RegisteredInverterId",
                table: "SofarStateLatest",
                column: "RegisteredInverterId");

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateLatest_SofarStateId",
                table: "SofarStateLatest",
                column: "SofarStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_SofarStateLatest_Company_CompanyId",
                table: "SofarStateLatest",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SofarStateLatest_RegisteredInverter_RegisteredInverterId",
                table: "SofarStateLatest",
                column: "RegisteredInverterId",
                principalTable: "RegisteredInverter",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SofarStateLatest_Company_CompanyId",
                table: "SofarStateLatest");

            migrationBuilder.DropForeignKey(
                name: "FK_SofarStateLatest_RegisteredInverter_RegisteredInverterId",
                table: "SofarStateLatest");

            migrationBuilder.DropIndex(
                name: "IX_SofarStateLatest_CompanyId",
                table: "SofarStateLatest");

            migrationBuilder.DropIndex(
                name: "IX_SofarStateLatest_RegisteredInverterId",
                table: "SofarStateLatest");

            migrationBuilder.DropIndex(
                name: "IX_SofarStateLatest_SofarStateId",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "RegisteredInverterId",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "SofarStateId",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "TimeHour",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "batterySOC",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "battery_current",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "battery_cycles",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "battery_power",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "battery_temp",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "battery_voltage",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "consumption",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "deviceName",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "grid_power",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "inverter_HStemp",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "inverter_power",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "inverter_temp",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "running_state",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "solarPV",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "solarPV1",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "solarPV1Current",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "solarPV2",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "solarPV2Current",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "today_charged",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "today_consumption",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "today_discharged",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "today_exported",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "today_generation",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "today_purchase",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "total_charged",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "total_consumption",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "total_discharged",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "total_exported",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "total_generation",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "total_purchase",
                table: "SofarStateLatest");

            migrationBuilder.AddForeignKey(
                name: "FK_SofarStateLatest_SofarState_Id",
                table: "SofarStateLatest",
                column: "Id",
                principalTable: "SofarState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
