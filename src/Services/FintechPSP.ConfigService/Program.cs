using System.Text;
using FintechPSP.ConfigService.Repositories;
using FintechPSP.Shared.Infrastructure.Database;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
    new PostgreSqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost:5433;Database=fintech_psp;Username=postgres;Password=postgres"));

// Repositories
builder.Services.AddScoped<IPriorizacaoRepository, PriorizacaoRepository>();
builder.Services.AddScoped<IBancoPersonalizadoRepository, BancoPersonalizadoRepository>();
builder.Services.AddScoped<IBankingConfigRepository, BankingConfigRepository>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
        cfg.Host(rabbitMqUri);
        cfg.ConfigureEndpoints(context);
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BankingScope", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "banking", "admin"));

    options.AddPolicy("AdminScope", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "admin"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FintechPSP ConfigService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfigService API V1"); c.RoutePrefix = string.Empty; });
}

app.UseHttpsRedirection();

// Use CORS (deve vir antes de Authentication/Authorization)
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
