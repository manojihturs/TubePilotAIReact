using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations
{
    public partial class AddAIModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContextWindow = table.Column<int>(type: "int", nullable: true),
                    MaxInputTokens = table.Column<int>(type: "int", nullable: true),
                    MaxOutputTokens = table.Column<int>(type: "int", nullable: true),
                    SupportsVision = table.Column<bool>(type: "bit", nullable: false),
                    SupportsAudio = table.Column<bool>(type: "bit", nullable: false),
                    SupportsVideo = table.Column<bool>(type: "bit", nullable: false),
                    SupportsTools = table.Column<bool>(type: "bit", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "bit", nullable: false),
                    SupportsReasoning = table.Column<bool>(type: "bit", nullable: false),
                    SupportsJSON = table.Column<bool>(type: "bit", nullable: false),
                    SupportsImageGeneration = table.Column<bool>(type: "bit", nullable: false),
                    InputPrice = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    OutputPrice = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIModels_AIProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AIProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_ProviderId_Name",
                table: "AIModels",
                columns: new[] { "ProviderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_ProviderId",
                table: "AIModels",
                column: "ProviderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIModels");
        }
    }
}
