using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.Notes
{
    /// <inheritdoc />
    public partial class AddUniqueActiveNotePerTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_TopicId",
                schema: "notes",
                table: "Notes");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TopicId",
                schema: "notes",
                table: "Notes",
                column: "TopicId",
                unique: true,
                filter: "\"TopicId\" IS NOT NULL AND \"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_TopicId",
                schema: "notes",
                table: "Notes");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TopicId",
                schema: "notes",
                table: "Notes",
                column: "TopicId");
        }
    }
}
