using System.Threading;
using System.Threading.Tasks;
using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using FintechPSP.TransactionService.Models;
using FintechPSP.TransactionService.Services;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Infrastructure.EventStore;
using MediatR;

namespace FintechPSP.TransactionService.Handlers;

/// <summary>
/// Handler para gerar QR Code PIX estático
/// </summary>
public class GerarQrEstaticoHandler : IRequestHandler<GerarQrEstaticoCommand, QrCodeResponse>
{
    private readonly IEventStore _eventStore;
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<GerarQrEstaticoHandler> _logger;

    public GerarQrEstaticoHandler(
        IEventStore eventStore,
        IQrCodeService qrCodeService,
        ILogger<GerarQrEstaticoHandler> logger)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<QrCodeResponse> Handle(GerarQrEstaticoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processando geração de QR Code estático para externalId {ExternalId}", request.ExternalId);

        // Gerar ID da transação (usar como aggregate ID)
        var transactionId = Guid.NewGuid();

        // Verificar idempotência - se já existe uma transação com este externalId
        // Para simplificar, vamos usar o hash do externalId como aggregateId
        var aggregateId = Guid.Parse(request.ExternalId.PadRight(32, '0')[..32].Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-"));

        try
        {
            var existingEvents = await _eventStore.GetEventsAsync(aggregateId);
            if (existingEvents.Any())
            {
                var existingEvent = existingEvents.OfType<QrCodeGerado>().FirstOrDefault();
                if (existingEvent != null)
                {
                    _logger.LogInformation("QR Code estático já existe para externalId {ExternalId}, retornando existente", request.ExternalId);

                    return new QrCodeResponse
                    {
                        TransactionId = existingEvent.TransactionId,
                        QrcodePayload = existingEvent.QrcodePayload,
                        QrcodeImage = existingEvent.QrcodeImage,
                        Type = existingEvent.Type,
                        ExpiresAt = existingEvent.ExpiresAt,
                        PixKey = existingEvent.PixKey,
                        Amount = existingEvent.Amount,
                        Description = existingEvent.Description,
                        CreatedAt = existingEvent.OccurredAt
                    };
                }
            }
        }
        catch
        {
            // Se não conseguir buscar eventos existentes, continua com a criação
        }

        // Validar PIX Key
        if (!IsValidPixKey(request.PixKey))
        {
            throw new ArgumentException("PIX Key inválida");
        }



        // Gerar payload EMV para QR Code estático
        var qrcodePayload = _qrCodeService.GenerateStaticEmvPayload(
            request.PixKey, 
            request.BankCode, 
            request.Description);

        // Gerar imagem base64 do QR Code
        var qrcodeImage = _qrCodeService.GenerateQrCodeImage(qrcodePayload);

        // Criar agregado QR Code
        var qrCode = QrCode.CreateStatic(
            request.ExternalId,
            transactionId,
            request.PixKey,
            request.BankCode,
            qrcodePayload,
            qrcodeImage,
            request.Description);

        // Salvar eventos no event store
        await _eventStore.SaveEventsAsync<QrCode>(aggregateId, qrCode.UncommittedEvents, 0);

        _logger.LogInformation("QR Code estático gerado com sucesso para transactionId {TransactionId}", transactionId);

        return new QrCodeResponse
        {
            TransactionId = transactionId,
            QrcodePayload = qrcodePayload,
            QrcodeImage = qrcodeImage,
            Type = "static",
            ExpiresAt = null,
            PixKey = request.PixKey,
            Amount = null,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
    }

    private bool IsValidPixKey(string pixKey)
    {
        if (string.IsNullOrWhiteSpace(pixKey))
            return false;

        // Validação básica de PIX Key
        // CPF/CNPJ (apenas números)
        if (pixKey.All(char.IsDigit) && (pixKey.Length == 11 || pixKey.Length == 14))
            return true;

        // Email
        if (pixKey.Contains("@") && pixKey.Contains("."))
            return true;

        // Telefone (+5511999887766)
        if (pixKey.StartsWith("+55") && pixKey.Length >= 13)
            return true;

        // Chave aleatória (UUID)
        if (Guid.TryParse(pixKey, out _))
            return true;

        return false;
    }
}
