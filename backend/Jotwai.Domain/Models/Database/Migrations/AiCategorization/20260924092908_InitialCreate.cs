using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.AiCategorization
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ai_categorization");

            migrationBuilder.CreateTable(
                name: "CategoryProjections",
                schema: "ai_categorization",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryProjections", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEntries",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ConfidenceThreshold = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suggestions",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suggestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuggestionCandidates",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SuggestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewCategoryName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestionCandidates_Suggestions_SuggestionId",
                        column: x => x.SuggestionId,
                        principalSchema: "ai_categorization",
                        principalTable: "Suggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "ai_categorization",
                table: "Settings",
                columns: new[] { "Id", "ConfidenceThreshold" },
                values: new object[] { 1, 0.59999999999999998 });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProjections_UserId",
                schema: "ai_categorization",
                table: "CategoryProjections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEntries_ProcessedAt",
                schema: "ai_categorization",
                table: "OutboxEntries",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionCandidates_SuggestionId",
                schema: "ai_categorization",
                table: "SuggestionCandidates",
                column: "SuggestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_NoteId",
                schema: "ai_categorization",
                table: "Suggestions",
                column: "NoteId",
                unique: true,
                filter: "\"ResolvedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryProjections",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "OutboxEntries",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "Settings",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "SuggestionCandidates",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "Suggestions",
                schema: "ai_categorization");
        }
    }
}
