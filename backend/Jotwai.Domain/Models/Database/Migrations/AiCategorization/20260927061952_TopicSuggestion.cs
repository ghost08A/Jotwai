using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.AiCategorization
{
    /// <inheritdoc />
    public partial class TopicSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TopicSuggestionConfidenceThreshold",
                schema: "ai_categorization",
                table: "Settings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "TopicProjections",
                schema: "ai_categorization",
                columns: table => new
                {
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicProjections", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "TopicSuggestionOutboxEntries",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastError = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSuggestionOutboxEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TopicSuggestions",
                schema: "ai_categorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    TargetTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicSuggestions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "ai_categorization",
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "TopicSuggestionConfidenceThreshold",
                value: 0.84999999999999998);

            migrationBuilder.CreateIndex(
                name: "IX_TopicProjections_UserId",
                schema: "ai_categorization",
                table: "TopicProjections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicSuggestionOutboxEntries_ProcessedAt",
                schema: "ai_categorization",
                table: "TopicSuggestionOutboxEntries",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TopicSuggestions_TopicId",
                schema: "ai_categorization",
                table: "TopicSuggestions",
                column: "TopicId",
                unique: true,
                filter: "\"ResolvedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopicProjections",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "TopicSuggestionOutboxEntries",
                schema: "ai_categorization");

            migrationBuilder.DropTable(
                name: "TopicSuggestions",
                schema: "ai_categorization");

            migrationBuilder.DropColumn(
                name: "TopicSuggestionConfidenceThreshold",
                schema: "ai_categorization",
                table: "Settings");
        }
    }
}
