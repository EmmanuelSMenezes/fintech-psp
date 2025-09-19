using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FintechPSP.CompanyService.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullDateTimeConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();

            logger.LogWarning("🚨 Model binding falhou. Erros: {Errors}",
                System.Text.Json.JsonSerializer.Serialize(errors));

            return new BadRequestObjectResult(context.ModelState);
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "FintechPSP Company Service", 
        Version = "v1",
        Description = "API para gerenciamento de empresas clientes"
    });
    
    // Configuração para JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-should-be-at-least-256-bits";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FintechPSP";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FintechPSP";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminScope", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "admin"));
              
    options.AddPolicy("BankingScope", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("scope", "banking"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontends", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // BackofficeWeb
                "http://localhost:3001"   // InternetBankingWeb
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FintechPSP Company Service v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontends");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Root endpoint
app.MapGet("/", () => new
{
    service = "FintechPSP.CompanyService",
    version = "1.0.0",
    status = "healthy",
    timestamp = DateTime.UtcNow,
    endpoints = new[]
    {
        "GET /admin/companies - Lista empresas",
        "POST /admin/companies - Cria empresa",
        "GET /admin/companies/{id} - Obtém empresa",
        "PUT /admin/companies/{id} - Atualiza empresa",
        "PATCH /admin/companies/{id}/status - Atualiza status",
        "DELETE /admin/companies/{id} - Exclui empresa",
        "GET /admin/companies/{id}/representatives - Lista representantes",
        "POST /admin/companies/{id}/representatives - Cria representante",
        "GET /admin/companies/{id}/representatives/{repId} - Obtém representante",
        "PUT /admin/companies/{id}/representatives/{repId} - Atualiza representante",
        "DELETE /admin/companies/{id}/representatives/{repId} - Exclui representante"
    }
});

Console.WriteLine("🏢 FintechPSP Company Service iniciado!");
Console.WriteLine($"🌐 Swagger UI: {(app.Environment.IsDevelopment() ? "http://localhost:5004" : "N/A")}");

app.Run();
