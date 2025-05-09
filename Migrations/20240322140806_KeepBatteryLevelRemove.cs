using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class KeepBatteryLevelRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InverterTypeActions",
                columns: new[] { "Id", "ActionName", "ActionType", "ActionTypeCommand", "ButtonBorderColor", "CreatedAt", "InverterTypeId", "OrderSequence", "UpdatedAt" },
                values: new object[] { 3, "Keep battery level", "Charge", "KeepBatteryLevel", "", null, 1, 0, null });

            migrationBuilder.InsertData(
                table: "InverterTypeCompanyActions",
                columns: new[] { "Id", "ActionName", "ActionState", "ActionType", "ActionTypeCommand", "CompanyId", "CreatedAt", "InverterId", "InverterTypeActionsId", "InverterTypeId", "UpdatedAt" },
                values: new object[] { 3, "Keep battery level", false, "Charge", "KeepBatteryLevel", 1, null, 1, 3, 1, null });
        }
    }
}
