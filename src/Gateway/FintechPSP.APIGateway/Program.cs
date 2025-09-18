using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration
var ocelotFile = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker"
    ? "ocelot.docker.json"
    : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

// JWT Authentication
var secretKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-should-be-at-least-256-bits";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FintechPSP",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FintechPSP",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Ocelot services
builder.Services.AddOcelot();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");
app.UseAuthentication();

// Add health check endpoint
app.MapGet("/health", () => new {
    status = "healthy",
    service = "APIGateway",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
});

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
