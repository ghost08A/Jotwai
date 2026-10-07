using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.AiCategorization
{
    /// <inheritdoc />
    public partial class AddNoteSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteProjections",
                schema: "ai_categorization",
                columns: table => new
                {
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteProjections", x => x.NoteId);
                });

            migrationBuilder.CreateTable(
                name: "NoteSuggestionOutboxEntries",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteSuggestionOutboxEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoteSuggestions",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    TargetNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteSuggestions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoteProjections_UserId",
                schema: "ai_categorization",
                table: "NoteProjections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteSuggestionOutboxEntries_ProcessedAt",
                schema: "ai_categorization",
                table: "NoteSuggestionOutboxEntries",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NoteSuggestions_NoteId",
                schema: "ai_categorization",
                table: "NoteSuggestions",
                column: "NoteId",
                unique: true,
                filter: "\"ResolvedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoteProjections",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "NoteSuggestionOutboxEntries",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "NoteSuggestions",
                schema: "ai_categorization");
        }
    }
}
