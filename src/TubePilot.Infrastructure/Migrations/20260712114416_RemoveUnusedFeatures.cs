using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptExecutions");

            migrationBuilder.DropTable(
                name: "WorkflowExecutions");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "AIModels");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "AIProviders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKeyEncrypted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DailyLimit = table.Column<long>(type: "bigint", nullable: true),
                    DefaultModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MonthlyLimit = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ProviderType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SoftDelete = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsFunctionCalling = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsImageGeneration = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsJSONMode = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsThinking = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsVision = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContextWindow = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InputPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxInputTokens = table.Column<int>(type: "integer", nullable: true),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OutputPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    SupportsAudio = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsImageGeneration = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsJSON = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsReasoning = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsTools = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsVideo = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsVision = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "WorkflowExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    InputJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedBy = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowExecutions_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromptExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    PromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ExecutedBy = table.Column<string>(type: "text", nullable: false),
                    InputTokens = table.Column<int>(type: "integer", nullable: true),
                    Latency = table.Column<long>(type: "bigint", nullable: true),
                    OutputTokens = table.Column<int>(type: "integer", nullable: true),
                    RenderedPrompt = table.Column<string>(type: "text", nullable: false),
                    Response = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    VariablesJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptExecutions_AIModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AIModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromptExecutions_AIProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AIProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromptExecutions_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_ProviderId_Name",
                table: "AIModels",
                columns: new[] { "ProviderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIProviders_Name",
                table: "AIProviders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptExecutions_ModelId",
                table: "PromptExecutions",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptExecutions_PromptId",
                table: "PromptExecutions",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptExecutions_ProviderId",
                table: "PromptExecutions",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_WorkflowId",
                table: "WorkflowExecutions",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId",
                table: "WorkflowSteps",
                column: "WorkflowId");
        }
    }
}
