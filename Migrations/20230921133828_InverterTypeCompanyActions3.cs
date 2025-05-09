using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterTypeCompanyActions3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActionState",
                table: "InverterTypeCompanyActions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionState",
                value: false);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionState",
                value: false);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActionState",
                value: false);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ActionState",
                value: false);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "ActionState",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionState",
                table: "InverterTypeCompanyActions");
        }
    }
}
