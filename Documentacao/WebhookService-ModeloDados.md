# 🔗 **WebhookService - Modelo de Dados**

## 📋 **Visão Geral**

O WebhookService gerencia webhooks, entregas e eventos para notificações em tempo real no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `public` (default)
### **Porta**: 5008
### **Tecnologias**: PostgreSQL + Dapper + MassTransit

---

## 📊 **Entidades Principais**

### **1. Webhook (Aggregate Root)**

**Descrição**: Configuração de webhook do cliente

```csharp
public class Webhook : AggregateRoot
{
    public Guid ClientId { get; private set; }          // ID do cliente
    public string Url { get; private set; }             // URL do webhook
    public List<string> Events { get; private set; }    // Eventos subscritos
    public string? Secret { get; private set; }         // Secret para HMAC
    public bool Active { get; private set; }            // Status ativo
    public string? Description { get; private set; }    // Descrição
    public DateTime CreatedAt { get; private set; }     // Data criação
    public DateTime? LastTriggered { get; private set; } // Último disparo
    public int SuccessCount { get; private set; }       // Sucessos
    public int FailureCount { get; private set; }       // Falhas
}
```

### **2. WebhookDelivery**

**Descrição**: Entrega de webhook (tentativas)

```csharp
public class WebhookDelivery
{
    public Guid Id { get; set; }                        // PK
    public Guid WebhookId { get; set; }                 // FK Webhook
    public string EventType { get; set; }               // Tipo do evento
    public string Payload { get; set; }                 // Payload JSON
    public string Status { get; set; }                  // Status entrega
    public int? HttpStatusCode { get; set; }            // Status HTTP
    public string? ResponseBody { get; set; }           // Resposta
    public string? ErrorMessage { get; set; }           // Mensagem erro
    public int AttemptCount { get; set; } = 0;          // Tentativas
    public DateTime? NextRetryAt { get; set; }          // Próxima tentativa
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? DeliveredAt { get; set; }          // Data entrega
}
```

### **3. WebhookEvent**

**Descrição**: Evento de webhook (log)

```csharp
public class WebhookEvent
{
    public Guid Id { get; set; }                        // PK
    public Guid ClientId { get; set; }                  // ID do cliente
    public string EventType { get; set; }               // Tipo evento
    public string EventData { get; set; }               // Dados do evento
    public string? SourceService { get; set; }          // Serviço origem
    public string? SourceId { get; set; }               // ID origem
    public DateTime CreatedAt { get; set; }             // Data criação
    public bool Processed { get; set; } = false;        // Processado
    public DateTime? ProcessedAt { get; set; }          // Data processamento
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **webhooks**
```sql
CREATE TABLE webhooks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    url VARCHAR(2000) NOT NULL,
    events JSONB NOT NULL DEFAULT '[]',
    secret VARCHAR(255),
    active BOOLEAN NOT NULL DEFAULT true,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_triggered TIMESTAMP WITH TIME ZONE,
    success_count INTEGER NOT NULL DEFAULT 0,
    failure_count INTEGER NOT NULL DEFAULT 0,
    
    CONSTRAINT chk_webhooks_url_valid CHECK (url ~ '^https?://'),
    CONSTRAINT chk_webhooks_counts_positive CHECK (success_count >= 0 AND failure_count >= 0)
);
```

### **webhook_deliveries**
```sql
CREATE TABLE webhook_deliveries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    webhook_id UUID NOT NULL REFERENCES webhooks(id) ON DELETE CASCADE,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    http_status_code INTEGER,
    response_body TEXT,
    error_message TEXT,
    attempt_count INTEGER NOT NULL DEFAULT 0,
    next_retry_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    delivered_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT chk_webhook_deliveries_status CHECK (status IN ('PENDING', 'SUCCESS', 'FAILED', 'RETRYING')),
    CONSTRAINT chk_webhook_deliveries_attempt_count CHECK (attempt_count >= 0),
    CONSTRAINT chk_webhook_deliveries_http_status CHECK (http_status_code IS NULL OR (http_status_code >= 100 AND http_status_code < 600))
);
```

### **webhook_events**
```sql
CREATE TABLE webhook_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    source_service VARCHAR(100),
    source_id VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    processed BOOLEAN NOT NULL DEFAULT false,
    processed_at TIMESTAMP WITH TIME ZONE
);
```

---

## 🔄 **Tipos de Eventos**

### **Eventos PIX**
```csharp
public static class PixEvents
{
    public const string PIX_RECEIVED = "pix.received";
    public const string PIX_SENT = "pix.sent";
    public const string PIX_FAILED = "pix.failed";
    public const string PIX_REFUNDED = "pix.refunded";
    public const string QR_CODE_CREATED = "qr_code.created";
    public const string QR_CODE_PAID = "qr_code.paid";
    public const string QR_CODE_EXPIRED = "qr_code.expired";
}
```

### **Eventos Boleto**
```csharp
public static class BoletoEvents
{
    public const string BOLETO_CREATED = "boleto.created";
    public const string BOLETO_PAID = "boleto.paid";
    public const string BOLETO_EXPIRED = "boleto.expired";
    public const string BOLETO_CANCELLED = "boleto.cancelled";
}
```

### **Eventos TED**
```csharp
public static class TedEvents
{
    public const string TED_SENT = "ted.sent";
    public const string TED_RECEIVED = "ted.received";
    public const string TED_FAILED = "ted.failed";
    public const string TED_RETURNED = "ted.returned";
}
```

### **Eventos Conta**
```csharp
public static class AccountEvents
{
    public const string BALANCE_UPDATED = "account.balance_updated";
    public const string ACCOUNT_CREATED = "account.created";
    public const string ACCOUNT_BLOCKED = "account.blocked";
    public const string ACCOUNT_UNBLOCKED = "account.unblocked";
}
```

---

## 🔑 **DTOs e Requests**

### **CreateWebhookRequest**
```csharp
public class CreateWebhookRequest
{
    public Guid? ClientId { get; set; }                 // ID cliente (opcional)
    public string Url { get; set; }                     // URL webhook
    public List<string> Events { get; set; } = new();   // Eventos
    public string? Secret { get; set; }                 // Secret HMAC
    public string? Description { get; set; }            // Descrição
}
```

### **UpdateWebhookRequest**
```csharp
public class UpdateWebhookRequest
{
    public string? Url { get; set; }                    // Nova URL
    public List<string>? Events { get; set; }           // Novos eventos
    public string? Secret { get; set; }                 // Novo secret
    public bool? Active { get; set; }                   // Status ativo
    public string? Description { get; set; }            // Nova descrição
}
```

### **WebhookResponse**
```csharp
public class WebhookResponse
{
    public Guid Id { get; set; }                        // ID webhook
    public Guid ClientId { get; set; }                  // ID cliente
    public string Url { get; set; }                     // URL
    public List<string> Events { get; set; }            // Eventos
    public bool Active { get; set; }                    // Status ativo
    public string? Description { get; set; }            // Descrição
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? LastTriggered { get; set; }        // Último disparo
    public int SuccessCount { get; set; }               // Sucessos
    public int FailureCount { get; set; }               // Falhas
}
```

### **WebhookDeliveryResponse**
```csharp
public class WebhookDeliveryResponse
{
    public Guid Id { get; set; }                        // ID entrega
    public string EventType { get; set; }               // Tipo evento
    public string Status { get; set; }                  // Status
    public int? HttpStatusCode { get; set; }            // Status HTTP
    public string? ErrorMessage { get; set; }           // Erro
    public int AttemptCount { get; set; }               // Tentativas
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? DeliveredAt { get; set; }          // Data entrega
    public DateTime? NextRetryAt { get; set; }          // Próxima tentativa
}
```

### **TriggerWebhookRequest**
```csharp
public class TriggerWebhookRequest
{
    public Guid? ClientId { get; set; }                 // ID cliente (opcional)
    public string EventType { get; set; }               // Tipo evento
    public object EventData { get; set; }               // Dados evento
    public string? SourceService { get; set; }          // Serviço origem
    public string? SourceId { get; set; }               // ID origem
}
```

---

## 🔄 **Sistema de Retry**

### **Configuração de Retry**
```csharp
public class RetryConfig
{
    public static readonly int[] RetryDelaysMinutes = { 1, 2, 4, 8, 16 }; // Exponential backoff
    public static readonly int MaxRetryAttempts = 5;
    public static readonly int TimeoutSeconds = 30;
}
```

### **Status de Entrega**
```csharp
public enum DeliveryStatus
{
    PENDING,    // Pendente
    SUCCESS,    // Sucesso
    FAILED,     // Falhou
    RETRYING    // Tentando novamente
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- Webhooks
CREATE INDEX idx_webhooks_client_id ON webhooks(client_id);
CREATE INDEX idx_webhooks_active ON webhooks(active);
CREATE INDEX idx_webhooks_events ON webhooks USING GIN(events);

-- Webhook Deliveries
CREATE INDEX idx_webhook_deliveries_webhook_id ON webhook_deliveries(webhook_id);
CREATE INDEX idx_webhook_deliveries_status ON webhook_deliveries(status);
CREATE INDEX idx_webhook_deliveries_event_type ON webhook_deliveries(event_type);
CREATE INDEX idx_webhook_deliveries_created_at ON webhook_deliveries(created_at);
CREATE INDEX idx_webhook_deliveries_next_retry ON webhook_deliveries(next_retry_at) WHERE status = 'RETRYING';

-- Webhook Events
CREATE INDEX idx_webhook_events_client_id ON webhook_events(client_id);
CREATE INDEX idx_webhook_events_event_type ON webhook_events(event_type);
CREATE INDEX idx_webhook_events_processed ON webhook_events(processed);
CREATE INDEX idx_webhook_events_created_at ON webhook_events(created_at);
CREATE INDEX idx_webhook_events_source ON webhook_events(source_service, source_id);
```

---

## 🔐 **Segurança**

### **HMAC Signature**
```csharp
public class WebhookSecurity
{
    public static string GenerateSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLower();
    }
    
    public static bool ValidateSignature(string payload, string signature, string secret)
    {
        var expectedSignature = GenerateSignature(payload, secret);
        return signature.Equals($"sha256={expectedSignature}", StringComparison.OrdinalIgnoreCase);
    }
}
```

### **Headers de Segurança**
```csharp
public static class WebhookHeaders
{
    public const string SIGNATURE = "X-Webhook-Signature";
    public const string TIMESTAMP = "X-Webhook-Timestamp";
    public const string EVENT_TYPE = "X-Webhook-Event";
    public const string DELIVERY_ID = "X-Webhook-Delivery";
}
```

---

## 🔗 **Relacionamentos**

### **Webhook ↔ WebhookDelivery**
- **Tipo**: One-to-Many
- **Chave**: `webhook_id` → `webhooks.id`
- **Descrição**: Um webhook pode ter múltiplas entregas

### **WebhookEvent → Cliente**
- **Tipo**: Many-to-One
- **Chave**: `client_id` (referência externa)
- **Descrição**: Eventos pertencem a um cliente

---

## 🎯 **Casos de Uso**

### **Configuração de Webhooks**
- Cadastro de URLs de webhook
- Seleção de eventos de interesse
- Configuração de secrets para segurança
- Ativação/desativação

### **Entrega de Eventos**
- Disparo automático de webhooks
- Sistema de retry com backoff exponencial
- Logs de tentativas e respostas
- Monitoramento de falhas

### **Segurança**
- Assinatura HMAC dos payloads
- Validação de timestamps
- Headers de identificação
- Proteção contra replay attacks

### **Monitoramento**
- Estatísticas de sucesso/falha
- Logs de entregas
- Alertas de falhas recorrentes
- Dashboard de performance

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
