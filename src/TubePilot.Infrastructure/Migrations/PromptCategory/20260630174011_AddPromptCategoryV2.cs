using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations.PromptCategory
{
    /// <inheritdoc />
    public partial class AddPromptCategoryV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PromptCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PromptCategories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptCategories_Name",
                table: "PromptCategories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromptCategories_Name",
                table: "PromptCategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PromptCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PromptCategories");
        }
    }
}
