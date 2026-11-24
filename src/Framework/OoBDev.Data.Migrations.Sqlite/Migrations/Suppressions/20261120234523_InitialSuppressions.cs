using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OoBDev.Data.Migrations.Sqlite.Migrations.Suppressions
{
    /// <inheritdoc />
    public partial class InitialSuppressions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppressions",
                columns: table => new
                {
                    Address = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AddedAtTicks = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppressions", x => x.Address);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Suppressions");
        }
    }
}
