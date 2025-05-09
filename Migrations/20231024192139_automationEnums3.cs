using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class automationEnums3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InverterTypeActions",
                columns: new[] { "Id", "ActionName", "ActionType", "ActionTypeCommand", "CreatedAt", "InverterTypeId", "UpdatedAt" },
                values: new object[] { 6, "Automatic control", "Automode", "AutoMode", null, 1, null });

            migrationBuilder.InsertData(
                table: "InverterTypeCompanyActions",
                columns: new[] { "Id", "ActionName", "ActionState", "ActionType", "ActionTypeCommand", "CompanyId", "CreatedAt", "InverterId", "InverterTypeActionsId", "InverterTypeId", "UpdatedAt" },
                values: new object[] { 11, "Automatic control", false, "Automode", "AutoMode", 1, null, 1, 6, 1, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
