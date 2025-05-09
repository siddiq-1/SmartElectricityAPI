using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class RegionOffsetHoursFromEstonianTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OffsetHoursFromEstonianTime",
                table: "Region",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 1,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 2,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 3,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 4,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 5,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 6,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 7,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 8,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 9,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 10,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 11,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 12,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 13,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 14,
                column: "OffsetHoursFromEstonianTime",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 15,
                column: "OffsetHoursFromEstonianTime",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffsetHoursFromEstonianTime",
                table: "Region");
        }
    }
}
