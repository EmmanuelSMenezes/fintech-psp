'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';

/**
 * Hook para validar tokens e fazer logout automático se inválido
 */
export const useTokenValidator = () => {
  const router = useRouter();

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

  // Função para limpar dados inválidos e fazer logout
  const clearInvalidAuth = () => {
    console.log('🧹 [TokenValidator] Limpando dados de autenticação inválidos...');
    localStorage.removeItem('access_token');
    localStorage.removeItem('user_data');
    toast.error('Token inválido detectado. Redirecionando para login...');
    router.push('/auth/signin');
  };

  // Verificar token periodicamente
  useEffect(() => {
    const validateToken = () => {
      const token = localStorage.getItem('access_token');
      
      if (token && !isValidToken(token)) {
        console.warn('⚠️ [TokenValidator] Token inválido detectado:', token.substring(0, 20) + '...');
        clearInvalidAuth();
      }
    };

    // Validar imediatamente
    validateToken();

    // Validar a cada 30 segundos
    const interval = setInterval(validateToken, 30000);

    return () => clearInterval(interval);
  }, [router]);

  return {
    isValidToken,
    clearInvalidAuth
  };
};
