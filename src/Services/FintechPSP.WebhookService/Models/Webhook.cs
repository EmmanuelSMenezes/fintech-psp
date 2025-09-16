using FintechPSP.Shared.Domain.Common;
using FintechPSP.Shared.Domain.Events;

namespace FintechPSP.WebhookService.Models;

/// <summary>
/// Aggregate root para webhooks
/// </summary>
public class Webhook : AggregateRoot
{
    public Guid ClientId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public List<string> Events { get; private set; } = new();
    public string? Secret { get; private set; }
    public bool Active { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastTriggered { get; private set; }
    public int SuccessCount { get; private set; }
    public int FailureCount { get; private set; }

    // Construtor privado para EF Core
    private Webhook() { }

    public Webhook(Guid clientId, string url, List<string> events, string? secret = null, 
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL é obrigatória", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || 
            (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new ArgumentException("URL deve ser um endereço HTTP/HTTPS válido", nameof(url));

        if (events == null || !events.Any())
            throw new ArgumentException("Pelo menos um evento deve ser especificado", nameof(events));

        Id = Guid.NewGuid();
        ClientId = clientId;
        Url = url;
        Events = events.ToList();
        Secret = secret;
        Description = description;
        Active = true;
        CreatedAt = DateTime.UtcNow;
        SuccessCount = 0;
        FailureCount = 0;

        ApplyEvent(new WebhookCriado(Id, ClientId, Url, Events, Active, Description));
    }

    public void UpdateUrl(string newUrl)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            throw new ArgumentException("URL é obrigatória", nameof(newUrl));

        if (!Uri.TryCreate(newUrl, UriKind.Absolute, out var uri) || 
            (uri.Scheme != "http" && uri.Scheme != "https"))
            throw new ArgumentException("URL deve ser um endereço HTTP/HTTPS válido", nameof(newUrl));

        var oldUrl = Url;
        Url = newUrl;

        ApplyEvent(new WebhookAtualizado(Id, ClientId, "url", oldUrl, newUrl));
    }

    public void UpdateEvents(List<string> newEvents)
    {
        if (newEvents == null || !newEvents.Any())
            throw new ArgumentException("Pelo menos um evento deve ser especificado", nameof(newEvents));

        var oldEvents = Events.ToList();
        Events = newEvents.ToList();

        ApplyEvent(new WebhookAtualizado(Id, ClientId, "events", 
            string.Join(",", oldEvents), string.Join(",", newEvents)));
    }

    public void UpdateSecret(string? newSecret)
    {
        var oldSecret = Secret;
        Secret = newSecret;

        ApplyEvent(new WebhookAtualizado(Id, ClientId, "secret", 
            oldSecret ?? "", newSecret ?? ""));
    }

    public void Activate()
    {
        if (Active) return;

        Active = true;
        ApplyEvent(new WebhookAtivado(Id, ClientId));
    }

    public void Deactivate()
    {
        if (!Active) return;

        Active = false;
        ApplyEvent(new WebhookDesativado(Id, ClientId));
    }

    public void RecordSuccess()
    {
        SuccessCount++;
        LastTriggered = DateTime.UtcNow;

        ApplyEvent(new WebhookEntregue(Id, ClientId, true, null));
    }

    public void RecordFailure(string? errorMessage = null)
    {
        FailureCount++;
        LastTriggered = DateTime.UtcNow;

        ApplyEvent(new WebhookEntregue(Id, ClientId, false, errorMessage));
    }

    public void UpdateDescription(string? description)
    {
        var oldDescription = Description;
        Description = description;

        ApplyEvent(new WebhookAtualizado(Id, ClientId, "description", 
            oldDescription ?? "", description ?? ""));
    }

    // Métodos Apply para Event Sourcing
    public void Apply(WebhookCriado @event)
    {
        Id = @event.WebhookId;
        ClientId = @event.ClientId;
        Url = @event.Url;
        Events = @event.Events.ToList();
        Active = @event.Active;
        Description = @event.Description;
        CreatedAt = @event.OccurredAt;
        SuccessCount = 0;
        FailureCount = 0;
    }

    public void Apply(WebhookAtualizado @event)
    {
        switch (@event.Field.ToLower())
        {
            case "url":
                Url = @event.NewValue;
                break;
            case "events":
                Events = @event.NewValue.Split(',').ToList();
                break;
            case "secret":
                Secret = string.IsNullOrEmpty(@event.NewValue) ? null : @event.NewValue;
                break;
            case "description":
                Description = string.IsNullOrEmpty(@event.NewValue) ? null : @event.NewValue;
                break;
        }
    }

    public void Apply(WebhookAtivado @event)
    {
        Active = true;
    }

    public void Apply(WebhookDesativado @event)
    {
        Active = false;
    }

    public void Apply(WebhookEntregue @event)
    {
        if (@event.Success)
        {
            SuccessCount++;
        }
        else
        {
            FailureCount++;
        }
        LastTriggered = @event.OccurredAt;
    }
}
