using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TubePilot.API.Models;
using TubePilot.Infrastructure;
using TubePilot.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Bind JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>();

// Ensure JWT secret is provided via configuration or environment/user-secrets
// Allow environment variable fallback if configuration binding did not pick it up (helps CLI runs)
var envSecret = Environment.GetEnvironmentVariable("JwtSettings__Secret");
if (string.IsNullOrWhiteSpace(jwtSettings?.Secret) || jwtSettings.Secret == "REPLACE_ME_FROM_ENV_OR_USER_SECRETS")
{
    if (!string.IsNullOrWhiteSpace(envSecret))
    {
        jwtSettings ??= new JwtSettings();
        jwtSettings.Secret = envSecret;
    }
}

if (string.IsNullOrWhiteSpace(jwtSettings?.Secret) || jwtSettings.Secret == "REPLACE_ME_FROM_ENV_OR_USER_SECRETS")
{
    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
    logger.LogError("JWT secret is not configured. Set JwtSettings:Secret via environment variable or user-secrets before starting the application.");
    throw new InvalidOperationException("JWT secret is not configured. Aborting startup.");
}

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Application services
builder.Services.AddApplication();

// Add infrastructure (DbContext + repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TubePilot.API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    });
});

var app = builder.Build();

// Ensure database seeded (development/demo)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<TubePilot.Infrastructure.Data.TubePilotDbContext>();
        await TubePilot.Infrastructure.Data.DataSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Global exception middleware
app.UseMiddleware<TubePilot.API.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
