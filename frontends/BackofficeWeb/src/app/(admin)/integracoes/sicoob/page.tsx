'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import toast from 'react-hot-toast';
import LoadingSpinner from '@/components/LoadingSpinner';

interface ConnectivityTest {
  name: string;
  success: boolean;
  message: string;
}

interface ConnectivityStatus {
  status: string;
  tests: ConnectivityTest[];
}

interface TokenCacheStatus {
  hasCachedToken: boolean;
  tokenPreview?: string;
  cacheKey: string;
  expiryMinutes: number;
}

interface CertificateStatus {
  isValid: boolean;
  expiryDate?: string;
  daysUntilExpiry: number;
  subject?: string;
  issuer?: string;
  thumbprint?: string;
  needsRenewal: boolean;
  errorMessage?: string;
}

interface ReconciliationStats {
  period: string;
  startDate: string;
  endDate: string;
  totalTransactions: number;
  reconciledCount: number;
  divergentCount: number;
  missingInBankCount: number;
  missingInInternalCount: number;
  reconciliationRate: number;
  totalAmount: number;
  divergentAmount: number;
}

const SicoobIntegrationPage: React.FC = () => {
  useRequireAuth('manage_integrations');
  const { user } = useAuth();

  const [isLoading, setIsLoading] = useState(true);
  const [connectivity, setConnectivity] = useState<ConnectivityStatus | null>(null);
  const [tokenCache, setTokenCache] = useState<TokenCacheStatus | null>(null);
  const [certificates, setCertificates] = useState<CertificateStatus | null>(null);
  const [reconciliation, setReconciliation] = useState<ReconciliationStats | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);

  useEffect(() => {
    loadIntegrationStatus();
  }, []);

  const loadIntegrationStatus = async () => {
    try {
      setIsLoading(true);
      
      // Carregar status de conectividade
      const connectivityResponse = await fetch('/api/integrations/sicoob/test-connectivity');
      if (connectivityResponse.ok) {
        const connectivityData = await connectivityResponse.json();
        setConnectivity(connectivityData);
      }

      // Carregar status do cache de tokens
      const tokenResponse = await fetch('/api/integrations/sicoob/token/cache/status');
      if (tokenResponse.ok) {
        const tokenData = await tokenResponse.json();
        setTokenCache(tokenData);
      }

      // Carregar status dos certificados
      const certResponse = await fetch('/api/integrations/sicoob/certificates/status');
      if (certResponse.ok) {
        const certData = await certResponse.json();
        setCertificates(certData);
      }

      // Carregar estatísticas de conciliação
      const reconciliationResponse = await fetch('/api/reconciliation/sicoob/stats?days=30');
      if (reconciliationResponse.ok) {
        const reconciliationData = await reconciliationResponse.json();
        setReconciliation(reconciliationData);
      }

    } catch (error) {
      console.error('Erro ao carregar status da integração:', error);
      toast.error('Erro ao carregar status da integração');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefreshToken = async () => {
    try {
      setIsRefreshing(true);
      const response = await fetch('/api/integrations/sicoob/token/refresh', {
        method: 'POST'
      });

      if (response.ok) {
        toast.success('Token renovado com sucesso!');
        await loadIntegrationStatus();
      } else {
        toast.error('Erro ao renovar token');
      }
    } catch (error) {
      console.error('Erro ao renovar token:', error);
      toast.error('Erro ao renovar token');
    } finally {
      setIsRefreshing(false);
    }
  };

  const handleRunReconciliation = async () => {
    try {
      setIsRefreshing(true);
      const response = await fetch('/api/reconciliation/sicoob/auto', {
        method: 'POST'
      });

      if (response.ok) {
        toast.success('Conciliação executada com sucesso!');
        await loadIntegrationStatus();
      } else {
        toast.error('Erro ao executar conciliação');
      }
    } catch (error) {
      console.error('Erro ao executar conciliação:', error);
      toast.error('Erro ao executar conciliação');
    } finally {
      setIsRefreshing(false);
    }
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Integração Sicoob</h1>
          <p className="text-gray-600 mt-1">Monitoramento e controle da integração com o Sicoob</p>
        </div>
        <button
          onClick={loadIntegrationStatus}
          disabled={isRefreshing}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center space-x-2 disabled:opacity-50"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          <span>Atualizar Status</span>
        </button>
      </div>

      {/* Status Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Conectividade */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Conectividade</p>
              <p className={`text-2xl font-bold ${connectivity?.status === 'success' ? 'text-green-600' : 'text-red-600'}`}>
                {connectivity?.status === 'success' ? 'Online' : 'Offline'}
              </p>
            </div>
            <div className={`p-3 rounded-full ${connectivity?.status === 'success' ? 'bg-green-100' : 'bg-red-100'}`}>
              <svg className={`w-6 h-6 ${connectivity?.status === 'success' ? 'text-green-600' : 'text-red-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Cache de Tokens */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Cache Token</p>
              <p className={`text-2xl font-bold ${tokenCache?.hasCachedToken ? 'text-green-600' : 'text-yellow-600'}`}>
                {tokenCache?.hasCachedToken ? 'Ativo' : 'Vazio'}
              </p>
            </div>
            <div className={`p-3 rounded-full ${tokenCache?.hasCachedToken ? 'bg-green-100' : 'bg-yellow-100'}`}>
              <svg className={`w-6 h-6 ${tokenCache?.hasCachedToken ? 'text-green-600' : 'text-yellow-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Certificados */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Certificado</p>
              <p className={`text-2xl font-bold ${certificates?.isValid ? (certificates?.needsRenewal ? 'text-yellow-600' : 'text-green-600') : 'text-red-600'}`}>
                {certificates?.isValid ? (certificates?.needsRenewal ? 'Renovar' : 'Válido') : 'Inválido'}
              </p>
            </div>
            <div className={`p-3 rounded-full ${certificates?.isValid ? (certificates?.needsRenewal ? 'bg-yellow-100' : 'bg-green-100') : 'bg-red-100'}`}>
              <svg className={`w-6 h-6 ${certificates?.isValid ? (certificates?.needsRenewal ? 'text-yellow-600' : 'text-green-600') : 'text-red-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Conciliação */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Conciliação</p>
              <p className="text-2xl font-bold text-blue-600">
                {reconciliation?.reconciliationRate?.toFixed(1) || '0'}%
              </p>
            </div>
            <div className="p-3 rounded-full bg-blue-100">
              <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      {/* Detalhes da Conectividade */}
      {connectivity && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="px-6 py-4 border-b border-gray-100">
            <h3 className="text-lg font-semibold text-gray-900">Testes de Conectividade</h3>
          </div>
          <div className="p-6">
            <div className="space-y-4">
              {connectivity.tests.map((test, index) => (
                <div key={index} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center space-x-3">
                    <div className={`p-2 rounded-full ${test.success ? 'bg-green-100' : 'bg-red-100'}`}>
                      <svg className={`w-4 h-4 ${test.success ? 'text-green-600' : 'text-red-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={test.success ? "M5 13l4 4L19 7" : "M6 18L18 6M6 6l12 12"} />
                      </svg>
                    </div>
                    <div>
                      <p className="font-medium text-gray-900">{test.name}</p>
                      <p className="text-sm text-gray-600">{test.message}</p>
                    </div>
                  </div>
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full ${test.success ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                    {test.success ? 'OK' : 'ERRO'}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Ações Rápidas */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="px-6 py-4 border-b border-gray-100">
          <h3 className="text-lg font-semibold text-gray-900">Ações Rápidas</h3>
        </div>
        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button
              onClick={handleRefreshToken}
              disabled={isRefreshing}
              className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50 text-left"
            >
              <div className="flex items-center space-x-3">
                <div className="p-2 bg-blue-100 rounded-lg">
                  <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                  </svg>
                </div>
                <div>
                  <p className="font-medium text-gray-900">Renovar Token</p>
                  <p className="text-sm text-gray-600">Forçar renovação do token OAuth</p>
                </div>
              </div>
            </button>

            <button
              onClick={handleRunReconciliation}
              disabled={isRefreshing}
              className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50 text-left"
            >
              <div className="flex items-center space-x-3">
                <div className="p-2 bg-green-100 rounded-lg">
                  <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2-2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                </div>
                <div>
                  <p className="font-medium text-gray-900">Executar Conciliação</p>
                  <p className="text-sm text-gray-600">Conciliar últimos 30 dias</p>
                </div>
              </div>
            </button>

            <button
              onClick={loadIntegrationStatus}
              disabled={isRefreshing}
              className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-50 text-left"
            >
              <div className="flex items-center space-x-3">
                <div className="p-2 bg-yellow-100 rounded-lg">
                  <svg className="w-5 h-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                </div>
                <div>
                  <p className="font-medium text-gray-900">Atualizar Status</p>
                  <p className="text-sm text-gray-600">Verificar todos os componentes</p>
                </div>
              </div>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SicoobIntegrationPage;
