﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeviceCompanyHoursIsProcessed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "DeviceCompanyHours",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "DeviceCompanyHours");
        }
    }
}
