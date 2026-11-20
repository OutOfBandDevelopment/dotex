using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OoBDev.Data.Migrations.Postgres.Migrations.Schedule
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DueAtTicks = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<byte[]>(type: "bytea", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Failed = table.Column<bool>(type: "boolean", nullable: false),
                    LeasedUntilTicks = table.Column<long>(type: "bigint", nullable: false)
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
