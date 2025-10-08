using System.Text;
using FintechPSP.BalanceService.Consumers;
using FintechPSP.BalanceService.Handlers;
using FintechPSP.BalanceService.Repositories;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.Shared.Infrastructure.EventStore;
using Marten;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

// MediatR para CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(provider =>
    new PostgreSqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

// Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionHistoryRepository, TransactionHistoryRepository>();

// Marten para Event Store
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("DefaultConnection")!);
    options.DatabaseSchemaName = "balance_events";
});

// Event Store
builder.Services.AddScoped<IEventStore, MartenEventStore>();

// MassTransit para mensageria
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers via assembly scanning
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
        Console.WriteLine($"🔍 DEBUG: RabbitMQ URI: {rabbitMqUri}");
        cfg.Host(rabbitMqUri);

        // Deixar o MassTransit configurar automaticamente TODOS os endpoints
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
    options.AddPolicy("AdminScope", policy =>
        policy.RequireClaim("scope", "admin"));

    options.AddPolicy("BankingScope", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "banking") ||
            context.User.HasClaim("scope", "admin")));

    options.AddPolicy("ClientScope", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "client") ||
            context.User.HasClaim("scope", "admin")));
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FintechPSP BalanceService API",
        Version = "v1",
        Description = "API para consulta de saldos e extratos"
    });

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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Log dos consumers registrados
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🔍 DEBUG: Verificando consumers registrados...");
logger.LogInformation("🔍 DEBUG: PixConfirmadoConsumer registrado: {Type}", typeof(PixConfirmadoConsumer).FullName);
logger.LogInformation("🔍 DEBUG: PixConfirmado event registrado: {Type}", typeof(PixConfirmado).FullName);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BalanceService API V1");
        c.RoutePrefix = string.Empty;
    });
}

// Use CORS
app.UseCors();

// Remover completamente HTTPS redirection e authentication para debug
// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();

app.Run();
