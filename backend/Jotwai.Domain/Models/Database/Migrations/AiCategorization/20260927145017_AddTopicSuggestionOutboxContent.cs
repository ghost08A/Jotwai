using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.AiCategorization
{
    /// <inheritdoc />
    public partial class AddTopicSuggestionOutboxContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                schema: "ai_categorization",
                table: "TopicSuggestionOutboxEntries",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                schema: "ai_categorization",
                table: "TopicSuggestionOutboxEntries");
        }
    }
}
