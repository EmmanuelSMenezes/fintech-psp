using System.Security.Cryptography.X509Certificates;
using SicoobIntegration.API.Helpers;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Services;
using SicoobIntegration.API.Services.CobrancaBancaria;
using SicoobIntegration.API.Services.ContaCorrente;
using SicoobIntegration.API.Services.Pagamentos;
using SicoobIntegration.API.Services.Pix;
using SicoobIntegration.API.Services.SPB;

var builder = WebApplication.CreateBuilder(args);

// Configurações do Sicoob
builder.Services.Configure<SicoobSettings>(
    builder.Configuration.GetSection("SicoobSettings"));

var sicoobSettings = builder.Configuration
    .GetSection("SicoobSettings")
    .Get<SicoobSettings>();

if (sicoobSettings == null)
{
    throw new InvalidOperationException("SicoobSettings não configurado no appsettings.json");
}

// Carrega o certificado digital usando o helper
Console.WriteLine("\n🔐 Carregando certificado digital...");
X509Certificate2? certificate = null;
try
{
    var certPath = Path.GetFullPath(
        Path.Combine(
            builder.Environment.ContentRootPath,
            sicoobSettings.CertificatePath));

    certificate = CertificateHelper.LoadCertificate(certPath, sicoobSettings.CertificatePassword);
    builder.Services.AddSingleton(certificate);

    Console.WriteLine($"✅ Certificado carregado com sucesso!");
    CertificateHelper.PrintCertificateInfo(certificate);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro ao carregar certificado: {ex.Message}");
    Console.WriteLine($"   Verifique se o arquivo existe e a senha está correta.");
    throw;
}

// Configura HttpClient para autenticação (com mTLS)
Console.WriteLine("\n🌐 Configurando HttpClients com mTLS...");
builder.Services.AddHttpClient("SicoobAuth", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    if (certificate == null)
    {
        throw new InvalidOperationException("Certificado não foi carregado!");
    }

    var handler = CertificateHelper.CreateHttpClientHandler(certificate, validateServerCertificate: false);
    Console.WriteLine($"   ✅ HttpClient Auth configurado com mTLS");
    return handler;
});

// Configura HttpClient para APIs (com mTLS)
builder.Services.AddHttpClient("SicoobApi", client =>
{
    client.BaseAddress = new Uri(sicoobSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    if (certificate == null)
    {
        throw new InvalidOperationException("Certificado não foi carregado!");
    }

    var handler = CertificateHelper.CreateHttpClientHandler(certificate, validateServerCertificate: false);
    Console.WriteLine($"   ✅ HttpClient API configurado com mTLS");
    return handler;
});

// Registra serviços
builder.Services.AddSingleton<ISicoobAuthService, SicoobAuthService>();
builder.Services.AddScoped<ICobrancaBancariaService, CobrancaBancariaService>();
builder.Services.AddScoped<IPagamentosService, PagamentosService>();
builder.Services.AddScoped<IContaCorrenteService, ContaCorrenteService>();
builder.Services.AddScoped<IPixRecebimentosService, PixRecebimentosService>();
builder.Services.AddScoped<IPixPagamentosService, PixPagamentosService>();
builder.Services.AddScoped<ISPBService, SPBService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Sicoob Integration API",
        Version = "v1",
        Description = "API de integração com todas as APIs do Sicoob usando OAuth 2.0 e mTLS",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "OWAYPAY SOLUCOES DE PAGAMENTOS LTDA"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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
