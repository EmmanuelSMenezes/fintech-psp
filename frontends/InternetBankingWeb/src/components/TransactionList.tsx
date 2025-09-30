'use client';

import React from 'react';
import { Transaction } from '@/services/api';

interface TransactionListProps {
  transactions: Transaction[];
  isLoading?: boolean;
  onExportCSV?: () => void;
  onTransactionClick?: (transaction: Transaction) => void;
}

const TransactionList: React.FC<TransactionListProps> = ({ 
  transactions, 
  isLoading = false, 
  onExportCSV,
  onTransactionClick 
}) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed': return 'bg-green-100 text-green-800';
      case 'processing': return 'bg-yellow-100 text-yellow-800';
      case 'pending': return 'bg-gray-100 text-gray-800';
      case 'failed': return 'bg-red-100 text-red-800';
      case 'cancelled': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'completed': return 'Concluído';
      case 'processing': return 'Processando';
      case 'pending': return 'Pendente';
      case 'failed': return 'Falhou';
      case 'cancelled': return 'Cancelado';
      default: return status;
    }
  };

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'pix': return 'bg-blue-100 text-blue-800';
      case 'ted': return 'bg-green-100 text-green-800';
      case 'boleto': return 'bg-orange-100 text-orange-800';
      case 'crypto': return 'bg-purple-100 text-purple-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'pix':
        return (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
          </svg>
        );
      case 'ted':
        return (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
          </svg>
        );
      case 'boleto':
        return (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        );
      case 'crypto':
        return (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
          </svg>
        );
      default:
        return null;
    }
  };

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-6 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <div className="h-6 bg-gray-200 rounded w-32 animate-pulse"></div>
            <div className="h-8 bg-gray-200 rounded w-24 animate-pulse"></div>
          </div>
        </div>
        <div className="divide-y divide-gray-200">
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} className="p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <div className="w-10 h-10 bg-gray-200 rounded-lg animate-pulse"></div>
                  <div className="space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-24 animate-pulse"></div>
                    <div className="h-3 bg-gray-200 rounded w-32 animate-pulse"></div>
                  </div>
                </div>
                <div className="text-right space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-20 animate-pulse"></div>
                  <div className="h-3 bg-gray-200 rounded w-16 animate-pulse"></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (transactions.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-8 text-center">
          <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mx-auto mb-4">
            <svg className="w-8 h-8 text-gray-400 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Nenhuma transação encontrada</h3>
          <p className="text-gray-600">
            Não há transações que correspondam aos filtros selecionados.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">
            Transações ({transactions.length})
          </h3>
          {onExportCSV && (
            <button
              onClick={onExportCSV}
              className="flex items-center px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              Exportar CSV
            </button>
          )}
        </div>
      </div>

      <div className="divide-y divide-gray-200">
        {transactions.map((transaction) => (
          <div
            key={transaction.transactionId}
            onClick={() => onTransactionClick?.(transaction)}
            className={`p-4 hover:bg-gray-50 transition-colors ${onTransactionClick ? 'cursor-pointer' : ''}`}
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                {/* Ícone do Tipo */}
                <div className={`p-2 rounded-lg ${getTypeColor(transaction.type)}`}>
                  {getTypeIcon(transaction.type)}
                </div>

                {/* Informações da Transação */}
                <div>
                  <div className="flex items-center space-x-2">
                    <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getTypeColor(transaction.type)}`}>
                      {transaction.type.toUpperCase()}
                    </span>
                    <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(transaction.status)}`}>
                      {getStatusText(transaction.status)}
                    </span>
                  </div>
                  
                  <div className="mt-1">
                    <p className="text-sm font-medium text-gray-900">
                      ID: {transaction.externalId}
                    </p>
                    <p className="text-xs text-gray-600">
                      {new Date(transaction.createdAt).toLocaleString('pt-BR')}
                    </p>
                    {transaction.description && (
                      <p className="text-xs text-gray-600 mt-1">
                        {transaction.description}
                      </p>
                    )}
                  </div>
                </div>
              </div>

              {/* Valor e Detalhes */}
              <div className="text-right">
                <p className="text-lg font-semibold text-gray-900">
                  R$ {transaction.amount.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                </p>
                <p className="text-xs text-gray-600">
                  Banco: {transaction.bankCode}
                </p>
                {transaction.endToEndId && (
                  <p className="text-xs text-gray-600 font-mono">
                    E2E: {transaction.endToEndId.slice(-8)}
                  </p>
                )}
              </div>
            </div>

            {/* Informações Adicionais (Expandidas) */}
            {onTransactionClick && (
              <div className="mt-3 pt-3 border-t border-gray-100">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-xs text-gray-600">
                  <div>
                    <span className="font-medium">ID da Transação:</span>
                    <p className="font-mono">{transaction.transactionId}</p>
                  </div>
                  {transaction.contaId && (
                    <div>
                      <span className="font-medium">Conta:</span>
                      <p>{transaction.contaId}</p>
                    </div>
                  )}
                  {transaction.updatedAt && (
                    <div>
                      <span className="font-medium">Última Atualização:</span>
                      <p>{new Date(transaction.updatedAt).toLocaleString('pt-BR')}</p>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Paginação Placeholder */}
      <div className="p-4 border-t border-gray-200 bg-gray-50">
        <div className="flex items-center justify-between">
          <p className="text-sm text-gray-600">
            Mostrando {transactions.length} transações
          </p>
          <div className="flex items-center space-x-2">
            <button
              disabled
              className="px-3 py-1 text-sm bg-white border border-gray-300 rounded-lg disabled:opacity-50"
            >
              Anterior
            </button>
            <span className="px-3 py-1 text-sm bg-blue-600 text-white rounded-lg">
              1
            </span>
            <button
              disabled
              className="px-3 py-1 text-sm bg-white border border-gray-300 rounded-lg disabled:opacity-50"
            >
              Próximo
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TransactionList;
