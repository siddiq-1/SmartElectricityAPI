using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterType2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "InverterTypeId",
                value: 1);

            migrationBuilder.InsertData(
                table: "InverterType",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { 1, null, "HYD 5-20KTL-3PH", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterType",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "InverterTypeId",
                value: 0);
        }
    }
}
