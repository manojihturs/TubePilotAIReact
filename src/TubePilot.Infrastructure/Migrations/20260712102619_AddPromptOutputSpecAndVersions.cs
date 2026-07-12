using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptOutputSpecAndVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutputSpecJson",
                table: "Prompts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PromptVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    TemplateText = table.Column<string>(type: "text", nullable: false),
                    OutputSpecJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptVersions_PromptId_VersionNumber",
                table: "PromptVersions",
                columns: new[] { "PromptId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptVersions");

            migrationBuilder.DropColumn(
                name: "OutputSpecJson",
                table: "Prompts");
        }
    }
}
