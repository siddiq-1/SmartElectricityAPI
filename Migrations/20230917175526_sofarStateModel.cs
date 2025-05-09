using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class sofarStateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SofarState",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    uptime = table.Column<int>(type: "int", nullable: false),
                    deviceName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    running_state = table.Column<int>(type: "int", nullable: false),
                    inverter_temp = table.Column<double>(type: "double", nullable: false),
                    inverter_HStemp = table.Column<double>(type: "double", nullable: false),
                    grid_freq = table.Column<double>(type: "double", nullable: false),
                    inverter_power = table.Column<int>(type: "int", nullable: false),
                    grid_power = table.Column<int>(type: "int", nullable: false),
                    grid_voltage = table.Column<double>(type: "double", nullable: false),
                    consumption = table.Column<int>(type: "int", nullable: false),
                    solarPV1Volt = table.Column<double>(type: "double", nullable: false),
                    solarPV1Current = table.Column<double>(type: "double", nullable: false),
                    solarPV1 = table.Column<int>(type: "int", nullable: false),
                    solarPV2Volt = table.Column<double>(type: "double", nullable: false),
                    solarPV2Current = table.Column<double>(type: "double", nullable: false),
                    solarPV2 = table.Column<int>(type: "int", nullable: false),
                    solarPV = table.Column<int>(type: "int", nullable: false),
                    battery_voltage = table.Column<double>(type: "double", nullable: false),
                    battery_current = table.Column<double>(type: "double", nullable: false),
                    battery_power = table.Column<double>(type: "double", nullable: false),
                    battery_temp = table.Column<double>(type: "double", nullable: false),
                    batterySOC = table.Column<int>(type: "int", nullable: false),
                    battery_cycles = table.Column<double>(type: "double", nullable: false),
                    today_generation = table.Column<double>(type: "double", nullable: false),
                    total_generation = table.Column<double>(type: "double", nullable: false),
                    today_consumption = table.Column<double>(type: "double", nullable: false),
                    total_consumption = table.Column<double>(type: "double", nullable: false),
                    today_purchase = table.Column<double>(type: "double", nullable: false),
                    total_purchase = table.Column<double>(type: "double", nullable: false),
                    today_exported = table.Column<double>(type: "double", nullable: false),
                    total_exported = table.Column<double>(type: "double", nullable: false),
                    today_charged = table.Column<double>(type: "double", nullable: false),
                    total_charged = table.Column<double>(type: "double", nullable: false),
                    today_discharged = table.Column<double>(type: "double", nullable: false),
                    total_discharged = table.Column<double>(type: "double", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SofarState", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SofarState");
        }
    }
}
