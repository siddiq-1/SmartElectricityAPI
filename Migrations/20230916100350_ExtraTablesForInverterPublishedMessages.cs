using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class ExtraTablesForInverterPublishedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InverterTypeListenTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InverterTypeId = table.Column<int>(type: "int", nullable: false),
                    TopicName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterTypeListenTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterTypeListenTopics_InverterType_InverterTypeId",
                        column: x => x.InverterTypeId,
                        principalTable: "InverterType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "InverterTypeListenTopics",
                columns: new[] { "Id", "CreatedAt", "InverterTypeId", "TopicName", "UpdatedAt" },
                values: new object[] { 1, null, 1, "/state", null });

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypeListenTopics_InverterTypeId",
                table: "InverterTypeListenTopics",
                column: "InverterTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InverterTypeListenTopics");
        }
    }
}
