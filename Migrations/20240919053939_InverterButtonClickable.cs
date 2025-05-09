using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterButtonClickable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClickable",
                table: "InverterTypeActions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsClickable",
                value: true);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsClickable",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClickable",
                table: "InverterTypeActions");
        }
    }
}
