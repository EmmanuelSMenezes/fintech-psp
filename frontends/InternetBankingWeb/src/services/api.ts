import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

// Configuração base da API
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000';
const AUTH_SERVICE_URL = process.env.NEXT_PUBLIC_AUTH_SERVICE_URL || 'http://localhost:5001';

// Instância do axios
const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Função para verificar se o token é válido
const isValidToken = (token: string): boolean => {
  if (!token) return false;

  // Rejeitar tokens temporários
  if (token.startsWith('temp-master-token-')) {
    return false;
  }

  // Verificar se é um JWT válido (formato básico)
  const jwtPattern = /^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$/;
  return jwtPattern.test(token);
};

// Função para limpar dados inválidos
const clearInvalidAuth = () => {
  console.log('🧹 [InternetBanking] Limpando dados de autenticação inválidos...');
  localStorage.removeItem('internetbanking_access_token');
  localStorage.removeItem('internetbanking_user_data');
  // Recarregar a página para forçar logout
  window.location.href = '/auth/signin';
};

// Interceptor para adicionar token JWT automaticamente
api.interceptors.request.use(
  (config) => {
    console.log('🚀 [InternetBanking] Interceptor executado para:', config.method?.toUpperCase(), config.url);

    const token = localStorage.getItem('internetbanking_access_token');
    console.log('🔑 [InternetBanking] Token encontrado:', token ? 'SIM' : 'NÃO');

    if (token) {
      // Verificar se o token é válido antes de usar
      if (!isValidToken(token)) {
        console.error('❌ [InternetBanking] Token inválido detectado no interceptor!');
        clearInvalidAuth();
        return Promise.reject(new Error('Token inválido'));
      }

      config.headers.Authorization = `Bearer ${token}`;
      console.log('✅ [InternetBanking] Authorization header adicionado');
      console.log('🎫 [InternetBanking] Token JWT válido (primeiros 20 chars):', token.substring(0, 20) + '...');
    } else {
      console.log('❌ [InternetBanking] Nenhum token para adicionar');
    }

    return config;
  },
  (error) => {
    console.error('❌ [InternetBanking] Erro no interceptor de request:', error);
    return Promise.reject(error);
  }
);

// Interceptor para tratar respostas e erros
api.interceptors.response.use(
  (response: AxiosResponse) => {
    console.log('✅ [InternetBanking] Resposta recebida:', response.status, response.config.url);
    return response;
  },
  (error) => {
    console.error('❌ [InternetBanking] Erro na resposta:', error.response?.status, error.config?.url);

    if (error.response?.status === 401) {
      const token = localStorage.getItem('internetbanking_access_token');
      console.error('🚫 [InternetBanking] Token rejeitado (401):', token?.substring(0, 20) + '...');

      // Fazer logout automático em caso de 401
      console.log('🚪 [InternetBanking] Fazendo logout automático devido ao erro 401...');
      clearInvalidAuth();
    }

    return Promise.reject(error);
  }
);

// Tipos para autenticação
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
  email: string;
  role: string;
  permissions: string[];
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  role: string;
  permissions?: string[];
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

// Criar instância específica para AuthService (sem interceptor de token)
const authApiInstance = axios.create({
  baseURL: AUTH_SERVICE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Serviços da API
export const authService = {
  login: (data: LoginRequest): Promise<AxiosResponse<LoginResponse>> =>
    authApiInstance.post('/auth/login', data),

  logout: () => {
    localStorage.removeItem('internetbanking_access_token');
    localStorage.removeItem('internetbanking_user_data');
  },

  // Buscar dados do usuário atual
  getCurrentUser: (): Promise<AxiosResponse<User>> =>
    api.get('/client-users/me'),
};

export const userService = {
  getUsers: (): Promise<AxiosResponse<User[]>> =>
    api.get('/admin/users'),
  
  createUser: (data: CreateUserRequest): Promise<AxiosResponse<User>> =>
    api.post('/admin/users', data),
  
  updateUser: (id: string, data: Partial<CreateUserRequest>): Promise<AxiosResponse<User>> =>
    api.put(`/admin/users/${id}`, data),
  
  deleteUser: (id: string): Promise<AxiosResponse<void>> =>
    api.delete(`/admin/users/${id}`),
};

export const bankAccountService = {
  // Banking scope - usuário gerencia suas próprias contas
  getMyAccounts: (): Promise<AxiosResponse<BankAccount[]>> =>
    api.get('/banking/contas'),

  createMyAccount: (data: Omit<CreateBankAccountRequest, 'clienteId'>): Promise<AxiosResponse<BankAccount>> =>
    api.post('/banking/contas', data),

  updateMyAccount: (id: string, data: Partial<Omit<CreateBankAccountRequest, 'clienteId'>>): Promise<AxiosResponse<BankAccount>> =>
    api.put(`/banking/contas/${id}`, data),

  deleteMyAccount: (id: string): Promise<AxiosResponse<void>> =>
    api.delete(`/banking/contas/${id}`),
};

export const configService = {
  // Banking scope - usuário gerencia sua própria configuração
  getMyPriorityConfig: (): Promise<AxiosResponse<PriorityConfig>> =>
    api.get('/banking/configs/roteamento'),

  updateMyPriorityConfig: (data: Omit<PriorityConfig, 'clienteId' | 'totalPercentual' | 'isValid' | 'updatedAt'>): Promise<AxiosResponse<PriorityConfig>> =>
    api.put('/banking/configs/roteamento', data),

  getBanks: (): Promise<AxiosResponse<Array<{ code: string; name: string; isActive: boolean }>>> =>
    api.get('/banking/bancos'),
};

export const transactionService = {
  // Banking scope - usuário vê suas próprias transações
  getMyTransactions: (params?: {
    type?: 'cashin' | 'cashout';
    contaId?: string;
    status?: string;
    startDate?: string;
    endDate?: string;
    page?: number;
    limit?: number;
  }): Promise<AxiosResponse<{ transactions: Transaction[]; total: number; page: number; limit: number }>> =>
    api.get('/banking/transacoes/historico', { params }),

  getTransactionStatus: (id: string): Promise<AxiosResponse<{ status: string; details: any }>> =>
    api.get(`/transacoes/${id}/status`),

  // Criar transações
  createPixTransaction: (data: {
    externalId: string;
    amount: number;
    pixKey: string;
    description?: string;
    contaId?: string;
  }): Promise<AxiosResponse<Transaction>> =>
    api.post('/banking/transacoes/pix', data),

  createTedTransaction: (data: {
    externalId: string;
    amount: number;
    bankCode: string;
    accountBranch: string;
    accountNumber: string;
    taxId: string;
    name: string;
    description?: string;
    contaId?: string;
  }): Promise<AxiosResponse<Transaction>> =>
    api.post('/banking/transacoes/ted', data),

  createBoletoTransaction: (data: {
    externalId: string;
    amount: number;
    dueDate: string;
    payerTaxId: string;
    payerName: string;
    instructions?: string;
    contaId?: string;
  }): Promise<AxiosResponse<Transaction>> =>
    api.post('/banking/transacoes/boleto', data),

  createCryptoTransaction: (data: {
    externalId: string;
    amount: number;
    cryptoType: 'bitcoin' | 'ethereum' | 'usdt' | 'usdc';
    walletAddress: string;
    fiatCurrency?: string;
    description?: string;
    contaId?: string;
  }): Promise<AxiosResponse<Transaction>> =>
    api.post('/banking/transacoes/crypto', data),
};

export const reportService = {
  // Banking scope - relatórios do próprio usuário
  getMyDashboardReport: (): Promise<AxiosResponse<DashboardReport>> =>
    api.get('/banking/transacoes/report'),

  getMyExtrato: (params?: {
    contaId?: string;
    startDate?: string;
    endDate?: string;
  }): Promise<AxiosResponse<ExtratoResponse>> =>
    api.get('/banking/extrato', { params }),
};

export const accessService = {
  // Banking scope - gerenciar sub-usuários
  getMySubUsers: (): Promise<AxiosResponse<AccessControl[]>> =>
    api.get('/api/acessos/banking'),

  createSubUser: (data: CreateSubUserRequest): Promise<AxiosResponse<AccessControl>> =>
    api.post('/api/acessos/banking', data),

  updateSubUser: (id: string, data: Partial<CreateSubUserRequest>): Promise<AxiosResponse<AccessControl>> =>
    api.put(`/api/acessos/banking/${id}`, data),

  deleteSubUser: (id: string, motivo?: string): Promise<AxiosResponse<void>> =>
    api.delete(`/api/acessos/banking/${id}${motivo ? `?motivo=${encodeURIComponent(motivo)}` : ''}`),

  getAvailablePermissions: (): Promise<AxiosResponse<PermissionsResponse>> =>
    api.get('/api/acessos/permissions'),
};

// Interfaces para Configurações da Empresa
export interface CompanySettings {
  settingsId: string;
  companyId: string;

  // Informações da Empresa
  companyName: string;
  cnpj: string;
  email: string;
  phone: string;

  // Configurações de Segurança
  maxDailyTransactionAmount: number;
  maxSingleTransactionAmount: number;
  requireTwoFactorAuth: boolean;
  sessionTimeoutMinutes: number;

  // Configurações de Notificação
  emailNotifications: {
    transactionConfirmation: boolean;
    dailyReport: boolean;
    securityAlerts: boolean;
    systemMaintenance: boolean;
  };

  // Configurações SMTP
  smtpSettings: {
    enabled: boolean;
    host: string;
    port: number;
    username: string;
    password: string; // Será mascarado na resposta
    fromEmail: string;
    fromName: string;
    useTLS: boolean;
  };

  createdAt: string;
  updatedAt: string;
}

export interface UpdateCompanySettingsRequest {
  // Informações da Empresa
  companyName: string;
  cnpj: string;
  email: string;
  phone: string;

  // Configurações de Segurança
  maxDailyTransactionAmount: number;
  maxSingleTransactionAmount: number;
  requireTwoFactorAuth: boolean;
  sessionTimeoutMinutes: number;

  // Configurações de Notificação
  emailNotifications: {
    transactionConfirmation: boolean;
    dailyReport: boolean;
    securityAlerts: boolean;
    systemMaintenance: boolean;
  };

  // Configurações SMTP
  smtpSettings: {
    enabled: boolean;
    host: string;
    port: number;
    username: string;
    password: string;
    fromEmail: string;
    fromName: string;
    useTLS: boolean;
  };
}

export interface TestSmtpRequest {
  host: string;
  port: number;
  username: string;
  password: string;
  fromEmail: string;
  fromName: string;
  useTLS: boolean;
  testEmail: string;
}

// Serviço de Configurações da Empresa
export const companySettingsService = {
  // Obter configurações da empresa
  getSettings: (): Promise<AxiosResponse<CompanySettings>> =>
    api.get('/api/empresa/configuracoes'),

  // Atualizar configurações da empresa
  updateSettings: (data: UpdateCompanySettingsRequest): Promise<AxiosResponse<CompanySettings>> =>
    api.put('/api/empresa/configuracoes', data),

  // Testar configurações SMTP
  testSmtp: (data: TestSmtpRequest): Promise<AxiosResponse<{ success: boolean; message: string }>> =>
    api.post('/api/empresa/configuracoes/test-smtp', data),

  // Obter histórico de alterações
  getSettingsHistory: (): Promise<AxiosResponse<any[]>> =>
    api.get('/api/empresa/configuracoes/historico'),
};

// Interfaces para Priorização
export interface PrioritizationRule {
  contaId: string;
  percentage: number;
  isActive: boolean;
  accountName?: string;
  accountNumber?: string;
  bankCode?: string;
}

export interface CreatePrioritizationRequest {
  rules: PrioritizationRule[];
}

export interface PrioritizationConfiguration {
  configurationId: string;
  clienteId: string;
  rules: PrioritizationRule[];
  isActive: boolean;
  totalPercentage: number;
  isValid: boolean;
  createdAt: string;
  updatedAt?: string;
}

// Serviço de Priorização
export const prioritizationService = {
  // Banking scope - configuração de priorização do usuário
  getMyConfiguration: (): Promise<AxiosResponse<PrioritizationConfiguration>> =>
    api.get('/banking/priorizacao'),

  createConfiguration: (data: CreatePrioritizationRequest): Promise<AxiosResponse<PrioritizationConfiguration>> =>
    api.post('/banking/priorizacao', data),

  updateConfiguration: (data: CreatePrioritizationRequest): Promise<AxiosResponse<PrioritizationConfiguration>> =>
    api.put('/banking/priorizacao', data),

  deleteConfiguration: (): Promise<AxiosResponse<void>> =>
    api.delete('/banking/priorizacao'),

  validateConfiguration: (rules: PrioritizationRule[]): Promise<AxiosResponse<{ isValid: boolean; errors: string[]; totalPercentage: number }>> =>
    api.post('/banking/priorizacao/validate', { rules }),
};

export default api;
