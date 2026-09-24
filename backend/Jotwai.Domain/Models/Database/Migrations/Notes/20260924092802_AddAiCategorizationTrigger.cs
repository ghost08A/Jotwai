using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jotwai.Domain.Models.Database.Migrations.Notes
{
    /// <inheritdoc />
    public partial class AddAiCategorizationTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AiCategorizationTriggeredAt",
                schema: "notes",
                table: "Notes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiCategorizationTriggeredAt",
                schema: "notes",
                table: "Notes");
        }
    }
}
