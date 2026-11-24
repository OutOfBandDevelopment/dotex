using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OoBDev.Data.Migrations.Sqlite.Migrations.RightAudit
{
    /// <inheritdoc />
    public partial class InitialRightAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RightAudit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Rights = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Result = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RightAudit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RightAudit_AtTicks",
                table: "RightAudit",
                column: "AtTicks");

            migrationBuilder.CreateIndex(
                name: "IX_RightAudit_Subject_AtTicks",
                table: "RightAudit",
                columns: new[] { "Subject", "AtTicks" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RightAudit");
        }
    }
}
