using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class fuseboxDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FuseBoxPowSet",
                type: "datetime(0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FuseBoxPowSet",
                type: "datetime(0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FuseBoxMessageHeader",
                type: "datetime(0)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FuseBoxMessageHeader",
                type: "datetime(0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FuseBoxPowSet");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FuseBoxPowSet");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FuseBoxMessageHeader");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FuseBoxMessageHeader");
        }
    }
}
