import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

// Configuração base da API
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';

// Instância do axios
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

console.log('🔧 API Service inicializado com baseURL:', API_BASE_URL);

// Interceptor para adicionar token JWT automaticamente
api.interceptors.request.use(
  (config) => {
    console.log('🚀 Interceptor executado para:', config.method?.toUpperCase(), config.url);

    // Só acessar localStorage no lado do cliente
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('access_token');
      console.log('🔑 Interceptor - Token encontrado:', token ? 'SIM' : 'NÃO');

      if (token) {
        // Garantir que headers existe
        if (!config.headers) {
          config.headers = {};
        }
        config.headers.Authorization = `Bearer ${token}`;
        console.log('✅ Interceptor - Authorization header adicionado');
        console.log('🎫 Token (primeiros 20 chars):', token.substring(0, 20) + '...');

        // Log detalhado dos headers finais
        console.log('📋 Headers finais:', {
          'Content-Type': config.headers['Content-Type'],
          'Authorization': config.headers.Authorization ? 'Bearer [TOKEN]' : 'AUSENTE'
        });
      } else {
        console.log('❌ Interceptor - Nenhum token para adicionar');
        // Verificar se há dados no localStorage
        const keys = Object.keys(localStorage);
        console.log('🗂️ Chaves no localStorage:', keys);
      }
    } else {
      console.log('🖥️ Interceptor - Executando no servidor (sem localStorage)');
    }

    // Log final da configuração
    console.log('🔧 Config final - URL:', config.url);
    console.log('🔧 Config final - Headers Authorization:', config.headers?.Authorization ? 'PRESENTE' : 'AUSENTE');

    return config;
  },
  (error) => {
    console.error('❌ Erro no interceptor de request:', error);
    return Promise.reject(error);
  }
);

// Interceptor para tratar respostas e erros
api.interceptors.response.use(
  (response: AxiosResponse) => {
    console.log('✅ Resposta recebida:', response.status, response.config.method?.toUpperCase(), response.config.url);
    return response;
  },
  (error) => {
    console.log('❌ Erro na resposta:', error.response?.status, error.config?.method?.toUpperCase(), error.config?.url);

    if (error.response?.status === 401) {
      console.log('🚫 Erro 401 - Token inválido ou ausente');
      console.log('🔍 Headers enviados na requisição:', error.config?.headers?.Authorization ? 'Authorization PRESENTE' : 'Authorization AUSENTE');

      // Token expirado ou inválido - só executar no cliente
      // if (typeof window !== 'undefined') {
      //   localStorage.removeItem('access_token');
      //   localStorage.removeItem('user_data');
      //   window.location.href = '/auth/signin';
      // }
    }
    return Promise.reject(error);
  }
);

// Tipos para autenticação OAuth2 (client credentials)
export interface TokenRequest {
  grant_type: 'client_credentials';
  client_id: string;
  client_secret: string;
  scope: string;
}

export interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  scope: string;
}

// Tipos para login de usuário
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  user: {
    id: string;
    email: string;
    name: string;
    role: string;
    isMaster: boolean;
  };
}

// Tipos para usuários
export interface User {
  id: string;
  userId?: string;
  email: string;
  name?: string;
  role: string;
  permissions?: string[];
  lastLogin?: string;
  isActive?: boolean;
  createdAt: string;
  updatedAt?: string;
  document?: string;
  phone?: string;
  address?: string;
}

export interface CreateUserRequest {
  email: string;
  role: string;
  name?: string;
  password?: string;
  permissions?: string[];
  document?: string;
  phone?: string;
  address?: string;
}

export interface UpdateUserRequest {
  email?: string;
  role?: string;
  name?: string;
  permissions?: string[];
  isActive?: boolean;
  document?: string;
  phone?: string;
  address?: string;
}

// Tipos para contas bancárias
export interface BankAccount {
  contaId: string;
  clienteId: string;
  bankCode: string;
  accountNumber: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateBankAccountRequest {
  clienteId: string;
  bankCode: string;
  accountNumber: string;
  description: string;
  credentials: {
    clientId: string;
    clientSecret: string;
    mtlsCert?: string;
    additionalData?: Record<string, string>;
  };
}

// Tipos para configuração de priorização
export interface PriorityConfig {
  clienteId: string;
  prioridades: Array<{
    contaId: string;
    percentual: number;
  }>;
  totalPercentual: number;
  isValid: boolean;
  updatedAt: string;
}

// Tipos para transações
export interface Transaction {
  transactionId: string;
  externalId: string;
  amount: number;
  type: 'pix' | 'ted' | 'boleto' | 'crypto';
  status: 'pending' | 'processing' | 'completed' | 'failed' | 'cancelled';
  bankCode: string;
  contaId?: string;
  createdAt: string;
  updatedAt?: string;
  endToEndId?: string;
  description?: string;
}

// Tipos para relatórios
export interface DashboardReport {
  totalClientes: number;
  totalTransacoes: number;
  totalTransacoesHoje: number;
  volumeFinanceiro: number;
  volumeFinanceiroHoje: number;
  transacoesPorTipo: Array<{
    tipo: string;
    quantidade: number;
    volume: number;
  }>;
  transacoesPorStatus: Array<{
    status: string;
    quantidade: number;
  }>;
  transacoesPorBanco: Array<{
    bankCode: string;
    quantidade: number;
    volume: number;
  }>;
}

// Tipos para extrato
export interface ExtratoEntry {
  id: string;
  tipo: 'entrada' | 'saida';
  valor: number;
  descricao: string;
  bankCode: string;
  contaId?: string;
  transactionId: string;
  data: string;
  status: string;
}

export interface ExtratoResponse {
  clienteId: string;
  contaId?: string;
  periodo: {
    inicio: string;
    fim: string;
  };
  resumo: {
    totalEntradas: number;
    totalSaidas: number;
    saldoFinal: number;
  };
  movimentacoes: ExtratoEntry[];
}

// Tipos para acessos RBAC
export interface AccessControl {
  acessoId: string;
  userId: string;
  parentUserId?: string;
  email: string;
  role: string;
  permissions: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateAccessRequest {
  userId: string;
  role: string;
  permissions: string[];
}

export interface CreateSubUserRequest {
  subUserEmail: string;
  role: string;
  permissions: string[];
}

// Serviços da API
export const authService = {
  // Login de usuário com email/senha
  login: (data: LoginRequest): Promise<AxiosResponse<LoginResponse>> =>
    api.post('/auth/login', data),

  // Obter token OAuth2 (para aplicações)
  getToken: (data: TokenRequest): Promise<AxiosResponse<TokenResponse>> =>
    api.post('/auth/token', data),

  logout: () => {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('access_token');
      localStorage.removeItem('user_data');
    }
  },
};

export const userService = {
  getUsers: (params?: {
    page?: number;
    limit?: number;
    search?: string;
    role?: string;
  }): Promise<AxiosResponse<{
    users: User[];
    total: number;
    page: number;
    limit: number;
  }>> =>
    api.get('/admin/users', { params }),

  getUserById: (id: string): Promise<AxiosResponse<User>> =>
    api.get(`/admin/users/${id}`),

  createUser: (data: CreateUserRequest): Promise<AxiosResponse<User>> => {
    console.log('🚀 Criando usuário:', data);
    return api.post('/admin/users', data);
  },

  updateUser: (id: string, data: UpdateUserRequest): Promise<AxiosResponse<User>> => {
    console.log('✏️ Atualizando usuário:', id, data);
    return api.put(`/admin/users/${id}`, data);
  },

  deleteUser: (id: string): Promise<AxiosResponse<void>> => {
    console.log('🗑️ Deletando usuário:', id);
    return api.delete(`/admin/users/${id}`);
  },
};

// ===== COMPANY SERVICE (EMPRESAS CLIENTES) =====

export interface Company {
  id: string;
  razaoSocial: string;
  nomeFantasia?: string;
  cnpj: string;
  inscricaoEstadual?: string;
  inscricaoMunicipal?: string;
  address: CompanyAddress;
  telefone?: string;
  email?: string;
  website?: string;
  contractData: ContractData;
  status: CompanyStatus;
  createdAt: string;
  updatedAt?: string;
  approvedAt?: string;
  approvedBy?: string;
  observacoes?: string;
}

export interface CompanyAddress {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string;
  pais: string;
}

export interface ContractData {
  numeroContrato?: string;
  dataContrato?: string;
  juntaComercial?: string;
  nire?: string;
  capitalSocial?: number;
  atividadePrincipal?: string;
  atividadesSecundarias: string[];
}

export enum CompanyStatus {
  PendingDocuments = 'PendingDocuments',
  UnderReview = 'UnderReview',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Active = 'Active',
  Suspended = 'Suspended',
  Inactive = 'Inactive'
}

export interface CreateCompanyRequest {
  company: CompanyData;
  applicant: ApplicantData;
  legalRepresentatives: LegalRepresentativeData[];
}

export interface CompanyData {
  razaoSocial: string;
  nomeFantasia?: string;
  cnpj: string;
  inscricaoEstadual?: string;
  inscricaoMunicipal?: string;
  address: CompanyAddress;
  telefone?: string;
  email?: string;
  website?: string;
  contractData: ContractData;
  observacoes?: string;
}

export interface ApplicantData {
  nomeCompleto: string;
  cpf: string;
  rg?: string;
  orgaoExpedidor?: string;
  dataNascimento?: string;
  estadoCivil?: string;
  nacionalidade?: string;
  profissao?: string;
  email?: string;
  telefone?: string;
  celular?: string;
  address: ApplicantAddress;
  rendaMensal?: number;
  cargo?: string;
  isMainRepresentative: boolean;
}

export interface ApplicantAddress {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string;
  pais: string;
}

export interface LegalRepresentativeData {
  nomeCompleto: string;
  cpf: string;
  rg?: string;
  orgaoExpedidor?: string;
  dataNascimento?: string;
  estadoCivil?: string;
  nacionalidade?: string;
  profissao?: string;
  email?: string;
  telefone?: string;
  celular?: string;
  address: RepresentativeAddress;
  cargo: string;
  type: RepresentationType;
  percentualParticipacao?: number;
  poderesRepresentacao?: string;
  podeAssinarSozinho: boolean;
  limiteAlcada?: number;
}

export interface RepresentativeAddress {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  estado: string;
  pais: string;
}

export enum RepresentationType {
  Administrator = 'Administrator',
  PartnerAdministrator = 'PartnerAdministrator',
  Director = 'Director',
  President = 'President',
  VicePresident = 'VicePresident',
  Attorney = 'Attorney',
  Partner = 'Partner',
  Other = 'Other'
}

export interface LegalRepresentative {
  id: string;
  companyId: string;
  nomeCompleto: string;
  cpf: string;
  rg?: string;
  orgaoExpedidor?: string;
  dataNascimento?: string;
  estadoCivil?: string;
  nacionalidade?: string;
  profissao?: string;
  email?: string;
  telefone?: string;
  celular?: string;
  address: RepresentativeAddress;
  cargo: string;
  type: RepresentationType;
  percentualParticipacao?: number;
  poderesRepresentacao?: string;
  podeAssinarSozinho: boolean;
  limiteAlcada?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export const companyService = {
  getCompanies: (params?: {
    page?: number;
    limit?: number;
    search?: string;
    status?: CompanyStatus;
  }): Promise<AxiosResponse<{
    companies: Company[];
    total: number;
    page: number;
    limit: number;
    totalPages: number;
  }>> =>
    api.get('/admin/companies', { params }),

  getCompanyById: (id: string): Promise<AxiosResponse<Company>> =>
    api.get(`/admin/companies/${id}`),

  createCompany: (data: CreateCompanyRequest): Promise<AxiosResponse<Company>> => {
    console.log('🚀 Criando empresa:', data);
    return api.post('/admin/companies', data);
  },

  updateCompany: (id: string, data: CompanyData): Promise<AxiosResponse<Company>> => {
    console.log('✏️ Atualizando empresa:', id, data);
    return api.put(`/admin/companies/${id}`, data);
  },

  updateCompanyStatus: (id: string, status: CompanyStatus, observacoes?: string): Promise<AxiosResponse<Company>> => {
    console.log('📊 Atualizando status da empresa:', id, status);
    return api.patch(`/admin/companies/${id}/status`, { status, observacoes });
  },

  deleteCompany: (id: string): Promise<AxiosResponse<void>> =>
    api.delete(`/admin/companies/${id}`),

  // Representantes Legais
  getRepresentatives: (companyId: string): Promise<AxiosResponse<LegalRepresentative[]>> =>
    api.get(`/admin/companies/${companyId}/representatives`),

  getRepresentativeById: (companyId: string, representativeId: string): Promise<AxiosResponse<LegalRepresentative>> =>
    api.get(`/admin/companies/${companyId}/representatives/${representativeId}`),

  createRepresentative: (companyId: string, data: LegalRepresentativeData): Promise<AxiosResponse<LegalRepresentative>> => {
    console.log('🚀 Criando representante legal:', companyId, data);
    return api.post(`/admin/companies/${companyId}/representatives`, data);
  },

  updateRepresentative: (companyId: string, representativeId: string, data: LegalRepresentativeData): Promise<AxiosResponse<LegalRepresentative>> => {
    console.log('✏️ Atualizando representante legal:', companyId, representativeId, data);
    return api.put(`/admin/companies/${companyId}/representatives/${representativeId}`, data);
  },

  deleteRepresentative: (companyId: string, representativeId: string): Promise<AxiosResponse<void>> =>
    api.delete(`/admin/companies/${companyId}/representatives/${representativeId}`),
};

export const bankAccountService = {
  getAccounts: (clienteId?: string): Promise<AxiosResponse<BankAccount[]>> =>
    api.get(`/admin/contas${clienteId ? `?clienteId=${clienteId}` : ''}`),
  
  getAccountsByClient: (clienteId: string): Promise<AxiosResponse<BankAccount[]>> =>
    api.get(`/admin/contas/${clienteId}`),
  
  createAccount: (data: CreateBankAccountRequest): Promise<AxiosResponse<BankAccount>> =>
    api.post('/admin/contas', data),
  
  updateAccount: (id: string, data: Partial<CreateBankAccountRequest>): Promise<AxiosResponse<BankAccount>> =>
    api.put(`/admin/contas/${id}`, data),
  
  deleteAccount: (id: string): Promise<AxiosResponse<void>> =>
    api.delete(`/admin/contas/${id}`),
};

export const configService = {
  getPriorityConfig: (clienteId: string): Promise<AxiosResponse<PriorityConfig>> =>
    api.get(`/admin/configs/roteamento/${clienteId}`),
  
  updatePriorityConfig: (clienteId: string, data: Omit<PriorityConfig, 'clienteId' | 'totalPercentual' | 'isValid' | 'updatedAt'>): Promise<AxiosResponse<PriorityConfig>> =>
    api.put(`/admin/configs/roteamento/${clienteId}`, data),
};

export const transactionService = {
  getTransactions: (params?: {
    type?: 'cashin' | 'cashout';
    clienteId?: string;
    contaId?: string;
    status?: string;
    startDate?: string;
    endDate?: string;
    page?: number;
    limit?: number;
  }): Promise<AxiosResponse<{ transactions: Transaction[]; total: number; page: number; limit: number }>> =>
    api.get('/admin/transacoes/historico', { params }),
  
  getTransactionStatus: (id: string): Promise<AxiosResponse<{ status: string; details: any }>> =>
    api.get(`/transacoes/${id}/status`),
};

export const reportService = {
  getDashboardReport: (): Promise<AxiosResponse<DashboardReport>> =>
    api.get('/admin/transacoes/report'),
  
  getExtrato: (clienteId: string, params?: {
    contaId?: string;
    startDate?: string;
    endDate?: string;
  }): Promise<AxiosResponse<ExtratoResponse>> =>
    api.get(`/admin/extrato/${clienteId}`, { params }),
};

export const accessService = {
  getAccesses: (userId?: string): Promise<AxiosResponse<AccessControl[]>> =>
    api.get(`/admin/acessos${userId ? `/${userId}` : ''}`),

  createAccess: (data: CreateAccessRequest): Promise<AxiosResponse<AccessControl>> =>
    api.post('/admin/acessos', data),

  updateAccess: (id: string, data: Partial<CreateAccessRequest>): Promise<AxiosResponse<AccessControl>> =>
    api.put(`/admin/acessos/${id}`, data),

  deleteAccess: (id: string): Promise<AxiosResponse<void>> =>
    api.delete(`/admin/acessos/${id}`),
};

// Novos serviços para as páginas adicionais
export const systemConfigService = {
  getConfig: (): Promise<AxiosResponse<any>> =>
    api.get('/admin/config/system'),

  updateConfig: (data: any): Promise<AxiosResponse<any>> =>
    api.put('/admin/config/system', data),

  testSmtp: (data: any): Promise<AxiosResponse<any>> =>
    api.post('/admin/config/system/test-smtp', data),
};

export const integrationService = {
  getBankStatus: (): Promise<AxiosResponse<any[]>> =>
    api.get('/admin/integrations/banks/status'),

  testBankConnection: (bankId: string): Promise<AxiosResponse<any>> =>
    api.post(`/admin/integrations/banks/${bankId}/test`),

  getWebhooks: (): Promise<AxiosResponse<any[]>> =>
    api.get('/admin/integrations/webhooks'),

  createWebhook: (data: any): Promise<AxiosResponse<any>> =>
    api.post('/admin/integrations/webhooks', data),

  updateWebhook: (id: string, data: any): Promise<AxiosResponse<any>> =>
    api.put(`/admin/integrations/webhooks/${id}`, data),

  deleteWebhook: (id: string): Promise<AxiosResponse<void>> =>
    api.delete(`/admin/integrations/webhooks/${id}`),

  testWebhook: (id: string): Promise<AxiosResponse<any>> =>
    api.post(`/admin/integrations/webhooks/${id}/test`),

  getWebhookLogs: (webhookId: string): Promise<AxiosResponse<any[]>> =>
    api.get(`/admin/integrations/webhooks/${webhookId}/logs`),
};

export const auditService = {
  getUserActivity: (params?: any): Promise<AxiosResponse<any[]>> =>
    api.get('/admin/audit/activity', { params }),

  getUserStats: (): Promise<AxiosResponse<any>> =>
    api.get('/admin/audit/users/stats'),

  bulkUserAction: (action: string, userIds: string[]): Promise<AxiosResponse<any>> =>
    api.post('/admin/audit/users/bulk', { action, userIds }),
};

export default api;
