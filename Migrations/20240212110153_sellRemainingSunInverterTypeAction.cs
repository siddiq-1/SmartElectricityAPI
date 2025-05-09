using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sellRemainingSunInverterTypeAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.InsertData(
                table: "InverterTypeActions",
                columns: new[] { "Id", "ActionName", "ActionType", "ActionTypeCommand", "CreatedAt", "InverterTypeId", "UpdatedAt" },
                values: new object[] { 7, "Sell remaining sun (no charging)", "Charge", "ConsumeBatteryWithMaxPower", null, 1, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DeleteData(
                table: "InverterTypeActions",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
