using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OoBDev.Data.Migrations.Sqlite.Migrations.Schedule
{
    /// <inheritdoc />
    public partial class InitialSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplexEventSchedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DueAtTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Payload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Failed = table.Column<bool>(type: "INTEGER", nullable: false),
                    LeasedUntilTicks = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplexEventSchedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplexEventSchedule_Failed_DueAtTicks",
                table: "ComplexEventSchedule",
                columns: new[] { "Failed", "DueAtTicks" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplexEventSchedule_Key",
                table: "ComplexEventSchedule",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplexEventSchedule");
        }
    }
}
