using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateHourly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SofarStateHourly",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    deviceName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    inverter_tempMin = table.Column<double>(type: "double", nullable: false),
                    inverter_tempMax = table.Column<double>(type: "double", nullable: false),
                    inverter_power = table.Column<int>(type: "int", nullable: false),
                    grid_power = table.Column<int>(type: "int", nullable: false),
                    consumption = table.Column<int>(type: "int", nullable: false),
                    solarPV = table.Column<int>(type: "int", nullable: false),
                    battery_voltage = table.Column<double>(type: "double", nullable: false),
                    battery_current = table.Column<double>(type: "double", nullable: false),
                    battery_power = table.Column<double>(type: "double", nullable: false),
                    battery_tempMin = table.Column<double>(type: "double", nullable: false),
                    battery_tempMax = table.Column<double>(type: "double", nullable: false),
                    batterySOC = table.Column<int>(type: "int", nullable: false),
                    hour_generation = table.Column<double>(type: "double", nullable: false),
                    today_generation = table.Column<double>(type: "double", nullable: false),
                    hour_consumption = table.Column<double>(type: "double", nullable: false),
                    today_consumption = table.Column<double>(type: "double", nullable: false),
                    total_consumption = table.Column<double>(type: "double", nullable: false),
                    hour_purchase = table.Column<double>(type: "double", nullable: false),
                    today_purchase = table.Column<double>(type: "double", nullable: false),
                    total_purchase = table.Column<double>(type: "double", nullable: false),
                    hour_exported = table.Column<double>(type: "double", nullable: false),
                    today_exported = table.Column<double>(type: "double", nullable: false),
                    total_exported = table.Column<double>(type: "double", nullable: false),
                    hour_charged = table.Column<double>(type: "double", nullable: false),
                    today_charged = table.Column<double>(type: "double", nullable: false),
                    total_charged = table.Column<double>(type: "double", nullable: false),
                    hour_discharged = table.Column<double>(type: "double", nullable: false),
                    today_discharged = table.Column<double>(type: "double", nullable: false),
                    total_discharged = table.Column<double>(type: "double", nullable: false),
                    startTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    endTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisteredInverterId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: true),
                    TimeHour = table.Column<TimeSpan>(type: "time(0)", nullable: false),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofarStateHourly", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SofarStateHourly_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SofarStateHourly_RegisteredInverter_RegisteredInverterId",
                        column: x => x.RegisteredInverterId,
                        principalTable: "RegisteredInverter",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateHourly_CompanyId",
                table: "SofarStateHourly",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SofarStateHourly_RegisteredInverterId",
                table: "SofarStateHourly",
                column: "RegisteredInverterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SofarStateHourly");
        }
    }
}
