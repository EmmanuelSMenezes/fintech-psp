'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { authService, LoginRequest, LoginResponse } from '@/services/api';
import toast from 'react-hot-toast';
import { useRouter } from 'next/navigation';

interface User {
  id: string;
  email: string;
  role: string;
  permissions: string[];
  scope: string;
}

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (credentials: Omit<LoginRequest, 'grant_type'>) => Promise<boolean>;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Função para verificar se o token é válido (não temporário)
  const isValidToken = (token: string): boolean => {
    if (!token) return false;

    // Rejeitar tokens temporários
    if (token.startsWith('temp-master-token-')) {
      console.warn('⚠️ Token temporário detectado - removendo...');
      return false;
    }

    // Verificar se é um JWT válido (formato básico)
    const jwtPattern = /^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$/;
    if (!jwtPattern.test(token)) {
      console.warn('⚠️ Token não é um JWT válido - removendo...');
      return false;
    }

    return true;
  };

  // Função para limpar dados inválidos e fazer logout
  const clearInvalidAuth = () => {
    console.log('🧹 Limpando dados de autenticação inválidos...');
    localStorage.removeItem('internetbanking_access_token');
    localStorage.removeItem('internetbanking_user_data');
    setUser(null);
    toast.error('Token inválido detectado. Faça login novamente.');
  };

  // Verificar se há token salvo no localStorage ao inicializar
  useEffect(() => {
    const token = localStorage.getItem('internetbanking_access_token');
    const userData = localStorage.getItem('internetbanking_user_data');

    if (token && userData) {
      // Verificar se o token é válido
      if (!isValidToken(token)) {
        clearInvalidAuth();
        setIsLoading(false);
        return;
      }

      try {
        const parsedUser = JSON.parse(userData);
        // Garantir que permissions sempre seja um array
        if (!parsedUser.permissions || !Array.isArray(parsedUser.permissions)) {
          parsedUser.permissions = [];
        }
        console.log('✅ Token válido encontrado, usuário autenticado:', parsedUser.email);
        setUser(parsedUser);
      } catch (error) {
        console.error('❌ Erro ao parsear dados do usuário:', error);
        clearInvalidAuth();
      }
    }

    setIsLoading(false);
  }, []);

  const login = async (credentials: LoginRequest): Promise<boolean> => {
    try {
      setIsLoading(true);

      const response = await authService.login(credentials);
      const { accessToken, user: userInfo } = response.data;

      // Mapear dados do usuário para o formato interno
      const userData: User = {
        id: userInfo.id,
        email: userInfo.email,
        role: mapBackendRoleToFrontend(userInfo.role),
        permissions: getPermissionsByRole(userInfo.role, userInfo.isMaster),
        scope: userInfo.isMaster ? 'admin' : 'banking',
      };

      // Salvar token e dados do usuário
      localStorage.setItem('internetbanking_access_token', accessToken);
      localStorage.setItem('internetbanking_user_data', JSON.stringify(userData));

      setUser(userData);
      toast.success(`Bem-vindo, ${userInfo.name}!`);

      return true;
    } catch (error: any) {
      console.error('Erro no login:', error);

      // Verificar se é um erro de conexão com a API
      if (error.code === 'ECONNREFUSED' || error.message.includes('Network Error')) {
        console.error('❌ Erro de conexão com a API:', error);
        toast.error('Erro de conexão. Verifique se as APIs estão rodando.');
      } else {
        console.error('❌ Login falhou:', error);
        toast.error(error.response?.data?.message || 'Email ou senha inválidos');
      }
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    toast.success('Logout realizado com sucesso!');
  };

  const hasPermission = (permission: string): boolean => {
    // Se ainda está carregando, não permitir acesso
    if (isLoading) return false;

    // Se não há usuário, não permitir acesso
    if (!user) return false;

    // Se permissions não é um array válido, inicializar como array vazio
    if (!user.permissions || !Array.isArray(user.permissions)) {
      user.permissions = [];
    }

    // Verificar permissão ou se é admin
    return user.permissions.includes(permission) || user.role === 'admin_empresa';
  };

  const hasRole = (role: string): boolean => {
    // Se ainda está carregando, não permitir acesso
    if (isLoading) return false;

    // Se não há usuário, não permitir acesso
    if (!user) return false;

    return user.role === role;
  };

  const isAuthenticated = !!user;

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated,
    login,
    logout,
    hasPermission,
    hasRole,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
};

// Função para mapear roles do backend para frontend
function mapBackendRoleToFrontend(backendRole: string): string {
  switch (backendRole.toLowerCase()) {
    case 'admin':
    case 'master':
      return 'admin_empresa';
    case 'cliente':
      return 'admin_empresa';
    case 'sub-usuario':
    case 'sub_usuario':
      return 'sub-usuario';
    default:
      return 'sub-usuario';
  }
}

// Função para mapear roles para permissões
function getPermissionsByRole(role: string, isMaster: boolean = false): string[] {
  const basePermissions = [
    'view_tela_dashboard',
    'view_tela_contas',
    'view_tela_transacoes',
    'view_tela_historico',
  ];

  // Se for master ou admin, tem todas as permissões
  if (isMaster || role.toLowerCase() === 'admin' || role.toLowerCase() === 'master') {
    return [
      ...basePermissions,
      'view_tela_priorizacao',
      'view_tela_acessos',
      'view_tela_configuracao',
      'transacionar_pix',
      'transacionar_ted',
      'transacionar_boleto',
      'transacionar_cripto',
      'gerenciar_contas',
      'gerenciar_usuarios',
      'exportar_relatorios',
    ];
  }

  // Cliente normal tem quase todas as permissões
  if (role.toLowerCase() === 'cliente') {
    return [
      ...basePermissions,
      'view_tela_priorizacao',
      'view_tela_acessos',
      'view_tela_configuracao',
      'transacionar_pix',
      'transacionar_ted',
      'transacionar_boleto',
      'transacionar_cripto',
      'gerenciar_contas',
      'gerenciar_usuarios',
      'exportar_relatorios',
    ];
  }

  // Sub-usuário tem permissões limitadas
  return [
    ...basePermissions,
    'transacionar_pix', // Sub-usuários só podem fazer PIX
    'exportar_relatorios',
  ];
}

// Hook para proteção de rotas
export const useRequireAuth = (requiredPermission?: string) => {
  const { isAuthenticated, hasPermission, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      console.log('User not authenticated, redirecting to signin');
      window.location.href = '/auth/signin';
    }
  }, [isAuthenticated, isLoading]);

  useEffect(() => {
    if (requiredPermission && !isLoading && isAuthenticated && !hasPermission(requiredPermission)) {
      console.log(`User lacks permission: ${requiredPermission}`);
      toast.error('Você não tem permissão para acessar esta página');
      // Usar setTimeout para evitar loops
      setTimeout(() => {
        window.location.href = '/dashboard';
      }, 1000);
    }
  }, [requiredPermission, hasPermission, isLoading, isAuthenticated]);

  return { isAuthenticated, hasPermission, isLoading };
};

// Componente para proteção de elementos baseado em permissões
interface ProtectedElementProps {
  permission?: string;
  role?: string;
  children: ReactNode;
  fallback?: ReactNode;
}

export const ProtectedElement: React.FC<ProtectedElementProps> = ({
  permission,
  role,
  children,
  fallback = null,
}) => {
  const { hasPermission, hasRole } = useAuth();

  const hasAccess = () => {
    if (permission && !hasPermission(permission)) return false;
    if (role && !hasRole(role)) return false;
    return true;
  };

  return hasAccess() ? <>{children}</> : <>{fallback}</>;
};

export default AuthContext;
