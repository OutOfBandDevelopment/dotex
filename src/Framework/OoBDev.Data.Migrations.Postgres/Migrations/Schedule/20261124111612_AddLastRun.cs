using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OoBDev.Data.Migrations.Postgres.Migrations.Schedule
{
    /// <inheritdoc />
    public partial class AddLastRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplexEventLastRun",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastRunTicks = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexEventLastRun", x => x.Key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplexEventLastRun");
        }
    }
}
