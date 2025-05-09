using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterTypeCompanyActions4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionTypeCommand",
                table: "InverterTypeCompanyActions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ActionTypeCommand",
                table: "InverterTypeActions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionTypeCommand",
                value: "ChargeMax");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionTypeCommand",
                value: "ChargeWithRemainingSun");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActionTypeCommand",
                value: "KeepBatteryLevel");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ActionTypeCommand",
                value: "CompensateMissingEnergy");

            migrationBuilder.UpdateData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "ActionTypeCommand",
                value: "ConsumeBatteryWithMaxPower");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActionTypeCommand",
                value: "ChargeMax");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActionTypeCommand",
                value: "ChargeWithRemainingSun");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActionTypeCommand",
                value: "KeepBatteryLevel");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 4,
                column: "ActionTypeCommand",
                value: "CompensateMissingEnergy");

            migrationBuilder.UpdateData(
                table: "InverterTypeCompanyActions",
                keyColumn: "Id",
                keyValue: 5,
                column: "ActionTypeCommand",
                value: "ConsumeBatteryWithMaxPower");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionTypeCommand",
                table: "InverterTypeCompanyActions");

            migrationBuilder.DropColumn(
                name: "ActionTypeCommand",
                table: "InverterTypeActions");
        }
    }
}
