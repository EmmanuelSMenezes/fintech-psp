'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { authService, LoginRequest, LoginResponse } from '@/services/api';
import toast from 'react-hot-toast';

interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  isMaster: boolean;
  // Campos opcionais para compatibilidade
  permissions?: string[];
  scope?: string;
}

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<boolean>;
  loginWithCredentials: (email: string, password: string) => Promise<void>;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [mounted, setMounted] = useState(false);

  console.log('🚀 AuthProvider renderizado');
  console.log('📊 Estado atual:', { user: !!user, isLoading, mounted });

  const isAuthenticated = !!user;

  // useEffect para restaurar usuário - execução única
  useEffect(() => {
    console.log('🔧 useEffect: Iniciando restauração...');

    const initAuth = async () => {
      try {
        console.log('🌐 Verificando se estamos no cliente...');
        if (typeof window !== 'undefined') {
          console.log('✅ Estamos no cliente, verificando localStorage...');
          const token = localStorage.getItem('backoffice_access_token');
          const userData = localStorage.getItem('backoffice_user_data');

          console.log('🎫 Token no localStorage:', token ? 'SIM' : 'NÃO');
          console.log('👤 UserData no localStorage:', userData ? 'SIM' : 'NÃO');

          if (token && userData) {
            try {
              const parsedUser = JSON.parse(userData);
              console.log('✅ RESTAURANDO USUÁRIO:', parsedUser);
              setUser(parsedUser);
            } catch (parseError) {
              console.error('❌ Erro ao fazer parse dos dados do usuário:', parseError);
              localStorage.removeItem('backoffice_access_token');
              localStorage.removeItem('backoffice_user_data');
            }
          } else {
            console.log('ℹ️ Nenhum usuário para restaurar');
          }
        } else {
          console.log('🖥️ Estamos no servidor, pulando localStorage');
        }
      } catch (error) {
        console.error('❌ Erro ao restaurar usuário:', error);
        // Limpar dados corrompidos
        if (typeof window !== 'undefined') {
          localStorage.removeItem('backoffice_access_token');
          localStorage.removeItem('backoffice_user_data');
        }
      } finally {
        console.log('🏁 Finalizando loading...');
        setIsLoading(false);
        setMounted(true);
      }
    };

    // Executar a inicialização
    initAuth();
  }, []); // Array vazio - executar apenas uma vez

  const login = async (credentials: LoginRequest): Promise<boolean> => {
    try {
      console.log('🔐 Iniciando login...');
      console.log('📧 Email:', credentials.email);
      setIsLoading(true);

      const response = await authService.login(credentials);

      console.log('📨 Resposta da API recebida!');
      console.log('📊 Status:', response.status);
      console.log('📊 StatusText:', response.statusText);
      console.log('📊 Dados recebidos:', response.data ? 'SIM' : 'NÃO');

      if (response.data) {
        // A API retorna accessToken e user (conforme LoginResponse.cs do backend)
        const { accessToken, user } = response.data;
        console.log('🎫 Token:', accessToken ? 'Presente' : 'Ausente');
        console.log('👤 User Info:', user ? 'Presente' : 'Ausente');

        // Log da estrutura completa para debug
        console.log('📋 Estrutura completa da resposta:', Object.keys(response.data));
        console.log('👤 Dados do usuário:', user);

        // Salvar no localStorage
        localStorage.setItem('backoffice_access_token', accessToken);
        localStorage.setItem('backoffice_user_data', JSON.stringify(user));

        // Verificar se salvou
        const savedToken = localStorage.getItem('backoffice_access_token');
        const savedUserData = localStorage.getItem('backoffice_user_data');
        console.log('🎫 Token salvo:', savedToken ? 'SIM' : 'NÃO');
        console.log('👤 UserData salvo:', savedUserData ? 'SIM' : 'NÃO');

        // Atualizar estado
        setUser(user);
        console.log('✅ Login concluído com sucesso!');

        toast.success('Login realizado com sucesso!');
        return true;
      }

      return false;
    } catch (error: any) {
      console.error('❌ Erro no login:', error);
      toast.error(error.response?.data?.error_description || 'Erro ao fazer login');
      return false;
    } finally {
      setIsLoading(false);
    }
  };

  const loginWithCredentials = async (email: string, password: string): Promise<void> => {
    const success = await login({ email, password });
    if (!success) {
      throw new Error('Falha no login');
    }
  };

  const logout = () => {
    console.log('🚪 Fazendo logout...');
    setUser(null);
    if (typeof window !== 'undefined') {
      localStorage.removeItem('backoffice_access_token');
      localStorage.removeItem('backoffice_user_data');
    }
    console.log('✅ Logout concluído');
    toast.success('Logout realizado com sucesso!');
  };

  const hasPermission = (permission: string): boolean => {
    if (!user || !user.permissions) return false;
    return user.permissions.includes(permission);
  };

  const hasRole = (role: string): boolean => {
    if (!user) return false;
    return user.role === role;
  };

  // Debug do AuthContext State
  if (typeof window !== 'undefined') {
    console.log('🔍 AuthContext State:', {
      user: user ? { id: user.id, email: user.email, role: user.role } : null,
      isLoading,
      isAuthenticated,
      hasToken: !!localStorage.getItem('backoffice_access_token')
    });
  }

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated,
    login,
    loginWithCredentials,
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
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Hook para verificar autenticação e permissões
export const useRequireAuth = (requiredPermission?: string) => {
  const { user, isLoading, hasPermission } = useAuth();

  const isAuthenticated = !!user;
  const hasRequiredPermission = requiredPermission ? hasPermission(requiredPermission) : true;

  console.log('🛡️ useRequireAuth:', {
    requiredPermission,
    isAuthenticated,
    isLoading,
    hasPermission: hasRequiredPermission
  });

  return {
    user,
    isLoading,
    isAuthenticated,
    hasPermission: hasRequiredPermission,
    shouldRedirect: !isLoading && (!isAuthenticated || !hasRequiredPermission)
  };
};
