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

// Interceptor para adicionar token JWT automaticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('access_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para tratar respostas e erros
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado ou inválido
      // localStorage.removeItem('access_token');
      // localStorage.removeItem('user_data');
      // window.location.href = '/auth/signin';
    }
    return Promise.reject(error);
  }
);

// Tipos para autenticação
export interface LoginRequest {
  grant_type: 'client_credentials';
  client_id: string;
  client_secret: string;
  scope: string;
}

export interface LoginResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  scope: string;
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

// Serviços da API
export const authService = {
  login: (data: LoginRequest): Promise<AxiosResponse<LoginResponse>> =>
    api.post('/auth/token', data),
  
  logout: () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('user_data');
  },
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
    contaId?: string;
  }): Promise<AxiosResponse<Transaction>> =>
    api.post('/banking/transacoes/ted', data),
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

export default api;
