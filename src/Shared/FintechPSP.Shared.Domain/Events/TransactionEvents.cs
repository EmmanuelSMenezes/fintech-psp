using System;
using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Common;
using FintechPSP.Shared.Domain.Enums;
using FintechPSP.Shared.Domain.ValueObjects;

namespace FintechPSP.Shared.Domain.Events;

/// <summary>
/// Evento disparado quando uma transação PIX é iniciada
/// </summary>
public class PixIniciado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }
    
    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }

    public PixIniciado() { }

    public PixIniciado(Guid transactionId, string externalId, decimal amount, string pixKey, 
        string bankCode, string description, string? webhookUrl = null, string? endToEndId = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        PixKey = pixKey;
        BankCode = bankCode;
        Description = description;
        WebhookUrl = webhookUrl;
        EndToEndId = endToEndId;
    }
}

/// <summary>
/// Evento disparado quando uma transação PIX é confirmada
/// </summary>
public class PixConfirmado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    [JsonPropertyName("endToEndId")]
    public string EndToEndId { get; set; } = string.Empty;

    [JsonPropertyName("txId")]
    public string? TxId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("payerDocument")]
    public string? PayerDocument { get; set; }

    [JsonPropertyName("payerName")]
    public string? PayerName { get; set; }

    [JsonPropertyName("confirmedAt")]
    public DateTime ConfirmedAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public PixConfirmado() { }

    public PixConfirmado(Guid transactionId, string endToEndId, decimal amount, string? txId = null)
    {
        TransactionId = transactionId;
        EndToEndId = endToEndId;
        Amount = amount;
        TxId = txId;
        ConfirmedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Evento disparado quando uma transação TED é iniciada
/// </summary>
public class TedIniciado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("accountBranch")]
    public string AccountBranch { get; set; } = string.Empty;
    
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("taxId")]
    public string TaxId { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public TedIniciado() { }

    public TedIniciado(Guid transactionId, string externalId, decimal amount, string bankCode,
        string accountBranch, string accountNumber, string taxId, string? name = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        BankCode = bankCode;
        AccountBranch = accountBranch;
        AccountNumber = accountNumber;
        TaxId = taxId;
        Name = name;
    }
}

/// <summary>
/// Evento disparado quando um boleto é emitido
/// </summary>
public class BoletoEmitido : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }
    
    [JsonPropertyName("payerTaxId")]
    public string PayerTaxId { get; set; } = string.Empty;
    
    [JsonPropertyName("payerName")]
    public string PayerName { get; set; } = string.Empty;
    
    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;
    
    [JsonPropertyName("boletoId")]
    public string? BoletoId { get; set; }

    public BoletoEmitido() { }

    public BoletoEmitido(Guid transactionId, string externalId, decimal amount, DateTime dueDate,
        string payerTaxId, string payerName, string instructions, string? boletoId = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        DueDate = dueDate;
        PayerTaxId = payerTaxId;
        PayerName = payerName;
        Instructions = instructions;
        BoletoId = boletoId;
    }
}

/// <summary>
/// Evento disparado quando uma transação cripto é iniciada
/// </summary>
public class CriptoIniciado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("cryptoType")]
    public string CryptoType { get; set; } = string.Empty;
    
    [JsonPropertyName("walletAddress")]
    public string WalletAddress { get; set; } = string.Empty;
    
    [JsonPropertyName("fiatCurrency")]
    public string FiatCurrency { get; set; } = "BRL";

    public CriptoIniciado() { }

    public CriptoIniciado(Guid transactionId, string externalId, decimal amount, 
        string cryptoType, string walletAddress, string fiatCurrency = "BRL")
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        CryptoType = cryptoType;
        WalletAddress = walletAddress;
        FiatCurrency = fiatCurrency;
    }
}

/// <summary>
/// Evento disparado quando o status de uma transação é alterado
/// </summary>
public class StatusTransacaoAlterado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("previousStatus")]
    public TransactionStatus PreviousStatus { get; set; }
    
    [JsonPropertyName("newStatus")]
    public TransactionStatus NewStatus { get; set; }
    
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    public StatusTransacaoAlterado() { }

    public StatusTransacaoAlterado(Guid transactionId, TransactionStatus previousStatus, 
        TransactionStatus newStatus, string? reason = null)
    {
        TransactionId = transactionId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}

/// <summary>
/// Evento disparado quando uma conta é criada
/// </summary>
public class ContaCriada : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("initialBalance")]
    public decimal InitialBalance { get; set; }

    public ContaCriada() { }

    public ContaCriada(Guid clientId, string accountId, decimal initialBalance)
    {
        ClientId = clientId;
        AccountId = accountId;
        InitialBalance = initialBalance;
    }
}

/// <summary>
/// Evento disparado quando uma conta bancária é criada
/// </summary>
public class ContaBancariaCriada : DomainEvent
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("credentialsTokenId")]
    public string CredentialsTokenId { get; set; } = string.Empty;

    public ContaBancariaCriada() { }

    public ContaBancariaCriada(Guid contaId, Guid clienteId, string bankCode, string accountNumber,
        string description, string credentialsTokenId)
    {
        ContaId = contaId;
        ClienteId = clienteId;
        BankCode = bankCode;
        AccountNumber = accountNumber;
        Description = description;
        CredentialsTokenId = credentialsTokenId;
    }
}

/// <summary>
/// Evento disparado quando uma conta bancária é atualizada
/// </summary>
public class ContaBancariaAtualizada : DomainEvent
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("credentialsTokenId")]
    public string? CredentialsTokenId { get; set; }

    public ContaBancariaAtualizada() { }

    public ContaBancariaAtualizada(Guid contaId, Guid clienteId, string? description = null,
        string? credentialsTokenId = null)
    {
        ContaId = contaId;
        ClienteId = clienteId;
        Description = description;
        CredentialsTokenId = credentialsTokenId;
    }
}

/// <summary>
/// Evento disparado quando uma conta bancária é removida
/// </summary>
public class ContaBancariaRemovida : DomainEvent
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public ContaBancariaRemovida() { }

    public ContaBancariaRemovida(Guid contaId, Guid clienteId, string bankCode)
    {
        ContaId = contaId;
        ClienteId = clienteId;
        BankCode = bankCode;
    }
}

/// <summary>
/// Evento disparado quando saldo é creditado
/// </summary>
public class SaldoCreditado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("oldBalance")]
    public decimal OldBalance { get; set; }

    [JsonPropertyName("newBalance")]
    public decimal NewBalance { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoCreditado() { }

    public SaldoCreditado(Guid clientId, string accountId, decimal amount,
        decimal oldBalance, decimal newBalance, string description, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        Description = description;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando saldo é debitado
/// </summary>
public class SaldoDebitado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("oldBalance")]
    public decimal OldBalance { get; set; }

    [JsonPropertyName("newBalance")]
    public decimal NewBalance { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoDebitado() { }

    public SaldoDebitado(Guid clientId, string accountId, decimal amount,
        decimal oldBalance, decimal newBalance, string description, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        Description = description;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando saldo é bloqueado
/// </summary>
public class SaldoBloqueado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoBloqueado() { }

    public SaldoBloqueado(Guid clientId, string accountId, decimal amount, string reason, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        Reason = reason;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando saldo é desbloqueado
/// </summary>
public class SaldoDesbloqueado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoDesbloqueado() { }

    public SaldoDesbloqueado(Guid clientId, string accountId, decimal amount, string reason, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        Reason = reason;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando um webhook é criado
/// </summary>
public class WebhookCriado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<string> Events { get; set; } = new();

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public WebhookCriado() { }

    public WebhookCriado(Guid webhookId, Guid clientId, string url, List<string> events, bool active, string? description = null)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Url = url;
        Events = events;
        Active = active;
        Description = description;
    }
}

/// <summary>
/// Evento disparado quando um webhook é atualizado
/// </summary>
public class WebhookAtualizado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("oldValue")]
    public string OldValue { get; set; } = string.Empty;

    [JsonPropertyName("newValue")]
    public string NewValue { get; set; } = string.Empty;

    public WebhookAtualizado() { }

    public WebhookAtualizado(Guid webhookId, Guid clientId, string field, string oldValue, string newValue)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Field = field;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Evento disparado quando um webhook é ativado
/// </summary>
public class WebhookAtivado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    public WebhookAtivado() { }

    public WebhookAtivado(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}

/// <summary>
/// Evento disparado quando um webhook é desativado
/// </summary>
public class WebhookDesativado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    public WebhookDesativado() { }

    public WebhookDesativado(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}

/// <summary>
/// Evento disparado quando um webhook é entregue
/// </summary>
public class WebhookEntregue : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    public WebhookEntregue() { }

    public WebhookEntregue(Guid webhookId, Guid clientId, bool success, string? errorMessage = null)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Success = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Evento disparado quando um QR Code PIX é gerado
/// </summary>
public class QrCodeGerado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;

    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("qrcodePayload")]
    public string QrcodePayload { get; set; } = string.Empty;

    [JsonPropertyName("qrcodeImage")]
    public string QrcodeImage { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "static" ou "dynamic"

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public QrCodeGerado() { }

    public QrCodeGerado(Guid transactionId, string externalId, string pixKey, decimal? amount,
        string qrcodePayload, string qrcodeImage, string type, DateTime? expiresAt,
        string bankCode, string? description = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        PixKey = pixKey;
        Amount = amount;
        QrcodePayload = qrcodePayload;
        QrcodeImage = qrcodeImage;
        Type = type;
        ExpiresAt = expiresAt;
        BankCode = bankCode;
        Description = description;
    }
}

/// <summary>
/// Evento disparado quando um QR Code dinâmico expira
/// </summary>
public class QrCodeExpirado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;

    [JsonPropertyName("qrcodePayload")]
    public string QrcodePayload { get; set; } = string.Empty;

    public QrCodeExpirado() { }

    public QrCodeExpirado(Guid transactionId, string externalId, string qrcodePayload)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        QrcodePayload = qrcodePayload;
    }
}

/// <summary>
/// Evento disparado quando um acesso é criado
/// </summary>
public class AcessoCriado : DomainEvent
{
    [JsonPropertyName("acessoId")]
    public Guid AcessoId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("parentUserId")]
    public Guid? ParentUserId { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    [JsonPropertyName("criadoPor")]
    public Guid CriadoPor { get; set; }

    public AcessoCriado() { }

    public AcessoCriado(Guid acessoId, Guid userId, string email, string role,
        List<string> permissions, Guid criadoPor, Guid? parentUserId = null)
    {
        AcessoId = acessoId;
        UserId = userId;
        ParentUserId = parentUserId;
        Email = email;
        Role = role;
        Permissions = permissions;
        CriadoPor = criadoPor;
    }
}

/// <summary>
/// Evento disparado quando um acesso é atualizado
/// </summary>
public class AcessoAtualizado : DomainEvent
{
    [JsonPropertyName("acessoId")]
    public Guid AcessoId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    [JsonPropertyName("permissoesAnteriores")]
    public List<string> PermissoesAnteriores { get; set; } = new();

    [JsonPropertyName("atualizadoPor")]
    public Guid AtualizadoPor { get; set; }

    public AcessoAtualizado() { }

    public AcessoAtualizado(Guid acessoId, Guid userId, string role,
        List<string> permissions, List<string> permissoesAnteriores, Guid atualizadoPor)
    {
        AcessoId = acessoId;
        UserId = userId;
        Role = role;
        Permissions = permissions;
        PermissoesAnteriores = permissoesAnteriores;
        AtualizadoPor = atualizadoPor;
    }
}

/// <summary>
/// Evento disparado quando um acesso é removido
/// </summary>
public class AcessoRemovido : DomainEvent
{
    [JsonPropertyName("acessoId")]
    public Guid AcessoId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("motivo")]
    public string Motivo { get; set; } = string.Empty;

    [JsonPropertyName("removidoPor")]
    public Guid RemovidoPor { get; set; }

    public AcessoRemovido() { }

    public AcessoRemovido(Guid acessoId, Guid userId, string email, string role,
        string motivo, Guid removidoPor)
    {
        AcessoId = acessoId;
        UserId = userId;
        Email = email;
        Role = role;
        Motivo = motivo;
        RemovidoPor = removidoPor;
    }
}

/// <summary>
/// Evento disparado quando um PIX é rejeitado via webhook
/// </summary>
public class PixRejeitado : DomainEvent
{
    [JsonPropertyName("txId")]
    public string TxId { get; set; } = string.Empty;

    [JsonPropertyName("rejectionReason")]
    public string RejectionReason { get; set; } = string.Empty;

    [JsonPropertyName("rejectedAt")]
    public DateTime RejectedAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public PixRejeitado() { }

    public PixRejeitado(string txId, string rejectionReason, string bankCode)
    {
        TxId = txId;
        RejectionReason = rejectionReason;
        RejectedAt = DateTime.UtcNow;
        BankCode = bankCode;
    }
}

/// <summary>
/// Evento disparado quando um PIX expira via webhook
/// </summary>
public class PixExpirado : DomainEvent
{
    [JsonPropertyName("txId")]
    public string TxId { get; set; } = string.Empty;

    [JsonPropertyName("expiredAt")]
    public DateTime ExpiredAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public PixExpirado() { }

    public PixExpirado(string txId, string bankCode)
    {
        TxId = txId;
        ExpiredAt = DateTime.UtcNow;
        BankCode = bankCode;
    }
}

/// <summary>
/// Evento disparado quando um Boleto é confirmado via webhook
/// </summary>
public class BoletoConfirmado : DomainEvent
{
    [JsonPropertyName("nossoNumero")]
    public string NossoNumero { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("payerDocument")]
    public string? PayerDocument { get; set; }

    [JsonPropertyName("payerName")]
    public string? PayerName { get; set; }

    [JsonPropertyName("paidAt")]
    public DateTime PaidAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public BoletoConfirmado() { }

    public BoletoConfirmado(string nossoNumero, decimal amount, string bankCode)
    {
        NossoNumero = nossoNumero;
        Amount = amount;
        PaidAt = DateTime.UtcNow;
        BankCode = bankCode;
    }
}

/// <summary>
/// Evento disparado quando um Boleto expira via webhook
/// </summary>
public class BoletoExpirado : DomainEvent
{
    [JsonPropertyName("nossoNumero")]
    public string NossoNumero { get; set; } = string.Empty;

    [JsonPropertyName("expiredAt")]
    public DateTime ExpiredAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public BoletoExpirado() { }

    public BoletoExpirado(string nossoNumero, string bankCode)
    {
        NossoNumero = nossoNumero;
        ExpiredAt = DateTime.UtcNow;
        BankCode = bankCode;
    }
}

/// <summary>
/// Evento disparado quando um Boleto é cancelado via webhook
/// </summary>
public class BoletoCancelado : DomainEvent
{
    [JsonPropertyName("nossoNumero")]
    public string NossoNumero { get; set; } = string.Empty;

    [JsonPropertyName("cancelledAt")]
    public DateTime CancelledAt { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public BoletoCancelado() { }

    public BoletoCancelado(string nossoNumero, string bankCode)
    {
        NossoNumero = nossoNumero;
        CancelledAt = DateTime.UtcNow;
        BankCode = bankCode;
    }
}



/// <summary>
/// Evento disparado para notificar frontend sobre PIX recebido
/// </summary>
public class NotificacaoPixRecebido : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("payerName")]
    public string PayerName { get; set; } = string.Empty;

    [JsonPropertyName("payerDocument")]
    public string? PayerDocument { get; set; }

    [JsonPropertyName("txId")]
    public string? TxId { get; set; }

    [JsonPropertyName("newBalance")]
    public decimal NewBalance { get; set; }

    [JsonPropertyName("receivedAt")]
    public DateTime ReceivedAt { get; set; }

    public NotificacaoPixRecebido() { }

    public NotificacaoPixRecebido(Guid clientId, decimal amount, string payerName)
    {
        ClientId = clientId;
        Amount = amount;
        PayerName = payerName;
        ReceivedAt = DateTime.UtcNow;
    }
}
