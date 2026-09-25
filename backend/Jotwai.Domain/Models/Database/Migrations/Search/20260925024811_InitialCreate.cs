using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.Search
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "search");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "SearchCategories",
                schema: "search",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "SearchDocuments",
                schema: "search",
                columns: table => new
                {
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TopicId = table.Column<Guid>(type: "uuid", nullable: true),
                    TopicName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocuments", x => x.NoteId);
                });

            migrationBuilder.CreateTable(
                name: "SearchTopics",
                schema: "search",
                columns: table => new
                {
                    TopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchTopics", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "SearchDocumentCategories",
                schema: "search",
                columns: table => new
                {
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchDocumentCategories", x => new { x.NoteId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_SearchDocumentCategories_SearchDocuments_NoteId",
                        column: x => x.NoteId,
                        principalSchema: "search",
                        principalTable: "SearchDocuments",
                        principalColumn: "NoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchCategories_UserId",
                schema: "search",
                table: "SearchCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocumentCategories_CategoryId",
                schema: "search",
                table: "SearchDocumentCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_Content",
                schema: "search",
                table: "SearchDocuments",
                column: "Content")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_TopicId",
                schema: "search",
                table: "SearchDocuments",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchDocuments_UserId",
                schema: "search",
                table: "SearchDocuments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchTopics_UserId",
                schema: "search",
                table: "SearchTopics",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchCategories",
                schema: "search");

            migrationBuilder.DropTable(
                name: "SearchDocumentCategories",
                schema: "search");

            migrationBuilder.DropTable(
                name: "SearchTopics",
                schema: "search");

            migrationBuilder.DropTable(
                name: "SearchDocuments",
                schema: "search");
        }
    }
}
