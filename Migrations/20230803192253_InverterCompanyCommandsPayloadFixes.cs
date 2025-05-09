using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class InverterCompanyCommandsPayloadFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InverterCompanyCommandsPayload_Company_CompanyId",
                table: "InverterCompanyCommandsPayload");

            migrationBuilder.DropIndex(
                name: "IX_InverterCompanyCommandsPayload_CompanyId",
                table: "InverterCompanyCommandsPayload");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "InverterCompanyCommandsPayload");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "InverterCompanyCommandsPayload",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "InverterCompanyCommandsPayload",
                keyColumn: "Id",
                keyValue: 1,
                column: "CompanyId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "InverterCompanyCommandsPayload",
                keyColumn: "Id",
                keyValue: 2,
                column: "CompanyId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_InverterCompanyCommandsPayload_CompanyId",
                table: "InverterCompanyCommandsPayload",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_InverterCompanyCommandsPayload_Company_CompanyId",
                table: "InverterCompanyCommandsPayload",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
