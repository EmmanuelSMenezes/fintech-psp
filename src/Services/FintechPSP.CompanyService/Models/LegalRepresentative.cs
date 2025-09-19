using System;
using System.ComponentModel.DataAnnotations;

namespace FintechPSP.CompanyService.Models;

/// <summary>
/// Representante legal da empresa
/// </summary>
public class LegalRepresentative
{
    /// <summary>
    /// ID único do representante
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID da empresa
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Nome completo
    /// </summary>
    [Required]
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>
    /// CPF
    /// </summary>
    [Required]
    public string Cpf { get; set; } = string.Empty;

    /// <summary>
    /// RG
    /// </summary>
    public string? Rg { get; set; }

    /// <summary>
    /// Órgão expedidor do RG
    /// </summary>
    public string? OrgaoExpedidor { get; set; }

    /// <summary>
    /// Data de nascimento
    /// </summary>
    public DateTime? DataNascimento { get; set; }

    /// <summary>
    /// Estado civil
    /// </summary>
    public string? EstadoCivil { get; set; }

    /// <summary>
    /// Nacionalidade
    /// </summary>
    public string? Nacionalidade { get; set; }

    /// <summary>
    /// Profissão
    /// </summary>
    public string? Profissao { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Telefone
    /// </summary>
    public string? Telefone { get; set; }

    /// <summary>
    /// Celular
    /// </summary>
    public string? Celular { get; set; }

    /// <summary>
    /// Endereço residencial
    /// </summary>
    public RepresentativeAddress Address { get; set; } = new();

    /// <summary>
    /// Cargo na empresa
    /// </summary>
    [Required]
    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de representação
    /// </summary>
    public RepresentationType Type { get; set; }

    /// <summary>
    /// Percentual de participação na empresa
    /// </summary>
    public decimal? PercentualParticipacao { get; set; }

    /// <summary>
    /// Poderes de representação
    /// </summary>
    public string? PoderesRepresentacao { get; set; }

    /// <summary>
    /// Se pode assinar sozinho
    /// </summary>
    public bool PodeAssinarSozinho { get; set; }

    /// <summary>
    /// Limite de alçada (valor máximo que pode movimentar)
    /// </summary>
    public decimal? LimiteAlcada { get; set; }

    /// <summary>
    /// Se está ativo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Endereço do representante legal
/// </summary>
public class RepresentativeAddress
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = "Brasil";
}

/// <summary>
/// Tipo de representação
/// </summary>
public enum RepresentationType
{
    /// <summary>
    /// Administrador
    /// </summary>
    Administrator,

    /// <summary>
    /// Sócio Administrador
    /// </summary>
    PartnerAdministrator,

    /// <summary>
    /// Diretor
    /// </summary>
    Director,

    /// <summary>
    /// Presidente
    /// </summary>
    President,

    /// <summary>
    /// Vice-Presidente
    /// </summary>
    VicePresident,

    /// <summary>
    /// Procurador
    /// </summary>
    Attorney,

    /// <summary>
    /// Sócio
    /// </summary>
    Partner,

    /// <summary>
    /// Outro
    /// </summary>
    Other
}
