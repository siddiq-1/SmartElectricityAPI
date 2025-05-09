using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterHoursAvgConsumptionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InverterHoursAvgConsumption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RegisteredInverterId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    InverterId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeHour = table.Column<TimeSpan>(type: "time(0)", nullable: false),
                    AvgHourlyConsumption = table.Column<double>(type: "double", nullable: false),
                    DateCalculated = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterHoursAvgConsumption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterHoursAvgConsumption_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterHoursAvgConsumption_Inverter_InverterId",
                        column: x => x.InverterId,
                        principalTable: "Inverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InverterHoursAvgConsumption_RegisteredInverter_RegisteredInv~",
                        column: x => x.RegisteredInverterId,
                        principalTable: "RegisteredInverter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InverterHoursAvgConsumption_CompanyId",
                table: "InverterHoursAvgConsumption",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InverterHoursAvgConsumption_InverterId_RegisteredInverterId_~",
                table: "InverterHoursAvgConsumption",
                columns: new[] { "InverterId", "RegisteredInverterId", "DateCalculated", "DayOfWeek", "TimeHour" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InverterHoursAvgConsumption_RegisteredInverterId",
                table: "InverterHoursAvgConsumption",
                column: "RegisteredInverterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterHoursAvgConsumption");
        }
    }
}
