using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class FuseBoxModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuseBoxMessageHeader",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    m_id = table.Column<long>(type: "bigint", nullable: false),
                    m_orig_id = table.Column<long>(type: "bigint", nullable: false),
                    m_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_id = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuseBoxMessageHeader", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FuseBoxPowSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FuseBoxMessageHeaderId = table.Column<int>(type: "int", nullable: false),
                    DeviceName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PowerValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuseBoxPowSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuseBoxPowSet_FuseBoxMessageHeader_FuseBoxMessageHeaderId",
                        column: x => x.FuseBoxMessageHeaderId,
                        principalTable: "FuseBoxMessageHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FuseBoxRealTimeMsg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FuseBoxMessageHeaderId = table.Column<int>(type: "int", nullable: false),
                    ts = table.Column<long>(type: "bigint", nullable: false),
                    exp = table.Column<int>(type: "int", nullable: false),
                    FuseBoxPowSetId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuseBoxRealTimeMsg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuseBoxRealTimeMsg_FuseBoxMessageHeader_FuseBoxMessageHeader~",
                        column: x => x.FuseBoxMessageHeaderId,
                        principalTable: "FuseBoxMessageHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FuseBoxRealTimeMsg_FuseBoxPowSet_FuseBoxPowSetId",
                        column: x => x.FuseBoxPowSetId,
                        principalTable: "FuseBoxPowSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FuseBoxSchedRegMsg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FuseBoxMessageHeaderId = table.Column<int>(type: "int", nullable: false),
                    start = table.Column<long>(type: "bigint", nullable: false),
                    end = table.Column<long>(type: "bigint", nullable: false),
                    cancel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FuseBoxPowSetId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuseBoxSchedRegMsg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuseBoxSchedRegMsg_FuseBoxMessageHeader_FuseBoxMessageHeader~",
                        column: x => x.FuseBoxMessageHeaderId,
                        principalTable: "FuseBoxMessageHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FuseBoxSchedRegMsg_FuseBoxPowSet_FuseBoxPowSetId",
                        column: x => x.FuseBoxPowSetId,
                        principalTable: "FuseBoxPowSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FuseBoxPowSet_FuseBoxMessageHeaderId",
                table: "FuseBoxPowSet",
                column: "FuseBoxMessageHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_FuseBoxRealTimeMsg_FuseBoxMessageHeaderId",
                table: "FuseBoxRealTimeMsg",
                column: "FuseBoxMessageHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_FuseBoxRealTimeMsg_FuseBoxPowSetId",
                table: "FuseBoxRealTimeMsg",
                column: "FuseBoxPowSetId");

            migrationBuilder.CreateIndex(
                name: "IX_FuseBoxSchedRegMsg_FuseBoxMessageHeaderId",
                table: "FuseBoxSchedRegMsg",
                column: "FuseBoxMessageHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_FuseBoxSchedRegMsg_FuseBoxPowSetId",
                table: "FuseBoxSchedRegMsg",
                column: "FuseBoxPowSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuseBoxRealTimeMsg");

            migrationBuilder.DropTable(
                name: "FuseBoxSchedRegMsg");

            migrationBuilder.DropTable(
                name: "FuseBoxPowSet");

            migrationBuilder.DropTable(
                name: "FuseBoxMessageHeader");
        }
    }
}
