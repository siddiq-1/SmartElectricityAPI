using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class inverterSelfUse2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InverterTypeActions",
                columns: new[] { "Id", "ActionName", "ActionType", "ActionTypeCommand", "ButtonBorderColor", "CreatedAt", "InverterTypeId", "OrderSequence", "UpdatedAt" },
                values: new object[] { 8, "Inverter Self use", "ModeControl", "InverterSelfUse", "", null, 1, 0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
