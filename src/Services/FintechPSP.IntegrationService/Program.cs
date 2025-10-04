using System.Text;
using FintechPSP.IntegrationService.Services;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Services.Sicoob;
using FintechPSP.IntegrationService.Services.Sicoob.Pix;
using FintechPSP.IntegrationService.Services.Sicoob.ContaCorrente;
using FintechPSP.IntegrationService.Services.Sicoob.SPB;
using FintechPSP.IntegrationService.Helpers;
using FintechPSP.IntegrationService.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurações do Sicoob
builder.Services.Configure<SicoobSettings>(
    builder.Configuration.GetSection("SicoobSettings"));

var sicoobSettings = builder.Configuration
    .GetSection("SicoobSettings")
    .Get<SicoobSettings>();

// HTTP Clients configuration
builder.Services.AddHttpClient();

// Configuração específica para Sicoob com mTLS
if (sicoobSettings != null && !string.IsNullOrEmpty(sicoobSettings.CertificatePath))
{
    try
    {
        var certificate = CertificateHelper.LoadCertificate(
            sicoobSettings.CertificatePath,
            sicoobSettings.CertificatePassword);

        // HttpClient para autenticação Sicoob
        builder.Services.AddHttpClient("SicoobAuth", client =>
        {
            client.BaseAddress = new Uri(sicoobSettings.AuthUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(() => CertificateHelper.CreateHttpClientHandler(certificate));

        // HttpClient para APIs Sicoob
        builder.Services.AddHttpClient("SicoobAPI", client =>
        {
            client.BaseAddress = new Uri(sicoobSettings.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(() => CertificateHelper.CreateHttpClientHandler(certificate));

        Console.WriteLine("✅ Certificado carregado com sucesso!");
        CertificateHelper.PrintCertificateInfo(certificate);
        Console.WriteLine("   ✅ HttpClient Auth configurado com mTLS");
        Console.WriteLine("   ✅ HttpClient API configurado com mTLS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao configurar certificados Sicoob: {ex.Message}");
        // Em desenvolvimento, continua sem certificado
        // Em produção, considere falhar aqui
    }
}

// Routing and Account Services
builder.Services.AddScoped<IRoutingService, RoutingService>();
builder.Services.AddScoped<IAccountDataService, AccountDataService>();
builder.Services.AddScoped<IPriorityConfigService, PriorityConfigService>();

// Sicoob Services
builder.Services.AddScoped<ISicoobAuthService, SicoobAuthService>();
builder.Services.AddScoped<IPixPagamentosService, PixPagamentosService>();
builder.Services.AddScoped<IPixRecebimentosService, PixRecebimentosService>();
builder.Services.AddScoped<IContaCorrenteService, ContaCorrenteService>();
builder.Services.AddScoped<ISPBService, SPBService>();

// MassTransit para consumir eventos de transação
builder.Services.AddMassTransit(x =>
{
    // Registrar consumers
    x.AddConsumer<PixTransactionConsumer>();

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

    options.AddPolicy("ClientScope", policy =>
        policy.RequireClaim("scope", "client", "admin"));

    options.AddPolicy("BankingScope", policy =>
        policy.RequireClaim("scope", "banking", "admin"));
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FintechPSP IntegrationService API",
        Version = "v1",
        Description = "API para integrações com bancos e provedores"
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IntegrationService API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Testa a autenticação ao iniciar
try
{
    using var scope = app.Services.CreateScope();
    var authService = scope.ServiceProvider.GetRequiredService<ISicoobAuthService>();

    Console.WriteLine("\n🔐 Testando autenticação OAuth 2.0...");
    var token = await authService.GetAccessTokenAsync();
    Console.WriteLine($"✅ Token obtido com sucesso!");
    Console.WriteLine($"   Token: {token[..20]}...");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Aviso: Não foi possível obter token na inicialização: {ex.Message}");
    Console.WriteLine("   Verifique se o Client ID está configurado corretamente.");
}

Console.WriteLine("\n🚀 API iniciada com sucesso!");

app.Run();
