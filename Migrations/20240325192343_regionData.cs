using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class regionData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { 8, null, "Netherlands", null });

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 2,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 3,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 4,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 5,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 7,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 8,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 9,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 10,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 11,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 12,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 13,
                column: "OffsetHoursFromEstonianTime",
                value: 1);

            migrationBuilder.InsertData(
                table: "Region",
                columns: new[] { "Id", "Abbreviation", "CountryId", "CreatedAt", "OffsetHoursFromEstonianTime", "UpdatedAt" },
                values: new object[] { 16, "NL", 8, null, 1, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Region",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: 8);

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
        }
    }
}
