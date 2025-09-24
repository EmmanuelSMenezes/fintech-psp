using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configuração JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Mortadela",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Mortadela",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                "mortadela-super-secret-key-that-should-be-at-least-256-bits"))
        };
    });

// ✅ CORS global
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()      // 🔑 para React local
            .AllowAnyHeader()
            .AllowAnyMethod();     // Inclui OPTIONS, GET, POST etc.
    });
});

// ✅ Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

// Health check
builder.Services.AddHealthChecks();

var app = builder.Build();

// ✅ ORDEM do pipeline é importante
app.UseRouting();

// CORS precisa vir ANTES de Auth/Ocelot
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// ✅ Ocelot por último
await app.UseOcelot();

app.Run();
