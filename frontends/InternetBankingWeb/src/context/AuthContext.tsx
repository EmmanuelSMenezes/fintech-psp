'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { authService, LoginRequest, LoginResponse } from '@/services/api';
import toast from 'react-hot-toast';

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

  // Verificar se há token salvo no localStorage ao inicializar
  useEffect(() => {
    const token = localStorage.getItem('access_token');
    const userData = localStorage.getItem('user_data');
    
    if (token && userData) {
      try {
        const parsedUser = JSON.parse(userData);
        setUser(parsedUser);
      } catch (error) {
        console.error('Erro ao parsear dados do usuário:', error);
        localStorage.removeItem('access_token');
        localStorage.removeItem('user_data');
      }
    }
    
    setIsLoading(false);
  }, []);

  const login = async (credentials: Omit<LoginRequest, 'grant_type'>): Promise<boolean> => {
    try {
      setIsLoading(true);
      
      const loginData: LoginRequest = {
        grant_type: 'client_credentials',
        ...credentials,
      };

      const response = await authService.login(loginData);
      const { access_token, scope } = response.data;

      // Decodificar o JWT para extrair informações do usuário (simulado)
      // Em produção, você pode decodificar o JWT real ou fazer uma chamada para /me
      const userData: User = {
        id: credentials.client_id,
        email: credentials.client_id + '@fintech.com',
        role: credentials.scope === 'banking' ? 'cliente' : 'sub-cliente',
        permissions: getPermissionsByScope(credentials.scope),
        scope: scope || credentials.scope,
      };

      // Salvar token e dados do usuário
      localStorage.setItem('access_token', access_token);
      localStorage.setItem('user_data', JSON.stringify(userData));
      
      setUser(userData);
      toast.success('Login realizado com sucesso!');
      
      return true;
    } catch (error: any) {
      console.error('Erro no login:', error);
      toast.error(error.response?.data?.error_description || 'Erro ao fazer login');
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
    if (!user) return false;
    return user.permissions.includes(permission) || user.role === 'admin';
  };

  const hasRole = (role: string): boolean => {
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

// Função auxiliar para mapear scopes para permissões
function getPermissionsByScope(scope: string): string[] {
  const permissionMap: Record<string, string[]> = {
    banking: [
      'view_dashboard',
      'view_transacoes',
      'view_contas',
      'view_extratos',
      'edit_contas',
      'configurar_priorizacao',
      'manage_sub_users',
      'create_transactions',
      'view_historico',
    ],
    'sub-banking': [
      'view_dashboard',
      'view_transacoes',
      'view_contas',
      'view_extratos',
      'view_historico',
    ],
  };

  return permissionMap[scope] || [];
}

// Hook para proteção de rotas
export const useRequireAuth = (requiredPermission?: string) => {
  const { isAuthenticated, hasPermission, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      window.location.href = '/auth/signin';
    }
  }, [isAuthenticated, isLoading]);

  useEffect(() => {
    if (requiredPermission && !isLoading && isAuthenticated && !hasPermission(requiredPermission)) {
      toast.error('Você não tem permissão para acessar esta página');
      window.location.href = '/';
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
