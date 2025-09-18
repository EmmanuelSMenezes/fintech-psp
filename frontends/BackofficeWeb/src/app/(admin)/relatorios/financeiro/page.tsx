'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { reportService, transactionService } from '@/services/api';
import toast from 'react-hot-toast';

interface FinancialMetrics {
  totalVolume: number;
  totalTransactions: number;
  successRate: number;
  averageTicket: number;
  volumeByType: { [key: string]: number };
  transactionsByStatus: { [key: string]: number };
  volumeByBank: { [key: string]: number };
  dailyVolume: { date: string; volume: number }[];
  monthlyGrowth: number;
}

const RelatorioFinanceiroPage: React.FC = () => {
  useRequireAuth('view_reports');
  const { user } = useAuth();
  const [metrics, setMetrics] = useState<FinancialMetrics>({
    totalVolume: 0,
    totalTransactions: 0,
    successRate: 0,
    averageTicket: 0,
    volumeByType: {},
    transactionsByStatus: {},
    volumeByBank: {},
    dailyVolume: [],
    monthlyGrowth: 0
  });
  const [isLoading, setIsLoading] = useState(true);
  const [dateRange, setDateRange] = useState({
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0]
  });

  const transactionTypes = [
    { key: 'pix', label: 'PIX', color: 'bg-blue-500' },
    { key: 'ted', label: 'TED', color: 'bg-green-500' },
    { key: 'boleto', label: 'Boleto', color: 'bg-yellow-500' },
    { key: 'crypto', label: 'Crypto', color: 'bg-purple-500' }
  ];

  const banks = [
    { key: 'stark_bank', label: 'Stark Bank', color: 'bg-indigo-500' },
    { key: 'sicoob', label: 'Sicoob', color: 'bg-green-600' },
    { key: 'banco_genial', label: 'Banco Genial', color: 'bg-red-500' },
    { key: 'efi', label: 'Efí', color: 'bg-orange-500' },
    { key: 'celcoin', label: 'Celcoin', color: 'bg-teal-500' }
  ];

  useEffect(() => {
    loadFinancialData();
  }, [dateRange]);

  const loadFinancialData = async () => {
    try {
      setIsLoading(true);
      
      // Simular dados do relatório financeiro
      const mockMetrics: FinancialMetrics = {
        totalVolume: 15750000,
        totalTransactions: 8450,
        successRate: 97.8,
        averageTicket: 1864.50,
        volumeByType: {
          pix: 8500000,
          ted: 4200000,
          boleto: 2800000,
          crypto: 250000
        },
        transactionsByStatus: {
          completed: 8267,
          pending: 98,
          failed: 85
        },
        volumeByBank: {
          stark_bank: 6300000,
          sicoob: 3150000,
          banco_genial: 2835000,
          efi: 1890000,
          celcoin: 1575000
        },
        dailyVolume: Array.from({ length: 30 }, (_, i) => ({
          date: new Date(Date.now() - (29 - i) * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
          volume: Math.floor(Math.random() * 800000) + 200000
        })),
        monthlyGrowth: 12.5
      };

      setMetrics(mockMetrics);
    } catch (error) {
      console.error('Erro ao carregar dados financeiros:', error);
      toast.error('Erro ao carregar dados financeiros');
    } finally {
      setIsLoading(false);
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

  const formatNumber = (value: number) => {
    return new Intl.NumberFormat('pt-BR').format(value);
  };

  const getPercentage = (value: number, total: number) => {
    return total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Relatório Financeiro</h1>
          <p className="text-gray-600 mt-1">Análise detalhada do desempenho financeiro</p>
        </div>
        <div className="flex items-center space-x-4">
          <div className="flex items-center space-x-2">
            <input
              type="date"
              value={dateRange.startDate}
              onChange={(e) => setDateRange({...dateRange, startDate: e.target.value})}
              className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            />
            <span className="text-gray-500">até</span>
            <input
              type="date"
              value={dateRange.endDate}
              onChange={(e) => setDateRange({...dateRange, endDate: e.target.value})}
              className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button
            onClick={loadFinancialData}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg"
          >
            Atualizar
          </button>
        </div>
      </div>

      {/* Main Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-3 bg-blue-100 rounded-lg">
              <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Volume Total</p>
              <p className="text-2xl font-bold text-gray-900">{formatCurrency(metrics.totalVolume)}</p>
              <p className="text-xs text-green-600">+{metrics.monthlyGrowth}% este mês</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-3 bg-green-100 rounded-lg">
              <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total Transações</p>
              <p className="text-2xl font-bold text-gray-900">{formatNumber(metrics.totalTransactions)}</p>
              <p className="text-xs text-blue-600">Ticket médio: {formatCurrency(metrics.averageTicket)}</p>
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
              <p className="text-sm font-medium text-gray-600">Taxa de Sucesso</p>
              <p className="text-2xl font-bold text-gray-900">{metrics.successRate}%</p>
              <p className="text-xs text-purple-600">{formatNumber(metrics.transactionsByStatus.completed)} aprovadas</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-3 bg-yellow-100 rounded-lg">
              <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Crescimento</p>
              <p className="text-2xl font-bold text-gray-900">+{metrics.monthlyGrowth}%</p>
              <p className="text-xs text-yellow-600">Comparado ao mês anterior</p>
            </div>
          </div>
        </div>
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Volume by Type */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Volume por Tipo de Transação</h3>
          <div className="space-y-4">
            {transactionTypes.map((type) => {
              const volume = metrics.volumeByType[type.key] || 0;
              const percentage = getPercentage(volume, metrics.totalVolume);
              return (
                <div key={type.key} className="flex items-center justify-between">
                  <div className="flex items-center space-x-3">
                    <div className={`w-4 h-4 rounded ${type.color}`}></div>
                    <span className="text-sm font-medium text-gray-700">{type.label}</span>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-semibold text-gray-900">{formatCurrency(volume)}</div>
                    <div className="text-xs text-gray-500">{percentage}%</div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Volume by Bank */}
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Volume por Banco</h3>
          <div className="space-y-4">
            {banks.map((bank) => {
              const volume = metrics.volumeByBank[bank.key] || 0;
              const percentage = getPercentage(volume, metrics.totalVolume);
              return (
                <div key={bank.key} className="flex items-center justify-between">
                  <div className="flex items-center space-x-3">
                    <div className={`w-4 h-4 rounded ${bank.color}`}></div>
                    <span className="text-sm font-medium text-gray-700">{bank.label}</span>
                  </div>
                  <div className="text-right">
                    <div className="text-sm font-semibold text-gray-900">{formatCurrency(volume)}</div>
                    <div className="text-xs text-gray-500">{percentage}%</div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Transaction Status */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Status das Transações</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="text-center">
            <div className="text-3xl font-bold text-green-600">{formatNumber(metrics.transactionsByStatus.completed)}</div>
            <div className="text-sm text-gray-600">Concluídas</div>
            <div className="text-xs text-gray-500">{getPercentage(metrics.transactionsByStatus.completed, metrics.totalTransactions)}%</div>
          </div>
          <div className="text-center">
            <div className="text-3xl font-bold text-yellow-600">{formatNumber(metrics.transactionsByStatus.pending)}</div>
            <div className="text-sm text-gray-600">Pendentes</div>
            <div className="text-xs text-gray-500">{getPercentage(metrics.transactionsByStatus.pending, metrics.totalTransactions)}%</div>
          </div>
          <div className="text-center">
            <div className="text-3xl font-bold text-red-600">{formatNumber(metrics.transactionsByStatus.failed)}</div>
            <div className="text-sm text-gray-600">Falharam</div>
            <div className="text-xs text-gray-500">{getPercentage(metrics.transactionsByStatus.failed, metrics.totalTransactions)}%</div>
          </div>
        </div>
      </div>

      {/* Daily Volume Chart */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Volume Diário (Últimos 30 dias)</h3>
        <div className="h-64 flex items-end justify-between space-x-1">
          {metrics.dailyVolume.map((day, index) => {
            const maxVolume = Math.max(...metrics.dailyVolume.map(d => d.volume));
            const height = (day.volume / maxVolume) * 100;
            return (
              <div key={index} className="flex-1 flex flex-col items-center">
                <div
                  className="w-full bg-blue-500 rounded-t hover:bg-blue-600 transition-colors cursor-pointer"
                  style={{ height: `${height}%` }}
                  title={`${new Date(day.date).toLocaleDateString('pt-BR')}: ${formatCurrency(day.volume)}`}
                ></div>
                {index % 5 === 0 && (
                  <div className="text-xs text-gray-500 mt-2 transform -rotate-45">
                    {new Date(day.date).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default RelatorioFinanceiroPage;
