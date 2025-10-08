# 🏢 **CompanyService - Modelo de Dados**

## 📋 **Visão Geral**

O CompanyService gerencia empresas clientes, representantes legais e solicitantes no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `company_service`
### **Porta**: 5009
### **Tecnologias**: PostgreSQL + Dapper

---

## 📊 **Entidades Principais**

### **1. Company**

**Descrição**: Empresa cliente principal

```csharp
public class Company
{
    public Guid Id { get; set; }                        // PK
    public string RazaoSocial { get; set; }             // Razão social
    public string? NomeFantasia { get; set; }           // Nome fantasia
    public string Cnpj { get; set; }                    // CNPJ único
    public string? InscricaoEstadual { get; set; }      // IE
    public string? InscricaoMunicipal { get; set; }     // IM
    public CompanyAddress Address { get; set; }         // Endereço
    public string? Telefone { get; set; }               // Telefone
    public string? Email { get; set; }                  // Email
    public string? Website { get; set; }                // Site
    public ContractData ContractData { get; set; }      // Dados contratuais
    public CompanyStatus Status { get; set; }           // Status
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
    public DateTime? ApprovedAt { get; set; }           // Data aprovação
    public Guid? ApprovedBy { get; set; }               // Quem aprovou
    public string? Observacoes { get; set; }            // Observações
    public ApplicantData? Applicant { get; set; }       // Solicitante
}
```

### **2. CompanyAddress**

**Descrição**: Endereço da empresa

```csharp
public class CompanyAddress
{
    public string Cep { get; set; }                     // CEP
    public string Logradouro { get; set; }              // Logradouro
    public string Numero { get; set; }                  // Número
    public string? Complemento { get; set; }            // Complemento
    public string Bairro { get; set; }                  // Bairro
    public string Cidade { get; set; }                  // Cidade
    public string Estado { get; set; }                  // Estado (UF)
    public string Pais { get; set; } = "Brasil";        // País
}
```

### **3. ContractData**

**Descrição**: Dados do contrato social

```csharp
public class ContractData
{
    public string? NumeroContrato { get; set; }         // Número contrato
    public DateTime? DataContrato { get; set; }         // Data contrato
    public string? JuntaComercial { get; set; }         // Junta comercial
    public string? Nire { get; set; }                   // NIRE
    public decimal? CapitalSocial { get; set; }         // Capital social
    public DateTime? DataConstituicao { get; set; }     // Data constituição
    public string? NaturezaJuridica { get; set; }       // Natureza jurídica
    public string? AtividadePrincipal { get; set; }     // Atividade principal
    public string? RegimeTributario { get; set; }       // Regime tributário
    public List<string> AtividadesSecundarias { get; set; } = new(); // CNAEs secundários
}
```

### **4. LegalRepresentativeData**

**Descrição**: Representante legal da empresa

```csharp
public class LegalRepresentativeData
{
    public Guid Id { get; set; }                        // PK
    public Guid CompanyId { get; set; }                 // FK Company
    public string NomeCompleto { get; set; }            // Nome completo
    public string Cpf { get; set; }                     // CPF
    public string? Rg { get; set; }                     // RG
    public string? OrgaoExpedidor { get; set; }         // Órgão expedidor
    public DateTime? DataNascimento { get; set; }       // Data nascimento
    public string? EstadoCivil { get; set; }            // Estado civil
    public string? Nacionalidade { get; set; }          // Nacionalidade
    public string? Profissao { get; set; }              // Profissão
    public string? Email { get; set; }                  // Email
    public string? Telefone { get; set; }               // Telefone
    public string? Celular { get; set; }                // Celular
    public ApplicantAddress Address { get; set; }       // Endereço
    public string Cargo { get; set; }                   // Cargo
    public RepresentationType Type { get; set; }        // Tipo representação
    public decimal? PercentualParticipacao { get; set; } // % participação
    public string? PoderesRepresentacao { get; set; }   // Poderes
    public bool PodeAssinarSozinho { get; set; }        // Pode assinar sozinho
    public decimal? LimiteAlcada { get; set; }          // Limite alçada
    public bool IsActive { get; set; }                  // Status ativo
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
}
```

### **5. ApplicantData**

**Descrição**: Solicitante da abertura da empresa

```csharp
public class ApplicantData
{
    public Guid Id { get; set; }                        // PK
    public Guid CompanyId { get; set; }                 // FK Company
    public string NomeCompleto { get; set; }            // Nome completo
    public string Cpf { get; set; }                     // CPF
    public string? Rg { get; set; }                     // RG
    public string? OrgaoExpedidor { get; set; }         // Órgão expedidor
    public DateTime? DataNascimento { get; set; }       // Data nascimento
    public string? EstadoCivil { get; set; }            // Estado civil
    public string? Nacionalidade { get; set; }          // Nacionalidade
    public string? Profissao { get; set; }              // Profissão
    public string? Email { get; set; }                  // Email
    public string? Telefone { get; set; }               // Telefone
    public string? Celular { get; set; }                // Celular
    public ApplicantAddress Address { get; set; }       // Endereço
    public decimal? RendaMensal { get; set; }           // Renda mensal
    public string? Cargo { get; set; }                  // Cargo
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **companies**
```sql
CREATE TABLE company_service.companies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    razao_social VARCHAR(255) NOT NULL,
    nome_fantasia VARCHAR(255),
    cnpj VARCHAR(18) NOT NULL UNIQUE,
    inscricao_estadual VARCHAR(50),
    inscricao_municipal VARCHAR(50),
    
    -- Address
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    
    -- Contact
    telefone VARCHAR(20),
    email VARCHAR(255),
    website VARCHAR(255),
    
    -- Contract Data
    capital_social DECIMAL(15,2),
    data_constituicao DATE,
    natureza_juridica VARCHAR(100),
    atividade_principal VARCHAR(255),
    regime_tributario VARCHAR(50),
    
    -- Status and Control
    status VARCHAR(50) NOT NULL DEFAULT 'PendingDocuments',
    observacoes TEXT,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    approved_at TIMESTAMP WITH TIME ZONE,
    approved_by UUID,
    
    CONSTRAINT chk_companies_status CHECK (status IN (
        'PendingDocuments', 'UnderReview', 'Approved', 'Rejected', 
        'Active', 'Inactive', 'Suspended'
    ))
);
```

### **legal_representatives**
```sql
CREATE TABLE company_service.legal_representatives (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES company_service.companies(id) ON DELETE CASCADE,
    
    -- Personal Data
    nome_completo VARCHAR(255) NOT NULL,
    cpf VARCHAR(14) NOT NULL,
    rg VARCHAR(20),
    orgao_expedidor VARCHAR(20),
    data_nascimento DATE,
    estado_civil VARCHAR(50),
    nacionalidade VARCHAR(100) DEFAULT 'Brasileira',
    profissao VARCHAR(100),
    
    -- Contact
    email VARCHAR(255),
    telefone VARCHAR(20),
    celular VARCHAR(20),
    
    -- Address
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    
    -- Company Role
    cargo VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    percentual_participacao DECIMAL(5,2),
    poderes_representacao TEXT,
    pode_assinar_sozinho BOOLEAN DEFAULT false,
    limite_alcada DECIMAL(15,2),
    
    -- Status and Control
    is_active BOOLEAN DEFAULT true,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT uk_legal_representatives_company_cpf UNIQUE (company_id, cpf)
);
```

### **applicants**
```sql
CREATE TABLE company_service.applicants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES company_service.companies(id) ON DELETE CASCADE,
    
    -- Personal Data
    nome_completo VARCHAR(255) NOT NULL,
    cpf VARCHAR(14) NOT NULL,
    rg VARCHAR(20),
    orgao_expedidor VARCHAR(20),
    data_nascimento DATE,
    estado_civil VARCHAR(50),
    nacionalidade VARCHAR(100) DEFAULT 'Brasileira',
    profissao VARCHAR(100),
    
    -- Contact
    email VARCHAR(255),
    telefone VARCHAR(20),
    celular VARCHAR(20),
    
    -- Address
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    
    -- Financial
    renda_mensal DECIMAL(10,2),
    cargo VARCHAR(100),
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);
```

---

## 🔄 **Enums**

### **CompanyStatus**
```csharp
public enum CompanyStatus
{
    PendingDocuments,   // Aguardando documentos
    UnderReview,        // Em análise
    Approved,           // Aprovada
    Rejected,           // Rejeitada
    Active,             // Ativa
    Suspended,          // Suspensa
    Inactive            // Inativa
}
```

### **RepresentationType**
```csharp
public enum RepresentationType
{
    Director,           // Diretor
    Manager,            // Gerente
    Partner,            // Sócio
    Attorney,           // Procurador
    Administrator       // Administrador
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- Companies
CREATE INDEX idx_companies_cnpj ON company_service.companies(cnpj);
CREATE INDEX idx_companies_status ON company_service.companies(status);
CREATE INDEX idx_companies_created_at ON company_service.companies(created_at);
CREATE INDEX idx_companies_razao_social ON company_service.companies(razao_social);

-- Legal Representatives
CREATE INDEX idx_legal_representatives_company_id ON company_service.legal_representatives(company_id);
CREATE INDEX idx_legal_representatives_cpf ON company_service.legal_representatives(cpf);
CREATE INDEX idx_legal_representatives_is_active ON company_service.legal_representatives(is_active);

-- Applicants
CREATE INDEX idx_applicants_company_id ON company_service.applicants(company_id);
CREATE INDEX idx_applicants_cpf ON company_service.applicants(cpf);
```

---

## 🔗 **Relacionamentos**

### **Company ↔ LegalRepresentative**
- **Tipo**: One-to-Many
- **Chave**: `company_id` → `companies.id`
- **Descrição**: Uma empresa pode ter múltiplos representantes legais

### **Company ↔ Applicant**
- **Tipo**: One-to-One
- **Chave**: `company_id` → `companies.id`
- **Descrição**: Uma empresa tem um solicitante principal

---

## 🎯 **Casos de Uso**

### **Gestão de Empresas**
- Cadastro de empresas clientes
- Aprovação/rejeição
- Controle de status
- Auditoria de mudanças

### **Representantes Legais**
- Cadastro de sócios e diretores
- Controle de poderes
- Hierarquia organizacional

### **Compliance**
- Validação de documentos
- Auditoria LGPD
- Rastreabilidade

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
