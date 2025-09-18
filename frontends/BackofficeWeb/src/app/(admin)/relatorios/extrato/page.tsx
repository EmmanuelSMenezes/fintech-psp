'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { reportService, userService, bankAccountService, User, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

interface ExtratoTransaction {
  id: string;
  type: string;
  amount: number;
  status: string;
  description: string;
  createdAt: string;
  bankAccount: string;
  reference: string;
  fee: number;
}

interface ExtratoData {
  transactions: ExtratoTransaction[];
  summary: {
    totalIn: number;
    totalOut: number;
    totalFees: number;
    balance: number;
    transactionCount: number;
  };
}

const ExtratoPage: React.FC = () => {
  useRequireAuth('view_reports');
  const { user } = useAuth();
  const [extratoData, setExtratoData] = useState<ExtratoData>({
    transactions: [],
    summary: {
      totalIn: 0,
      totalOut: 0,
      totalFees: 0,
      balance: 0,
      transactionCount: 0
    }
  });
  const [clientes, setClientes] = useState<User[]>([]);
  const [contas, setContas] = useState<BankAccount[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [filters, setFilters] = useState({
    clienteId: '',
    contaId: '',
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
    type: '',
    status: ''
  });

  const transactionTypes = [
    { value: 'pix', label: 'PIX' },
    { value: 'ted', label: 'TED' },
    { value: 'boleto', label: 'Boleto' },
    { value: 'crypto', label: 'Crypto' }
  ];

  const transactionStatuses = [
    { value: 'completed', label: 'Concluída' },
    { value: 'pending', label: 'Pendente' },
    { value: 'failed', label: 'Falhou' },
    { value: 'cancelled', label: 'Cancelada' }
  ];

  useEffect(() => {
    loadInitialData();
  }, []);

  useEffect(() => {
    if (filters.clienteId) {
      loadContasByClient(filters.clienteId);
    } else {
      setContas([]);
      setFilters(prev => ({ ...prev, contaId: '' }));
    }
  }, [filters.clienteId]);

  const loadInitialData = async () => {
    try {
      const clientesResponse = await userService.getUsers();
      setClientes(clientesResponse.data);
    } catch (error) {
      console.error('Erro ao carregar dados iniciais:', error);
      toast.error('Erro ao carregar dados iniciais');
    }
  };

  const loadContasByClient = async (clienteId: string) => {
    try {
      const response = await bankAccountService.getAccountsByClient(clienteId);
      setContas(response.data);
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      toast.error('Erro ao carregar contas');
    }
  };

  const loadExtrato = async () => {
    if (!filters.clienteId) {
      toast.error('Selecione um cliente para gerar o extrato');
      return;
    }

    try {
      setIsLoading(true);
      
      // Simular dados do extrato
      const mockTransactions: ExtratoTransaction[] = Array.from({ length: 50 }, (_, i) => {
        const types = ['pix', 'ted', 'boleto', 'crypto'];
        const statuses = ['completed', 'pending', 'failed'];
        const isIncoming = Math.random() > 0.6;
        
        return {
          id: `txn_${Date.now()}_${i}`,
          type: types[Math.floor(Math.random() * types.length)],
          amount: isIncoming ? Math.floor(Math.random() * 5000) + 100 : -(Math.floor(Math.random() * 3000) + 50),
          status: statuses[Math.floor(Math.random() * statuses.length)],
          description: isIncoming ? 'Recebimento PIX' : 'Transferência TED',
          createdAt: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString(),
          bankAccount: contas.length > 0 ? contas[0].banco : 'stark_bank',
          reference: `REF${Math.floor(Math.random() * 1000000)}`,
          fee: Math.floor(Math.random() * 10) + 1
        };
      }).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

      const totalIn = mockTransactions.filter(t => t.amount > 0).reduce((sum, t) => sum + t.amount, 0);
      const totalOut = Math.abs(mockTransactions.filter(t => t.amount < 0).reduce((sum, t) => sum + t.amount, 0));
      const totalFees = mockTransactions.reduce((sum, t) => sum + t.fee, 0);

      setExtratoData({
        transactions: mockTransactions,
        summary: {
          totalIn,
          totalOut,
          totalFees,
          balance: totalIn - totalOut - totalFees,
          transactionCount: mockTransactions.length
        }
      });
    } catch (error) {
      console.error('Erro ao carregar extrato:', error);
      toast.error('Erro ao carregar extrato');
    } finally {
      setIsLoading(false);
    }
  };

  const exportExtrato = () => {
    if (extratoData.transactions.length === 0) {
      toast.error('Nenhuma transação para exportar');
      return;
    }

    const csvContent = [
      ['Data', 'Tipo', 'Descrição', 'Referência', 'Valor', 'Taxa', 'Status', 'Banco'].join(','),
      ...extratoData.transactions.map(t => [
        new Date(t.createdAt).toLocaleString('pt-BR'),
        t.type.toUpperCase(),
        t.description,
        t.reference,
        t.amount.toFixed(2),
        t.fee.toFixed(2),
        t.status,
        t.bankAccount
      ].join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `extrato_${filters.startDate}_${filters.endDate}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    toast.success('Extrato exportado com sucesso!');
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('pt-BR');
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'failed':
        return 'bg-red-100 text-red-800';
      case 'cancelled':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status: string) => {
    const statusInfo = transactionStatuses.find(s => s.value === status);
    return statusInfo?.label || status;
  };

  const getTypeLabel = (type: string) => {
    const typeInfo = transactionTypes.find(t => t.value === type);
    return typeInfo?.label || type.toUpperCase();
  };

  const getClienteName = (clienteId: string) => {
    const cliente = clientes.find(c => c.id === clienteId);
    return cliente?.name || cliente?.email || 'N/A';
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Extrato de Transações</h1>
          <p className="text-gray-600 mt-1">Relatório detalhado de movimentações financeiras</p>
        </div>
        <div className="flex space-x-2">
          <button
            onClick={loadExtrato}
            disabled={!filters.clienteId}
            className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white px-4 py-2 rounded-lg"
          >
            Gerar Extrato
          </button>
          <button
            onClick={exportExtrato}
            disabled={extratoData.transactions.length === 0}
            className="bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white px-4 py-2 rounded-lg"
          >
            Exportar CSV
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Cliente *
            </label>
            <select
              value={filters.clienteId}
              onChange={(e) => setFilters({...filters, clienteId: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Selecione um cliente</option>
              {clientes.map((cliente) => (
                <option key={cliente.id} value={cliente.id}>
                  {cliente.name || cliente.email}
                </option>
              ))}
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Conta
            </label>
            <select
              value={filters.contaId}
              onChange={(e) => setFilters({...filters, contaId: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              disabled={!filters.clienteId}
            >
              <option value="">Todas as contas</option>
              {contas.map((conta) => (
                <option key={conta.id} value={conta.id}>
                  {conta.banco} - {conta.agencia}/{conta.conta}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Início
            </label>
            <input
              type="date"
              value={filters.startDate}
              onChange={(e) => setFilters({...filters, startDate: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Data Fim
            </label>
            <input
              type="date"
              value={filters.endDate}
              onChange={(e) => setFilters({...filters, endDate: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tipo
            </label>
            <select
              value={filters.type}
              onChange={(e) => setFilters({...filters, type: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos os tipos</option>
              {transactionTypes.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.label}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Status
            </label>
            <select
              value={filters.status}
              onChange={(e) => setFilters({...filters, status: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todos os status</option>
              {transactionStatuses.map((status) => (
                <option key={status.value} value={status.value}>
                  {status.label}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Summary */}
      {extratoData.transactions.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100 text-center">
            <div className="text-2xl font-bold text-green-600">{formatCurrency(extratoData.summary.totalIn)}</div>
            <div className="text-sm text-gray-600">Total Entradas</div>
          </div>
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100 text-center">
            <div className="text-2xl font-bold text-red-600">{formatCurrency(extratoData.summary.totalOut)}</div>
            <div className="text-sm text-gray-600">Total Saídas</div>
          </div>
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100 text-center">
            <div className="text-2xl font-bold text-yellow-600">{formatCurrency(extratoData.summary.totalFees)}</div>
            <div className="text-sm text-gray-600">Total Taxas</div>
          </div>
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100 text-center">
            <div className={`text-2xl font-bold ${extratoData.summary.balance >= 0 ? 'text-green-600' : 'text-red-600'}`}>
              {formatCurrency(extratoData.summary.balance)}
            </div>
            <div className="text-sm text-gray-600">Saldo Líquido</div>
          </div>
          <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100 text-center">
            <div className="text-2xl font-bold text-blue-600">{extratoData.summary.transactionCount}</div>
            <div className="text-sm text-gray-600">Transações</div>
          </div>
        </div>
      )}

      {/* Transactions Table */}
      {extratoData.transactions.length > 0 ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
            <h3 className="text-lg font-semibold text-gray-900">
              Extrato - {getClienteName(filters.clienteId)}
            </h3>
            <div className="text-sm text-gray-500">
              {filters.startDate} a {filters.endDate}
            </div>
          </div>
          
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Data/Hora
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tipo
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Descrição
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Referência
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Valor
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Taxa
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {extratoData.transactions.map((transaction) => (
                  <tr key={transaction.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {formatDate(transaction.createdAt)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        {getTypeLabel(transaction.type)}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900">
                      {transaction.description}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {transaction.reference}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <span className={transaction.amount >= 0 ? 'text-green-600' : 'text-red-600'}>
                        {formatCurrency(transaction.amount)}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-900">
                      {formatCurrency(transaction.fee)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(transaction.status)}`}>
                        {getStatusLabel(transaction.status)}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      ) : (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="text-gray-400 mb-4">
            <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {isLoading ? 'Carregando extrato...' : 'Nenhum extrato gerado'}
          </h3>
          <p className="text-gray-500">
            {isLoading ? 'Aguarde enquanto processamos os dados' : 'Selecione um cliente e clique em "Gerar Extrato" para visualizar as transações'}
          </p>
        </div>
      )}
    </div>
  );
};

export default ExtratoPage;
