using System.Text;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.Shared.Infrastructure.Messaging;
using FintechPSP.UserService.Repositories;
using FintechPSP.UserService.Services;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("ControllerDiscovery");
        logger.LogInformation("Descobrindo controllers...");

        // Adicionar explicitamente o assembly atual
        var currentAssembly = typeof(Program).Assembly;
        manager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(currentAssembly));
        logger.LogInformation("Adicionado assembly: {AssemblyName}", currentAssembly.FullName);

        foreach (var part in manager.ApplicationParts)
        {
            logger.LogInformation("ApplicationPart: {PartName}", part.Name);
        }
    });

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
        ?? "Host=localhost;Database=fintech_psp_users;Username=postgres;Password=postgres"));

// Repositories
builder.Services.AddScoped<IAcessoRepository, AcessoRepository>();
builder.Services.AddScoped<ISystemUserRepository>(provider =>
    new SystemUserRepository(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Services
builder.Services.AddSingleton<ICredentialsProtector, AesCredentialsProtector>();

// Marten para Event Store
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("DefaultConnection")!);
    options.DatabaseSchemaName = "user_events";
});

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

// Event Publisher (RabbitMQ/MassTransit)
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

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

        // Adicionar logs de debug para autenticação
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                logger.LogInformation("JWT Token validated successfully. Claims: {Claims}", string.Join(", ", claims ?? new List<string>()));
                return Task.CompletedTask;
            }
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminScope", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "admin"));

    options.AddPolicy("BankingScope", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "banking", "admin"));
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FintechPSP UserService API",
        Version = "v1",
        Description = "API para gerenciamento de usuários"
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = string.Empty;
    });
}

// app.UseHttpsRedirection(); // Desabilitado para Docker

// Use CORS (deve vir antes de Authentication/Authorization)
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
