using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TubePilotAI.Application.Features.PromptTemplates;
using TubePilotAI.Application.Features.AIProviderSettings;
using TubePilotAI.Application.Features.ContentGeneration;
using TubePilotAI.Application.Features.Projects;
using TubePilotAI.Infrastructure.DependencyInjection;
using TubePilotAI.Infrastructure.Persistence.Context;
using TubePilotAI.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// CORS - allow Vite dev server during development
var devClientOrigins = new[] { "https://localhost:49153", "http://localhost:49153" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(devClientOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Database configuration - SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=TubePilotAI;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<TubePilotDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IPromptTemplateService, PromptTemplateService>();
builder.Services.AddScoped<IAIProviderSettingService, AIProviderSettingService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IContentGenerationService, ContentGenerationService>();
builder.Services.AddScoped<ProjectExportService, ProjectExportService>();
builder.Services.AddAIProviders();

// Level 0: register project generator service and image service implementations
builder.Services.AddScoped<TubePilotAI.Application.Interfaces.IProjectGeneratorService, TubePilotAI.Application.Services.ProjectGeneratorService>();
builder.Services.AddScoped<TubePilotAI.Application.Interfaces.IImageService, TubePilotAI.Infrastructure.Image.LocalImageService>();

// Background hosted service placeholder (Level 0)
builder.Services.AddHostedService<TubePilotAIReact.Server.Services.BackgroundGenerationHostedService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("DevCors");

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
