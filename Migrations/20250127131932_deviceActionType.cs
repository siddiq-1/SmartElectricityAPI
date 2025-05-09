using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class deviceActionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "Switch",
                newName: "DeviceActionType");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "Sensor",
                newName: "DeviceActionType");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "DeviceCompanyHours",
                newName: "DeviceActionType");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceCompanyHours_DeviceId_SpotPriceId_CompanyId_ActionType",
                table: "DeviceCompanyHours",
                newName: "IX_DeviceCompanyHours_DeviceId_SpotPriceId_CompanyId_DeviceActi~");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeviceActionType",
                value: "None");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeviceActionType",
                value: "None");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeviceActionType",
                value: "None");

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeviceActionType",
                value: "None");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceActionType",
                table: "Switch",
                newName: "ActionType");

            migrationBuilder.RenameColumn(
                name: "DeviceActionType",
                table: "Sensor",
                newName: "ActionType");

            migrationBuilder.RenameColumn(
                name: "DeviceActionType",
                table: "DeviceCompanyHours",
                newName: "ActionType");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceCompanyHours_DeviceId_SpotPriceId_CompanyId_DeviceActi~",
                table: "DeviceCompanyHours",
                newName: "IX_DeviceCompanyHours_DeviceId_SpotPriceId_CompanyId_ActionType");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionType",
                value: "Off");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionType",
                value: "Off");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActionType",
                value: "Off");

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionType",
                value: "Off");
        }
    }
}
