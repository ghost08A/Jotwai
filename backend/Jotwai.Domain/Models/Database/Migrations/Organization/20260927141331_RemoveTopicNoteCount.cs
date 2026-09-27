using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.Organization
{
    /// <inheritdoc />
    public partial class RemoveTopicNoteCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteCount",
                schema: "organization",
                table: "Topics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoteCount",
                schema: "organization",
                table: "Topics",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
