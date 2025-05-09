using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterTypeCompanyActions2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InverterId",
                table: "InverterTypeCompanyActions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "InverterId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "InverterId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "InverterId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "InverterId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "InverterId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypeCompanyActions_InverterId",
                table: "InverterTypeCompanyActions",
                column: "InverterId");

            migrationBuilder.AddForeignKey(
                name: "FK_InverterTypeCompanyActions_Inverter_InverterId",
                table: "InverterTypeCompanyActions",
                column: "InverterId",
                principalTable: "Inverter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InverterTypeCompanyActions_Inverter_InverterId",
                table: "InverterTypeCompanyActions");

            migrationBuilder.DropIndex(
                name: "IX_InverterTypeCompanyActions_InverterId",
                table: "InverterTypeCompanyActions");

            migrationBuilder.DropColumn(
                name: "InverterId",
                table: "InverterTypeCompanyActions");
        }
    }
}
