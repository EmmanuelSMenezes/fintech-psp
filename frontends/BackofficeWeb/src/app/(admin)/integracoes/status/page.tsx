'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import toast from 'react-hot-toast';

interface BankStatus {
  id: string;
  name: string;
  status: 'online' | 'offline' | 'maintenance' | 'degraded';
  responseTime: number;
  uptime: number;
  lastCheck: string;
  endpoints: {
    name: string;
    url: string;
    status: 'online' | 'offline';
    responseTime: number;
  }[];
  transactions: {
    total: number;
    successful: number;
    failed: number;
    successRate: number;
  };
  environment: 'sandbox' | 'production';
}

const StatusIntegracaoPage: React.FC = () => {
  useRequireAuth('view_integrations');
  const { user } = useAuth();
  const [banksStatus, setBanksStatus] = useState<BankStatus[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [lastUpdate, setLastUpdate] = useState<string>('');

  const mockBanksStatus: BankStatus[] = [
    {
      id: 'stark_bank',
      name: 'Stark Bank',
      status: 'online',
      responseTime: 245,
      uptime: 99.8,
      lastCheck: new Date().toISOString(),
      endpoints: [
        { name: 'PIX', url: '/pix', status: 'online', responseTime: 180 },
        { name: 'TED', url: '/ted', status: 'online', responseTime: 320 },
        { name: 'Boleto', url: '/boleto', status: 'online', responseTime: 150 },
        { name: 'Auth', url: '/auth', status: 'online', responseTime: 95 }
      ],
      transactions: {
        total: 1250,
        successful: 1238,
        failed: 12,
        successRate: 99.04
      },
      environment: 'production'
    },
    {
      id: 'sicoob',
      name: 'Sicoob',
      status: 'degraded',
      responseTime: 1200,
      uptime: 97.2,
      lastCheck: new Date().toISOString(),
      endpoints: [
        { name: 'PIX', url: '/pix', status: 'online', responseTime: 890 },
        { name: 'TED', url: '/ted', status: 'offline', responseTime: 0 },
        { name: 'Boleto', url: '/boleto', status: 'online', responseTime: 1500 },
        { name: 'Auth', url: '/auth', status: 'online', responseTime: 450 }
      ],
      transactions: {
        total: 850,
        successful: 798,
        failed: 52,
        successRate: 93.88
      },
      environment: 'production'
    },
    {
      id: 'banco_genial',
      name: 'Banco Genial',
      status: 'online',
      responseTime: 380,
      uptime: 99.5,
      lastCheck: new Date().toISOString(),
      endpoints: [
        { name: 'PIX', url: '/pix', status: 'online', responseTime: 290 },
        { name: 'TED', url: '/ted', status: 'online', responseTime: 420 },
        { name: 'Boleto', url: '/boleto', status: 'online', responseTime: 350 },
        { name: 'Auth', url: '/auth', status: 'online', responseTime: 180 }
      ],
      transactions: {
        total: 650,
        successful: 642,
        failed: 8,
        successRate: 98.77
      },
      environment: 'production'
    },
    {
      id: 'efi',
      name: 'Efí (Gerencianet)',
      status: 'maintenance',
      responseTime: 0,
      uptime: 95.1,
      lastCheck: new Date().toISOString(),
      endpoints: [
        { name: 'PIX', url: '/pix', status: 'offline', responseTime: 0 },
        { name: 'TED', url: '/ted', status: 'offline', responseTime: 0 },
        { name: 'Boleto', url: '/boleto', status: 'offline', responseTime: 0 },
        { name: 'Auth', url: '/auth', status: 'offline', responseTime: 0 }
      ],
      transactions: {
        total: 420,
        successful: 398,
        failed: 22,
        successRate: 94.76
      },
      environment: 'production'
    },
    {
      id: 'celcoin',
      name: 'Celcoin',
      status: 'online',
      responseTime: 520,
      uptime: 98.9,
      lastCheck: new Date().toISOString(),
      endpoints: [
        { name: 'PIX', url: '/pix', status: 'online', responseTime: 480 },
        { name: 'TED', url: '/ted', status: 'online', responseTime: 650 },
        { name: 'Boleto', url: '/boleto', status: 'online', responseTime: 420 },
        { name: 'Auth', url: '/auth', status: 'online', responseTime: 280 }
      ],
      transactions: {
        total: 380,
        successful: 371,
        failed: 9,
        successRate: 97.63
      },
      environment: 'production'
    }
  ];

  useEffect(() => {
    loadBanksStatus();
    const interval = setInterval(loadBanksStatus, 30000); // Atualizar a cada 30 segundos
    return () => clearInterval(interval);
  }, []);

  const loadBanksStatus = async () => {
    try {
      setIsLoading(true);
      
      // Simular delay de API
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // Simular variações nos dados
      const updatedStatus = mockBanksStatus.map(bank => ({
        ...bank,
        responseTime: bank.status === 'online' ? bank.responseTime + Math.floor(Math.random() * 100) - 50 : 0,
        lastCheck: new Date().toISOString(),
        transactions: {
          ...bank.transactions,
          total: bank.transactions.total + Math.floor(Math.random() * 10)
        }
      }));
      
      setBanksStatus(updatedStatus);
      setLastUpdate(new Date().toLocaleString('pt-BR'));
    } catch (error) {
      console.error('Erro ao carregar status dos bancos:', error);
      toast.error('Erro ao carregar status dos bancos');
    } finally {
      setIsLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'online':
        return 'bg-green-100 text-green-800';
      case 'offline':
        return 'bg-red-100 text-red-800';
      case 'maintenance':
        return 'bg-yellow-100 text-yellow-800';
      case 'degraded':
        return 'bg-orange-100 text-orange-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'online':
        return 'Online';
      case 'offline':
        return 'Offline';
      case 'maintenance':
        return 'Manutenção';
      case 'degraded':
        return 'Degradado';
      default:
        return 'Desconhecido';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'online':
        return (
          <div className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
        );
      case 'offline':
        return (
          <div className="w-3 h-3 bg-red-500 rounded-full"></div>
        );
      case 'maintenance':
        return (
          <div className="w-3 h-3 bg-yellow-500 rounded-full animate-pulse"></div>
        );
      case 'degraded':
        return (
          <div className="w-3 h-3 bg-orange-500 rounded-full animate-pulse"></div>
        );
      default:
        return (
          <div className="w-3 h-3 bg-gray-500 rounded-full"></div>
        );
    }
  };

  const getResponseTimeColor = (responseTime: number) => {
    if (responseTime === 0) return 'text-gray-500';
    if (responseTime < 300) return 'text-green-600';
    if (responseTime < 800) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getUptimeColor = (uptime: number) => {
    if (uptime >= 99) return 'text-green-600';
    if (uptime >= 95) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getSuccessRateColor = (successRate: number) => {
    if (successRate >= 98) return 'text-green-600';
    if (successRate >= 95) return 'text-yellow-600';
    return 'text-red-600';
  };

  const overallStatus = banksStatus.length > 0 ? {
    online: banksStatus.filter(b => b.status === 'online').length,
    offline: banksStatus.filter(b => b.status === 'offline').length,
    maintenance: banksStatus.filter(b => b.status === 'maintenance').length,
    degraded: banksStatus.filter(b => b.status === 'degraded').length,
    avgResponseTime: banksStatus.filter(b => b.responseTime > 0).reduce((sum, b) => sum + b.responseTime, 0) / banksStatus.filter(b => b.responseTime > 0).length || 0,
    avgUptime: banksStatus.reduce((sum, b) => sum + b.uptime, 0) / banksStatus.length || 0
  } : null;

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Status das Integrações</h1>
          <p className="text-gray-600 mt-1">Monitoramento em tempo real dos bancos integrados</p>
          {lastUpdate && (
            <p className="text-sm text-gray-500 mt-1">Última atualização: {lastUpdate}</p>
          )}
        </div>
        <button
          onClick={loadBanksStatus}
          disabled={isLoading}
          className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white px-4 py-2 rounded-lg flex items-center space-x-2"
        >
          {isLoading ? (
            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
          ) : (
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          )}
          <span>Atualizar</span>
        </button>
      </div>

      {/* Overall Status */}
      {overallStatus && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-green-100 rounded-lg">
                <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Bancos Online</p>
                <p className="text-2xl font-bold text-gray-900">{overallStatus.online}/{banksStatus.length}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-blue-100 rounded-lg">
                <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Tempo Resposta Médio</p>
                <p className={`text-2xl font-bold ${getResponseTimeColor(overallStatus.avgResponseTime)}`}>
                  {Math.round(overallStatus.avgResponseTime)}ms
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-purple-100 rounded-lg">
                <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Uptime Médio</p>
                <p className={`text-2xl font-bold ${getUptimeColor(overallStatus.avgUptime)}`}>
                  {overallStatus.avgUptime.toFixed(1)}%
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-yellow-100 rounded-lg">
                <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Problemas</p>
                <p className="text-2xl font-bold text-gray-900">
                  {overallStatus.offline + overallStatus.maintenance + overallStatus.degraded}
                </p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Banks Status */}
      <div className="space-y-6">
        {banksStatus.map((bank) => (
          <div key={bank.id} className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
              <div className="flex items-center space-x-3">
                {getStatusIcon(bank.status)}
                <h3 className="text-lg font-semibold text-gray-900">{bank.name}</h3>
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(bank.status)}`}>
                  {getStatusLabel(bank.status)}
                </span>
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  {bank.environment}
                </span>
              </div>
              <div className="text-right">
                <div className="text-sm text-gray-500">Última verificação</div>
                <div className="text-sm font-medium text-gray-900">
                  {new Date(bank.lastCheck).toLocaleTimeString('pt-BR')}
                </div>
              </div>
            </div>

            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
                <div className="text-center">
                  <div className={`text-2xl font-bold ${getResponseTimeColor(bank.responseTime)}`}>
                    {bank.responseTime > 0 ? `${bank.responseTime}ms` : 'N/A'}
                  </div>
                  <div className="text-sm text-gray-600">Tempo de Resposta</div>
                </div>
                <div className="text-center">
                  <div className={`text-2xl font-bold ${getUptimeColor(bank.uptime)}`}>
                    {bank.uptime.toFixed(1)}%
                  </div>
                  <div className="text-sm text-gray-600">Uptime</div>
                </div>
                <div className="text-center">
                  <div className={`text-2xl font-bold ${getSuccessRateColor(bank.transactions.successRate)}`}>
                    {bank.transactions.successRate.toFixed(1)}%
                  </div>
                  <div className="text-sm text-gray-600">Taxa de Sucesso</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-blue-600">
                    {bank.transactions.total}
                  </div>
                  <div className="text-sm text-gray-600">Transações (24h)</div>
                </div>
              </div>

              <div>
                <h4 className="text-md font-medium text-gray-900 mb-3">Endpoints</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  {bank.endpoints.map((endpoint) => (
                    <div key={endpoint.name} className="border border-gray-200 rounded-lg p-3">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-900">{endpoint.name}</span>
                        {getStatusIcon(endpoint.status)}
                      </div>
                      <div className="text-xs text-gray-500 mb-1">{endpoint.url}</div>
                      <div className={`text-sm font-medium ${getResponseTimeColor(endpoint.responseTime)}`}>
                        {endpoint.responseTime > 0 ? `${endpoint.responseTime}ms` : 'Offline'}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Empty State */}
      {banksStatus.length === 0 && !isLoading && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="text-gray-400 mb-4">
            <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Nenhuma integração encontrada
          </h3>
          <p className="text-gray-500">
            Configure as integrações bancárias para monitorar o status
          </p>
        </div>
      )}
    </div>
  );
};

export default StatusIntegracaoPage;
