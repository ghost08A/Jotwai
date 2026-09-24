using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.Notes
{
    /// <inheritdoc />
    public partial class AddCategoryAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnownCategories",
                schema: "notes",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "NoteCategories",
                schema: "notes",
                columns: table => new
                {
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteCategories", x => new { x.NoteId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_NoteCategories_Notes_NoteId",
                        column: x => x.NoteId,
                        principalSchema: "notes",
                        principalTable: "Notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoteCategories_CategoryId",
                schema: "notes",
                table: "NoteCategories",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownCategories",
                schema: "notes");

            migrationBuilder.DropTable(
                name: "NoteCategories",
                schema: "notes");
        }
    }
}
