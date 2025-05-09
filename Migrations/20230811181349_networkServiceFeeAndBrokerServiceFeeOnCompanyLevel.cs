using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class networkServiceFeeAndBrokerServiceFeeOnCompanyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NetworkFeeNightTime",
                table: "Company",
                newName: "NetworkServiceFeeWeekend");

            migrationBuilder.RenameColumn(
                name: "NetworkFeeDayTime",
                table: "Company",
                newName: "BrokerServiceFeeWeekend");

            migrationBuilder.AddColumn<bool>(
                name: "UseWeekendFeeOnSaturdayAndSunday",
                table: "Company",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BrokerServiceFeeWeekend", "NetworkServiceFeeWeekend", "UseWeekendFeeOnSaturdayAndSunday" },
                values: new object[] { 0.071999999999999995, 0.050500000000000003, false });

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BrokerServiceFeeWeekend", "NetworkServiceFeeWeekend", "UseWeekendFeeOnSaturdayAndSunday" },
                values: new object[] { 0.082000000000000003, 0.060499999999999998, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseWeekendFeeOnSaturdayAndSunday",
                table: "Company");

            migrationBuilder.RenameColumn(
                name: "NetworkServiceFeeWeekend",
                table: "Company",
                newName: "NetworkFeeNightTime");

            migrationBuilder.RenameColumn(
                name: "BrokerServiceFeeWeekend",
                table: "Company",
                newName: "NetworkFeeDayTime");

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "NetworkFeeDayTime", "NetworkFeeNightTime" },
                values: new object[] { 9.0, 5.0 });

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "NetworkFeeDayTime", "NetworkFeeNightTime" },
                values: new object[] { 8.0, 4.0 });
        }
    }
}
