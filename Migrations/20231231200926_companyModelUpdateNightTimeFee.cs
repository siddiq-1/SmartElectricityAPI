using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class companyModelUpdateNightTimeFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UseNightTimeFreeOnSaturdayAndSunday",
                table: "Company",
                newName: "UseNightTimeFeeOnSaturdayAndSunday");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UseNightTimeFeeOnSaturdayAndSunday",
                table: "Company",
                newName: "UseNightTimeFreeOnSaturdayAndSunday");
        }
    }
}
