using System.Text;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.Shared.Infrastructure.EventStore;
using FintechPSP.Shared.Infrastructure.Messaging;
using FintechPSP.TransactionService.Repositories;
using FintechPSP.TransactionService.Services;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FintechPSP TransactionService", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Database
builder.Services.AddSingleton<IDbConnectionFactory, PostgreSqlConnectionFactory>();

// Repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Services
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

// Sicoob Integration (sandbox)
try
{
    var sicoobSettings = builder.Configuration.GetSection("SicoobSettings");
    if (sicoobSettings.Exists() && !string.IsNullOrEmpty(sicoobSettings["ClientId"]))
    {
        // Registrar configurações Sicoob
        builder.Services.Configure<FintechPSP.IntegrationService.Models.Sicoob.SicoobSettings>(sicoobSettings);

        // HTTP Client para Sicoob Produção com mTLS
        builder.Services.AddHttpClient("SicoobApi", client =>
        {
            client.BaseAddress = new Uri(sicoobSettings["BaseUrl"] ?? "https://api.sicoob.com.br");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();

            // Configurar mTLS se certificado estiver disponível
            var certPath = sicoobSettings["CertificatePath"];
            var certPassword = sicoobSettings["CertificatePassword"];

            if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
            {
                try
                {
                    var certBytes = File.ReadAllBytes(certPath);
                    var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certBytes, certPassword);
                    handler.ClientCertificates.Add(certificate);
                    Console.WriteLine($"✅ Certificado mTLS carregado: {certificate.Subject}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao carregar certificado: {ex.Message}");
                }
            }

            return handler;
        });

        // HTTP Client para autenticação Sicoob (também com mTLS)
        builder.Services.AddHttpClient("SicoobAuth", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();

            // Configurar mTLS se certificado estiver disponível
            var certPath = sicoobSettings["CertificatePath"];
            var certPassword = sicoobSettings["CertificatePassword"];

            if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
            {
                try
                {
                    var certBytes = File.ReadAllBytes(certPath);
                    var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certBytes, certPassword);
                    handler.ClientCertificates.Add(certificate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao carregar certificado para auth: {ex.Message}");
                }
            }

            return handler;
        });

        // Registrar serviços Sicoob
        builder.Services.AddScoped<FintechPSP.IntegrationService.Services.Sicoob.ISicoobAuthService,
                                   FintechPSP.IntegrationService.Services.Sicoob.SicoobAuthService>();
        builder.Services.AddScoped<FintechPSP.IntegrationService.Services.Sicoob.Pix.IPixPagamentosService,
                                   FintechPSP.IntegrationService.Services.Sicoob.Pix.PixPagamentosService>();
        builder.Services.AddScoped<FintechPSP.IntegrationService.Services.Sicoob.Pix.IPixRecebimentosService,
                                   FintechPSP.IntegrationService.Services.Sicoob.Pix.PixRecebimentosService>();
        builder.Services.AddScoped<FintechPSP.IntegrationService.Services.Sicoob.Pix.IPixQrCodeService,
                                   FintechPSP.IntegrationService.Services.Sicoob.Pix.PixQrCodeService>();
        builder.Services.AddScoped<FintechPSP.IntegrationService.Services.Sicoob.SPB.ISPBService,
                                   FintechPSP.IntegrationService.Services.Sicoob.SPB.SPBService>();
        builder.Services.AddScoped<FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration.ITransactionIntegrationService,
                                   FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration.TransactionIntegrationService>();

        Console.WriteLine("✅ Sicoob Production integration configured successfully");
    }
    else
    {
        Console.WriteLine("⚠️ Sicoob integration not configured - missing ClientId");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Warning: Sicoob integration configuration failed: {ex.Message}");
}

// Marten para Event Store
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("DefaultConnection")!);
    options.DatabaseSchemaName = "transaction_events";
});

// Event Store
builder.Services.AddScoped<IEventStore, MartenEventStore>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = builder.Configuration.GetConnectionString("RabbitMQ");
        if (!string.IsNullOrEmpty(rabbitMqUri))
        {
            cfg.Host(rabbitMqUri);
        }
        else
        {
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }

        cfg.ConfigureEndpoints(context);
    });
});

// Event Publisher
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
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminScope", policy =>
        policy.RequireClaim("scope", "admin"));

    options.AddPolicy("ClientScope", policy =>
        policy.RequireClaim("scope", "client", "admin"));

    options.AddPolicy("BankingScope", policy =>
        policy.RequireClaim("scope", "banking", "admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
