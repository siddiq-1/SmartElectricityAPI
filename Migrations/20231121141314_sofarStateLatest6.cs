using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateLatest6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SofarStateLatest_SofarStateId",
                table: "SofarStateLatest");

            migrationBuilder.DropColumn(
                name: "SofarStateId",
                table: "SofarStateLatest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SofarStateId",
                table: "SofarStateLatest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateLatest_SofarStateId",
                table: "SofarStateLatest",
                column: "SofarStateId");
        }
    }
}
