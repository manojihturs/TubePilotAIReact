using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TubePilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ApiKeyEncrypted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DefaultModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SupportsVision = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsImageGeneration = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsThinking = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsJSONMode = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsFunctionCalling = table.Column<bool>(type: "boolean", nullable: false),
                    DailyLimit = table.Column<long>(type: "bigint", nullable: true),
                    MonthlyLimit = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoftDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Icon = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoftDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptedKey = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ContextWindow = table.Column<int>(type: "integer", nullable: true),
                    MaxInputTokens = table.Column<int>(type: "integer", nullable: true),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: true),
                    SupportsVision = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsAudio = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsVideo = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsTools = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsStreaming = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsReasoning = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsJSON = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsImageGeneration = table.Column<bool>(type: "boolean", nullable: false),
                    InputPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    OutputPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PromptText = table.Column<string>(type: "text", nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: true),
                    TopP = table.Column<double>(type: "double precision", nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: true),
                    Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prompts_PromptCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PromptCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    InputJson = table.Column<string>(type: "text", nullable: false),
                    ResultJson = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedBy = table.Column<string>(type: "text", nullable: false)
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
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ConfigJson = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
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
                    PromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    VariablesJson = table.Column<string>(type: "text", nullable: false),
                    RenderedPrompt = table.Column<string>(type: "text", nullable: false),
                    Response = table.Column<string>(type: "text", nullable: false),
                    InputTokens = table.Column<int>(type: "integer", nullable: true),
                    OutputTokens = table.Column<int>(type: "integer", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Latency = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedBy = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PromptVariables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Placeholder = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SoftDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "IX_PromptCategories_Name",
                table: "PromptCategories",
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
                name: "IX_Prompts_CategoryId",
                table: "Prompts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_Name",
                table: "Prompts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PromptVariables_PromptId_Name",
                table: "PromptVariables",
                columns: new[] { "PromptId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserApiKeys_UserId_ProviderName",
                table: "UserApiKeys",
                columns: new[] { "UserId", "ProviderName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowExecutions_WorkflowId",
                table: "WorkflowExecutions",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId",
                table: "WorkflowSteps",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptExecutions");

            migrationBuilder.DropTable(
                name: "PromptVariables");

            migrationBuilder.DropTable(
                name: "UserApiKeys");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkflowExecutions");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "AIModels");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "AIProviders");

            migrationBuilder.DropTable(
                name: "PromptCategories");
        }
    }
}
