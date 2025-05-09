using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class FuseboxDataModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceName",
                table: "FuseBoxPowSet",
                newName: "BipolarControl");

            migrationBuilder.InsertData(
                table: "InverterTypeActions",
                columns: new[] { "Id", "ActionName", "ActionType", "ActionTypeCommand", "ButtonBorderColor", "CreatedAt", "InverterTypeId", "OrderSequence", "UpdatedAt" },
                values: new object[] { 9, "Fusebox control", "ExternalControlFuseBox", "FuseBoxControl", "", null, 1, 0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.RenameColumn(
                name: "BipolarControl",
                table: "FuseBoxPowSet",
                newName: "DeviceName");
        }
    }
}
