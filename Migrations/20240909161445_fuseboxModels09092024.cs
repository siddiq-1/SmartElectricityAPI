using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class fuseboxModels09092024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "actualEnd",
                table: "FuseBoxSchedRegMsg",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "actualStart",
                table: "FuseBoxSchedRegMsg",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actualEnd",
                table: "FuseBoxSchedRegMsg");

            migrationBuilder.DropColumn(
                name: "actualStart",
                table: "FuseBoxSchedRegMsg");
        }
    }
}
