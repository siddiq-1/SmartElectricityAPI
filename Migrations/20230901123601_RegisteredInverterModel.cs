using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class RegisteredInverterModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Inverter",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RegisteredInverterId",
                table: "Inverter",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegisteredInverter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredInverter", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Inverter",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisteredInverterId",
                value: 1);

            migrationBuilder.InsertData(
                table: "RegisteredInverter",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { 1, null, "SofarMQTTJaamari20kw", null });

            migrationBuilder.CreateIndex(
                name: "IX_Inverter_RegisteredInverterId",
                table: "Inverter",
                column: "RegisteredInverterId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredInverter_Name",
                table: "RegisteredInverter",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inverter_RegisteredInverter_RegisteredInverterId",
                table: "Inverter",
                column: "RegisteredInverterId",
                principalTable: "RegisteredInverter",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inverter_RegisteredInverter_RegisteredInverterId",
                table: "Inverter");

            migrationBuilder.DropTable(
                name: "RegisteredInverter");

            migrationBuilder.DropIndex(
                name: "IX_Inverter_RegisteredInverterId",
                table: "Inverter");

            migrationBuilder.DropColumn(
                name: "RegisteredInverterId",
                table: "Inverter");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Inverter",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
