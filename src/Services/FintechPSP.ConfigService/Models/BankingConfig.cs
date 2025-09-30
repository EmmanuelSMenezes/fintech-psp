using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechPSP.ConfigService.Models;

/// <summary>
/// Modelo para configurações bancárias
/// </summary>
[Table("banking_configs")]
public class BankingConfig
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [Column("settings", TypeName = "jsonb")]
    public string? Settings { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Construtor padrão
    /// </summary>
    public BankingConfig()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public BankingConfig(string name, string type, bool enabled = true, string? settings = null, string? createdBy = null)
        : this()
    {
        Name = name;
        Type = type;
        Enabled = enabled;
        Settings = settings;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Atualiza a configuração
    /// </summary>
    public void Update(string name, string type, bool enabled, string? settings = null, string? updatedBy = null)
    {
        Name = name;
        Type = type;
        Enabled = enabled;
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
