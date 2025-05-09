using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class SofarStateAdditionaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SofarState",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "SofarState",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SofarState_CompanyId",
                table: "SofarState",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SofarState_Company_CompanyId",
                table: "SofarState",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SofarState_Company_CompanyId",
                table: "SofarState");

            migrationBuilder.DropIndex(
                name: "IX_SofarState_CompanyId",
                table: "SofarState");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SofarState");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "SofarState");
        }
    }
}
