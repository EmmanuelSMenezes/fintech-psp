'use client';

import React, { useState, useEffect } from 'react';
import { useRequireAuth } from '@/context/AuthContext';
import { transactionService, Transaction } from '@/services/api';
import TransactionFilters, { TransactionFilters as FilterType } from '@/components/TransactionFilters';
import TransactionList from '@/components/TransactionList';
import toast from 'react-hot-toast';

const HistoricoPage: React.FC = () => {
  useRequireAuth('view_tela_historico');

  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [filters, setFilters] = useState<FilterType>({});
  const [selectedTransaction, setSelectedTransaction] = useState<Transaction | null>(null);

  useEffect(() => {
    loadTransactions();
  }, [filters]);

  const loadTransactions = async () => {
    try {
      setIsLoading(true);

      // Preparar parâmetros da API
      const params: any = {};
      if (filters.type) params.type = filters.type;
      if (filters.contaId) params.contaId = filters.contaId;
      if (filters.status) params.status = filters.status;
      if (filters.startDate) params.startDate = filters.startDate;
      if (filters.endDate) params.endDate = filters.endDate;

      const response = await transactionService.getMyTransactions(params);
      let transactionData = response.data.transactions;

      // Aplicar filtros locais que não são suportados pela API
      if (filters.transactionType) {
        transactionData = transactionData.filter(t => t.type === filters.transactionType);
      }

      if (filters.minAmount) {
        transactionData = transactionData.filter(t => t.amount >= filters.minAmount!);
      }

      if (filters.maxAmount) {
        transactionData = transactionData.filter(t => t.amount <= filters.maxAmount!);
      }

      if (filters.search) {
        const searchTerm = filters.search.toLowerCase();
        transactionData = transactionData.filter(t =>
          t.transactionId.toLowerCase().includes(searchTerm) ||
          t.externalId.toLowerCase().includes(searchTerm) ||
          (t.description && t.description.toLowerCase().includes(searchTerm)) ||
          (t.endToEndId && t.endToEndId.toLowerCase().includes(searchTerm))
        );
      }

      setTransactions(transactionData);
    } catch (error) {
      console.error('Erro ao carregar transações:', error);
      toast.error('Erro ao carregar histórico de transações. Verifique se as APIs estão rodando.');

      // Não usar dados mock - deixar array vazio
      setTransactions([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleExportCSV = () => {
    try {
      const csvHeaders = [
        'ID da Transação',
        'ID Externo',
        'Tipo',
        'Status',
        'Valor',
        'Banco',
        'Conta',
        'Data de Criação',
        'Última Atualização',
        'End-to-End ID',
        'Descrição'
      ];

      const csvData = transactions.map(transaction => [
        transaction.transactionId,
        transaction.externalId,
        transaction.type.toUpperCase(),
        transaction.status,
        transaction.amount.toFixed(2),
        transaction.bankCode,
        transaction.contaId || '',
        new Date(transaction.createdAt).toLocaleString('pt-BR'),
        transaction.updatedAt ? new Date(transaction.updatedAt).toLocaleString('pt-BR') : '',
        transaction.endToEndId || '',
        transaction.description || ''
      ]);

      const csvContent = [
        csvHeaders.join(','),
        ...csvData.map(row => row.map(cell => `"${cell}"`).join(','))
      ].join('\n');

      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `historico-transacoes-${new Date().toISOString().split('T')[0]}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      toast.success('Relatório CSV exportado com sucesso!');
    } catch (error) {
      console.error('Erro ao exportar CSV:', error);
      toast.error('Erro ao exportar relatório');
    }
  };

  const handleTransactionClick = (transaction: Transaction) => {
    setSelectedTransaction(transaction);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Histórico</h1>
          <p className="text-gray-600 mt-1">Histórico completo de transações com filtros avançados</p>
        </div>
      </div>

      {/* Filtros */}
      <TransactionFilters
        onFiltersChange={setFilters}
        isLoading={isLoading}
      />

      {/* Lista de Transações */}
      <TransactionList
        transactions={transactions}
        isLoading={isLoading}
        onExportCSV={handleExportCSV}
        onTransactionClick={handleTransactionClick}
      />

      {/* Modal de Detalhes da Transação */}
      {selectedTransaction && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-gray-900">
                  Detalhes da Transação
                </h3>
                <button
                  onClick={() => setSelectedTransaction(null)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <div className="p-6 space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">ID da Transação</label>
                  <p className="mt-1 text-sm text-gray-900 font-mono">{selectedTransaction.transactionId}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">ID Externo</label>
                  <p className="mt-1 text-sm text-gray-900 font-mono">{selectedTransaction.externalId}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Tipo</label>
                  <p className="mt-1 text-sm text-gray-900">{selectedTransaction.type.toUpperCase()}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Status</label>
                  <p className="mt-1 text-sm text-gray-900">{selectedTransaction.status}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Valor</label>
                  <p className="mt-1 text-lg font-semibold text-gray-900">
                    R$ {selectedTransaction.amount.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                  </p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Banco</label>
                  <p className="mt-1 text-sm text-gray-900">{selectedTransaction.bankCode}</p>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Data de Criação</label>
                  <p className="mt-1 text-sm text-gray-900">
                    {new Date(selectedTransaction.createdAt).toLocaleString('pt-BR')}
                  </p>
                </div>
                {selectedTransaction.updatedAt && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Última Atualização</label>
                    <p className="mt-1 text-sm text-gray-900">
                      {new Date(selectedTransaction.updatedAt).toLocaleString('pt-BR')}
                    </p>
                  </div>
                )}
                {selectedTransaction.endToEndId && (
                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">End-to-End ID</label>
                    <p className="mt-1 text-sm text-gray-900 font-mono">{selectedTransaction.endToEndId}</p>
                  </div>
                )}
                {selectedTransaction.description && (
                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">Descrição</label>
                    <p className="mt-1 text-sm text-gray-900">{selectedTransaction.description}</p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default HistoricoPage;
