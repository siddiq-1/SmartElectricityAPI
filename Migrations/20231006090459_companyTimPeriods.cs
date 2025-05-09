using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyTimPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DayEndTime",
                table: "Company",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DayStartTime",
                table: "Company",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NightEndTime",
                table: "Company",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NightStartTime",
                table: "Company",
                type: "time(6)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DayEndTime", "DayStartTime", "NightEndTime", "NightStartTime" },
                values: new object[] { new TimeSpan(0, 22, 0, 0, 0), new TimeSpan(0, 7, 0, 0, 0), new TimeSpan(0, 7, 0, 0, 0), new TimeSpan(0, 22, 0, 0, 0) });

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DayEndTime", "DayStartTime", "NightEndTime", "NightStartTime" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayEndTime",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "DayStartTime",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "NightEndTime",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "NightStartTime",
                table: "Company");
        }
    }
}
