using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenerationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutputSpecJson",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DataRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectOutputId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowIndex = table.Column<int>(type: "integer", nullable: false),
                    DataJson = table.Column<string>(type: "text", nullable: false),
                    ImageStatus = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedImagePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenerationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProviderUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModelUsed = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InputTokens = table.Column<int>(type: "integer", nullable: false),
                    OutputTokens = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    FolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectOutputs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataRows_ProjectOutputId",
                table: "DataRows",
                column: "ProjectOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationRecords_ProjectId",
                table: "GenerationRecords",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectOutputs_ProjectId",
                table: "ProjectOutputs",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataRows");

            migrationBuilder.DropTable(
                name: "GenerationRecords");

            migrationBuilder.DropTable(
                name: "ProjectOutputs");

            migrationBuilder.DropColumn(
                name: "OutputSpecJson",
                table: "Projects");
        }
    }
}
