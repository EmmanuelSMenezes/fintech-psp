using System.Text;
using FintechPSP.BalanceService.Consumers;
using FintechPSP.BalanceService.Handlers;
using FintechPSP.BalanceService.Repositories;
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
    // Registrar consumers
    x.AddConsumer<QrCodeEventHandler>();
    x.AddConsumer<ContaBancariaEventConsumer>();
    x.AddConsumer<PriorizacaoEventConsumer>();

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

app.UseHttpsRedirection();

// Use CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
