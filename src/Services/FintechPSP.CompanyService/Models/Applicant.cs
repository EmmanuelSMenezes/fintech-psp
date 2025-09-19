using System;
using System.ComponentModel.DataAnnotations;

namespace FintechPSP.CompanyService.Models;

/// <summary>
/// Dados do solicitante (pessoa física)
/// </summary>
public class Applicant
{
    /// <summary>
    /// ID único do solicitante
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
    public ApplicantAddress Address { get; set; } = new();

    /// <summary>
    /// Renda mensal
    /// </summary>
    public decimal? RendaMensal { get; set; }

    /// <summary>
    /// Cargo na empresa
    /// </summary>
    public string? Cargo { get; set; }

    /// <summary>
    /// Se é o representante legal principal
    /// </summary>
    public bool IsMainRepresentative { get; set; }

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
/// Endereço do solicitante
/// </summary>
public class ApplicantAddress
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
