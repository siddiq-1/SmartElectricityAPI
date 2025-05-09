using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterTypeActionsOrderSeq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderSequence",
                table: "InverterTypeActions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "OrderSequence",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "OrderSequence",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "OrderSequence",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "OrderSequence",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "OrderSequence",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 6,
                column: "OrderSequence",
                value: 0);

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 7,
                column: "OrderSequence",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderSequence",
                table: "InverterTypeActions");
        }
    }
}
