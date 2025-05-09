using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class bugfixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SwitchModelId",
                table: "Sensor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 1,
                column: "SwitchModelId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 2,
                column: "SwitchModelId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 3,
                column: "SwitchModelId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "SwitchModelId", "Topic" },
                values: new object[] { 1, "shellyplus1-virgo1/rpc" });

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 2,
                column: "SwitchModelId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Sensor_SwitchModelId",
                table: "Sensor",
                column: "SwitchModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensor_SwitchModel_SwitchModelId",
                table: "Sensor",
                column: "SwitchModelId",
                principalTable: "SwitchModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensor_SwitchModel_SwitchModelId",
                table: "Sensor");

            migrationBuilder.DropIndex(
                name: "IX_Sensor_SwitchModelId",
                table: "Sensor");

            migrationBuilder.DropColumn(
                name: "SwitchModelId",
                table: "Sensor");

            migrationBuilder.UpdateData(
                table: "Sensor",
                keyColumn: "Id",
                keyValue: 4,
                column: "Topic",
                value: "shellies/shellyem3-485519DC6688/emeter/0/energy");

            migrationBuilder.UpdateData(
                table: "Switch",
                keyColumn: "Id",
                keyValue: 2,
                column: "SwitchModelId",
                value: 2);
        }
    }
}
