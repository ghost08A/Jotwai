using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Service.Migrations.Notes
{
    /// <inheritdoc />
    public partial class AddTopicAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TopicId",
                schema: "notes",
                table: "Notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KnownTopics",
                schema: "notes",
                columns: table => new
                {
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownTopics", x => x.TopicId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TopicId",
                schema: "notes",
                table: "Notes",
                column: "TopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownTopics",
                schema: "notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_TopicId",
                schema: "notes",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "TopicId",
                schema: "notes",
                table: "Notes");
        }
    }
}
