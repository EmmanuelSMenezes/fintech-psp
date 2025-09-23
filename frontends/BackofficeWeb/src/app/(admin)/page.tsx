'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { reportService, userService, bankAccountService, transactionService } from '@/services/api';
import toast from 'react-hot-toast';

interface DashboardData {
  totalClientes: number;
  totalContas: number;
  totalTransacoes: number;
  volumeTransacoes: number;
  transacoesHoje: number;
  volumeHoje: number;
  taxaSucesso: number;
}

const DashboardPage: React.FC = () => {
  useRequireAuth('view_dashboard');
  const { user } = useAuth();
  const [isLoading, setIsLoading] = useState(true);
  const [dashboardData, setDashboardData] = useState<DashboardData>({
    totalClientes: 0,
    totalContas: 0,
    totalTransacoes: 0,
    volumeTransacoes: 0,
    transacoesHoje: 0,
    volumeHoje: 0,
    taxaSucesso: 0,
  });

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setIsLoading(true);

      // Carregar dados do dashboard
      const [reportResponse, usersResponse, accountsResponse, transactionsResponse] = await Promise.allSettled([
        reportService.getDashboardReport(),
        userService.getUsers(),
        bankAccountService.getAccounts(),
        transactionService.getTransactions({ page: 1, limit: 1 })
      ]);

      const newData: DashboardData = {
        totalClientes: 0,
        totalContas: 0,
        totalTransacoes: 0,
        volumeTransacoes: 0,
        transacoesHoje: 0,
        volumeHoje: 0,
        taxaSucesso: 98.5,
      };

      // Processar dados do relatório
      if (reportResponse.status === 'fulfilled') {
        const report = reportResponse.value.data;
        newData.totalTransacoes = report.totalTransacoes || 0;
        newData.volumeTransacoes = report.volumeTotal || 0;
        newData.transacoesHoje = report.transacoesHoje || 0;
        newData.volumeHoje = report.volumeHoje || 0;
        newData.taxaSucesso = report.taxaSucesso || 98.5;
      }

      // Processar dados de usuários
      if (usersResponse.status === 'fulfilled') {
        newData.totalClientes = usersResponse.value.data.length;
      }

      // Processar dados de contas
      if (accountsResponse.status === 'fulfilled') {
        newData.totalContas = accountsResponse.value.data.length;
      }

      setDashboardData(newData);
    } catch (error) {
      console.error('Erro ao carregar dados do dashboard:', error);
      toast.error('Erro ao carregar dados do dashboard');

      // Usar dados de fallback
      setDashboardData({
        totalClientes: 1250,
        totalContas: 320,
        totalTransacoes: 45678,
        volumeTransacoes: 12500000,
        transacoesHoje: 234,
        volumeHoje: 125000,
        taxaSucesso: 98.5,
      });
    } finally {
      setIsLoading(false);
    }
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
          <h1 className="text-3xl font-bold text-gray-900">Dashboard PSP</h1>
          <p className="text-gray-600 mt-1">Visão geral do sistema - Empresas, Transações e Relatórios</p>
        </div>
        <div className="text-right">
          <p className="text-sm text-gray-500">Olá,</p>
          <p className="font-semibold text-gray-900">{user?.email}</p>
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
            {user?.role}
          </span>
          <button
            onClick={loadDashboardData}
            className="mt-2 text-xs text-blue-600 hover:text-blue-800"
          >
            Atualizar dados
          </button>
        </div>
      </div>

      {/* Cards de Resumo */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-2 bg-blue-100 rounded-lg">
              <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total de Clientes</p>
              <p className="text-2xl font-bold text-gray-900">{(dashboardData.totalClientes || 0).toLocaleString()}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-2 bg-green-100 rounded-lg">
              <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Total de Contas</p>
              <p className="text-2xl font-bold text-gray-900">{(dashboardData.totalContas || 0).toLocaleString()}</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-2 bg-yellow-100 rounded-lg">
              <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Transações Hoje</p>
              <p className="text-2xl font-bold text-gray-900">{(dashboardData.transacoesHoje || 0).toLocaleString()}</p>
              <p className="text-xs text-yellow-600">R$ {((dashboardData.volumeHoje || 0) / 1000).toFixed(0)}K</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <div className="flex items-center">
            <div className="p-2 bg-purple-100 rounded-lg">
              <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Volume Total</p>
              <p className="text-2xl font-bold text-gray-900">R$ {((dashboardData.volumeTransacoes || 0) / 1000000).toFixed(1)}M</p>
              <p className="text-xs text-purple-600">{(dashboardData.totalTransacoes || 0).toLocaleString()} transações</p>
            </div>
          </div>
        </div>
      </div>

      {/* Ações Rápidas */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Ações Rápidas</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-left">
            <div className="flex items-center">
              <div className="p-2 bg-blue-100 rounded-lg mr-3">
                <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
              </div>
              <div>
                <p className="font-medium text-gray-900">Novo Cliente</p>
                <p className="text-sm text-gray-500">Cadastrar cliente</p>
              </div>
            </div>
          </button>

          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-left">
            <div className="flex items-center">
              <div className="p-2 bg-green-100 rounded-lg mr-3">
                <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
              </div>
              <div>
                <p className="font-medium text-gray-900">Ver Transações</p>
                <p className="text-sm text-gray-500">Monitorar atividade</p>
              </div>
            </div>
          </button>

          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-left">
            <div className="flex items-center">
              <div className="p-2 bg-yellow-100 rounded-lg mr-3">
                <svg className="w-5 h-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </div>
              <div>
                <p className="font-medium text-gray-900">Configurações</p>
                <p className="text-sm text-gray-500">Gerenciar sistema</p>
              </div>
            </div>
          </button>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
