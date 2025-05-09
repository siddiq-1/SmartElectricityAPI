using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class CompensateToSelfUse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ActionName", "ActionTypeCommand" },
                values: new object[] { "Self use", "SelfUse" });

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ActionName", "ActionTypeCommand" },
                values: new object[] { "Self use", "SelfUse" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ActionName", "ActionTypeCommand" },
                values: new object[] { "Compensate missing energy", "CompensateMissingEnergy" });

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ActionName", "ActionTypeCommand" },
                values: new object[] { "Compensate missing energy", "CompensateMissingEnergy" });
        }
    }
}
