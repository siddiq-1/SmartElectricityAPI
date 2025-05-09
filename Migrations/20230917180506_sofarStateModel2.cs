using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegisteredInverterId",
                table: "SofarState",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SofarState_RegisteredInverterId",
                table: "SofarState",
                column: "RegisteredInverterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SofarState_RegisteredInverter_RegisteredInverterId",
                table: "SofarState",
                column: "RegisteredInverterId",
                principalTable: "RegisteredInverter",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SofarState_RegisteredInverter_RegisteredInverterId",
                table: "SofarState");

            migrationBuilder.DropIndex(
                name: "IX_SofarState_RegisteredInverterId",
                table: "SofarState");

            migrationBuilder.DropColumn(
                name: "RegisteredInverterId",
                table: "SofarState");
        }
    }
}
