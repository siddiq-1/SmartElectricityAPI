using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartElectricityAPI.Migrations
{
    /// <inheritdoc />
    public partial class LogMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Log",
                type: "varchar(1568)",
                maxLength: 1568,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(784)",
                oldMaxLength: 784)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Log",
                type: "varchar(784)",
                maxLength: 784,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1568)",
                oldMaxLength: 1568)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
