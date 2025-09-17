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
var secretKey = "fintech_psp_super_secret_key_2024_very_long_key";
var key = Encoding.ASCII.GetBytes(secretKey);

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
            ValidIssuer = "FintechPSP",
            ValidateAudience = true,
            ValidAudience = "FintechPSP.API",
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

// Use Ocelot middleware
await app.UseOcelot();

app.Run();
