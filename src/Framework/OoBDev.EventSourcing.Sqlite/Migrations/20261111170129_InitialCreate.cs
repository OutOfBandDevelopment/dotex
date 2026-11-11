using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OoBDev.EventSourcing.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => new { x.Type, x.Key });
                    table.CheckConstraint("CK_Entities_Value_Json", "json_valid(\"Value\")");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Position = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StreamId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Sequence = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Position);
                    table.CheckConstraint("CK_Events_Payload_Json", "json_valid(\"Payload\")");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_StreamId_Sequence",
                table: "Events",
                columns: new[] { "StreamId", "Sequence" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
