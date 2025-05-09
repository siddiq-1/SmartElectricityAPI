using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class switchActionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActionState",
                table: "Switch",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionState",
                value: false);

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionState",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionState",
                table: "Switch");
        }
    }
}
