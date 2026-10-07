using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.Search
{
    /// <inheritdoc />
    public partial class AddSearchDocumentNameAndParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "search",
                table: "SearchDocuments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                schema: "search",
                table: "SearchDocuments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "search",
                table: "SearchDocuments");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "search",
                table: "SearchDocuments");
        }
    }
}
