using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterTypeActionsButtonBorderColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ButtonBorderColor",
                table: "InverterTypeActions",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "ButtonBorderColor",
                value: "");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ButtonBorderColor",
                value: "");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "ButtonBorderColor",
                value: "");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ButtonBorderColor",
                value: "");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "ButtonBorderColor",
                value: "");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 6,
                column: "ButtonBorderColor",
                value: "");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 7,
                column: "ButtonBorderColor",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonBorderColor",
                table: "InverterTypeActions");
        }
    }
}
