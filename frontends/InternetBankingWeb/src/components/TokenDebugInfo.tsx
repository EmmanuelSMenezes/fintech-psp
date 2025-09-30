'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/context/AuthContext';

const TokenDebugInfo: React.FC = () => {
  const { user } = useAuth();
  const [tokenInfo, setTokenInfo] = useState<{
    token: string | null;
    isTemporary: boolean;
    userData: any;
  }>({
    token: null,
    isTemporary: false,
    userData: null,
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

  useEffect(() => {
    const token = localStorage.getItem('access_token');
    const userData = localStorage.getItem('user_data');

    setTokenInfo({
      token,
      isTemporary: token ? !isValidToken(token) : false,
      userData: userData ? JSON.parse(userData) : null,
    });
  }, [user]);

  // Só mostrar em desenvolvimento
  if (process.env.NODE_ENV !== 'development') {
    return null;
  }

  return (
    <div className="fixed bottom-4 right-4 bg-gray-900 text-white p-4 rounded-lg shadow-lg max-w-md text-xs font-mono z-50">
      <div className="flex items-center justify-between mb-2">
        <h3 className="font-bold text-yellow-400">🔍 Token Debug</h3>
        <button
          onClick={() => {
            localStorage.removeItem('access_token');
            localStorage.removeItem('user_data');
            window.location.reload();
          }}
          className="text-red-400 hover:text-red-300 text-xs"
        >
          Limpar
        </button>
      </div>
      
      <div className="space-y-2">
        <div>
          <span className="text-gray-400">Status:</span>{' '}
          {tokenInfo.isTemporary ? (
            <span className="text-red-400">❌ Token Inválido</span>
          ) : tokenInfo.token ? (
            <span className="text-green-400">✅ Token JWT Válido</span>
          ) : (
            <span className="text-yellow-400">⚠️ Sem Token</span>
          )}
        </div>
        
        {tokenInfo.token && (
          <div>
            <span className="text-gray-400">Token:</span>{' '}
            <span className="text-blue-400">
              {tokenInfo.token.substring(0, 20)}...
            </span>
          </div>
        )}
        
        {tokenInfo.userData && (
          <div>
            <span className="text-gray-400">Usuário:</span>{' '}
            <span className="text-green-400">{tokenInfo.userData.email}</span>
          </div>
        )}
        
        {tokenInfo.isTemporary && (
          <div className="mt-3 p-2 bg-red-900 rounded text-red-200">
            <div className="font-bold">⚠️ Token Inválido Detectado:</div>
            <div className="text-xs mt-1">
              Token inválido será removido automaticamente.
              <br />
              <strong>Faça login novamente com:</strong>
              <br />
              📧 admin@fintechpsp.com
              <br />
              🔑 admin123
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default TokenDebugInfo;
