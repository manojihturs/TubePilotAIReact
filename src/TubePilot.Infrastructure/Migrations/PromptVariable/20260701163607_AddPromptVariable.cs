using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations.PromptVariable
{
    /// <inheritdoc />
    public partial class AddPromptVariable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prompt_PromptCategories_CategoryId",
                table: "Prompt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prompt",
                table: "Prompt");

            migrationBuilder.RenameTable(
                name: "Prompt",
                newName: "Prompts");

            migrationBuilder.RenameIndex(
                name: "IX_Prompt_Name",
                table: "Prompts",
                newName: "IX_Prompts_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Prompt_CategoryId",
                table: "Prompts",
                newName: "IX_Prompts_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prompts",
                table: "Prompts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PromptVariables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptVariables_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptVariables_PromptId_Name",
                table: "PromptVariables",
                columns: new[] { "PromptId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prompts_PromptCategories_CategoryId",
                table: "Prompts",
                column: "CategoryId",
                principalTable: "PromptCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prompts_PromptCategories_CategoryId",
                table: "Prompts");

            migrationBuilder.DropTable(
                name: "PromptVariables");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prompts",
                table: "Prompts");

            migrationBuilder.RenameTable(
                name: "Prompts",
                newName: "Prompt");

            migrationBuilder.RenameIndex(
                name: "IX_Prompts_Name",
                table: "Prompt",
                newName: "IX_Prompt_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Prompts_CategoryId",
                table: "Prompt",
                newName: "IX_Prompt_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prompt",
                table: "Prompt",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prompt_PromptCategories_CategoryId",
                table: "Prompt",
                column: "CategoryId",
                principalTable: "PromptCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
