using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class renameActionToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "InverterTypeCompanyActions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionType",
                value: "Charge");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionType",
                value: "Charge");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActionType",
                value: "Charge");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ActionType",
                value: "Discharge");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "ActionType",
                value: "Discharge");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ActionType",
                table: "InverterTypeCompanyActions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionType",
                value: 7);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionType",
                value: 7);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActionType",
                value: 7);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ActionType",
                value: 8);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "ActionType",
                value: 8);
        }
    }
}
