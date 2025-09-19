using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FintechPSP.CompanyService.Models;

/// <summary>
/// Modelo para empresa cliente
/// </summary>
public class Company
{
    /// <summary>
    /// ID único da empresa
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Razão Social
    /// </summary>
    [Required]
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// Nome Fantasia
    /// </summary>
    public string? NomeFantasia { get; set; }

    /// <summary>
    /// CNPJ da empresa
    /// </summary>
    [Required]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Inscrição Estadual
    /// </summary>
    public string? InscricaoEstadual { get; set; }

    /// <summary>
    /// Inscrição Municipal
    /// </summary>
    public string? InscricaoMunicipal { get; set; }

    /// <summary>
    /// Endereço da empresa
    /// </summary>
    public CompanyAddress Address { get; set; } = new();

    /// <summary>
    /// Telefone principal
    /// </summary>
    public string? Telefone { get; set; }

    /// <summary>
    /// Email principal
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Site da empresa
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Dados do contrato social
    /// </summary>
    public ContractData ContractData { get; set; } = new();

    /// <summary>
    /// Status da empresa
    /// </summary>
    public CompanyStatus Status { get; set; } = CompanyStatus.PendingDocuments;

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Data de aprovação
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// ID do usuário que aprovou
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Observações
    /// </summary>
    public string? Observacoes { get; set; }
}

/// <summary>
/// Endereço da empresa
/// </summary>
public class CompanyAddress
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
/// Dados do contrato social
/// </summary>
public class ContractData
{
    /// <summary>
    /// Número do contrato social
    /// </summary>
    public string? NumeroContrato { get; set; }

    /// <summary>
    /// Data do contrato social
    /// </summary>
    public DateTime? DataContrato { get; set; }

    /// <summary>
    /// Junta Comercial
    /// </summary>
    public string? JuntaComercial { get; set; }

    /// <summary>
    /// NIRE (Número de Identificação do Registro de Empresas)
    /// </summary>
    public string? Nire { get; set; }

    /// <summary>
    /// Capital social
    /// </summary>
    public decimal? CapitalSocial { get; set; }

    /// <summary>
    /// Atividade principal (CNAE)
    /// </summary>
    public string? AtividadePrincipal { get; set; }

    /// <summary>
    /// Atividades secundárias (CNAE)
    /// </summary>
    public List<string> AtividadesSecundarias { get; set; } = new();
}

/// <summary>
/// Status da empresa
/// </summary>
public enum CompanyStatus
{
    /// <summary>
    /// Aguardando documentos
    /// </summary>
    PendingDocuments,

    /// <summary>
    /// Em análise
    /// </summary>
    UnderReview,

    /// <summary>
    /// Aprovada
    /// </summary>
    Approved,

    /// <summary>
    /// Rejeitada
    /// </summary>
    Rejected,

    /// <summary>
    /// Ativa
    /// </summary>
    Active,

    /// <summary>
    /// Suspensa
    /// </summary>
    Suspended,

    /// <summary>
    /// Inativa
    /// </summary>
    Inactive
}

/// <summary>
/// Request para criar empresa
/// </summary>
public class CreateCompanyRequest
{
    /// <summary>
    /// Dados da empresa
    /// </summary>
    public CompanyData Company { get; set; } = new();

    /// <summary>
    /// Dados do solicitante
    /// </summary>
    public ApplicantData Applicant { get; set; } = new();

    /// <summary>
    /// Representantes legais
    /// </summary>
    public List<LegalRepresentativeData> LegalRepresentatives { get; set; } = new();
}

/// <summary>
/// Dados da empresa para criação
/// </summary>
public class CompanyData
{
    [Required]
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    [Required]
    public string Cnpj { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public CompanyAddress Address { get; set; } = new();
    public string? Telefone { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Website { get; set; }
    public ContractData ContractData { get; set; } = new();
    public string? Observacoes { get; set; }
}

/// <summary>
/// Dados do solicitante para criação
/// </summary>
public class ApplicantData
{
    [Required]
    public string NomeCompleto { get; set; } = string.Empty;
    [Required]
    public string Cpf { get; set; } = string.Empty;
    public string? Rg { get; set; }
    public string? OrgaoExpedidor { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Nacionalidade { get; set; }
    public string? Profissao { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public ApplicantAddress Address { get; set; } = new();
    public decimal? RendaMensal { get; set; }
    public string? Cargo { get; set; }
    public bool IsMainRepresentative { get; set; }
}

/// <summary>
/// Dados do representante legal para criação
/// </summary>
public class LegalRepresentativeData
{
    [Required]
    public string NomeCompleto { get; set; } = string.Empty;
    [Required]
    public string Cpf { get; set; } = string.Empty;
    public string? Rg { get; set; }
    public string? OrgaoExpedidor { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Nacionalidade { get; set; }
    public string? Profissao { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public RepresentativeAddress Address { get; set; } = new();
    [Required]
    public string Cargo { get; set; } = string.Empty;
    public RepresentationType Type { get; set; }
    public decimal? PercentualParticipacao { get; set; }
    public string? PoderesRepresentacao { get; set; }
    public bool PodeAssinarSozinho { get; set; }
    public decimal? LimiteAlcada { get; set; }
}
