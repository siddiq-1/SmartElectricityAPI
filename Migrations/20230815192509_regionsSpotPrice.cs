using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class regionsSpotPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropIndex(
                name: "IX_SpotPrice_DateTime",
                table: "SpotPrice");
            */
            /*
                    migrationBuilder.CreateIndex(
            name: "IX_SpotPrice_DateTime_RegionId",
            table: "SpotPrice",
            columns: new[] { "DateTime", "RegionId" },
            unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpotPrice_RegionId",
                table: "SpotPrice",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpotPrice_Region_RegionId",
                table: "SpotPrice",
                column: "RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
