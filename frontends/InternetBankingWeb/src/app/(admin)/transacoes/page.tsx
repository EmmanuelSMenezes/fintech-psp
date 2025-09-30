'use client';

import React, { useState } from 'react';
import { useRequireAuth, useAuth } from '@/context/AuthContext';
import PixTransactionForm from '@/components/PixTransactionForm';
import TedTransactionForm from '@/components/TedTransactionForm';
import BoletoTransactionForm from '@/components/BoletoTransactionForm';
import CryptoTransactionForm from '@/components/CryptoTransactionForm';
import toast from 'react-hot-toast';

type TransactionType = 'pix' | 'ted' | 'boleto' | 'crypto' | null;

const TransacoesPage: React.FC = () => {
  useRequireAuth('view_tela_transacoes');
  const { hasPermission } = useAuth();

  const [selectedType, setSelectedType] = useState<TransactionType>(null);
  const [completedTransactionId, setCompletedTransactionId] = useState<string | null>(null);

  const transactionTypes = [
    {
      type: 'pix' as const,
      title: 'PIX',
      description: 'Transferência instantânea 24h',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
        </svg>
      ),
      color: 'bg-blue-100 text-blue-600 hover:bg-blue-200',
      permission: 'transacionar_pix',
      available: true,
    },
    {
      type: 'ted' as const,
      title: 'TED',
      description: 'Transferência eletrônica disponível',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
        </svg>
      ),
      color: 'bg-green-100 text-green-600 hover:bg-green-200',
      permission: 'transacionar_ted',
      available: true,
    },
    {
      type: 'boleto' as const,
      title: 'Boleto',
      description: 'Gerar boleto de cobrança',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
      ),
      color: 'bg-orange-100 text-orange-600 hover:bg-orange-200',
      permission: 'transacionar_boleto',
      available: true,
    },
    {
      type: 'crypto' as const,
      title: 'Cripto',
      description: 'Comprar criptomoedas',
      icon: (
        <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
        </svg>
      ),
      color: 'bg-purple-100 text-purple-600 hover:bg-purple-200',
      permission: 'transacionar_crypto',
      available: true,
    },
  ];

  const handleTransactionSuccess = (transactionId: string) => {
    setCompletedTransactionId(transactionId);
    setSelectedType(null);
    toast.success('Transação realizada com sucesso!');
  };

  const handleCancel = () => {
    setSelectedType(null);
    setCompletedTransactionId(null);
  };

  const renderTransactionForm = () => {
    switch (selectedType) {
      case 'pix':
        return (
          <PixTransactionForm
            onSuccess={handleTransactionSuccess}
            onCancel={handleCancel}
          />
        );
      case 'ted':
        return (
          <TedTransactionForm
            onSuccess={handleTransactionSuccess}
            onCancel={handleCancel}
          />
        );
      case 'boleto':
        return (
          <BoletoTransactionForm
            onSuccess={handleTransactionSuccess}
            onCancel={handleCancel}
          />
        );
      case 'crypto':
        return (
          <CryptoTransactionForm
            onSuccess={handleTransactionSuccess}
            onCancel={handleCancel}
          />
        );
      default:
        return null;
    }
  };

  if (selectedType) {
    return (
      <div className="p-6">
        <div className="mb-6">
          <button
            onClick={handleCancel}
            className="flex items-center text-gray-600 hover:text-gray-900"
          >
            <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Voltar para Transações
          </button>
        </div>
        {renderTransactionForm()}
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Transações</h1>
          <p className="text-gray-600 mt-1">PIX, TED, Boleto e Criptomoedas</p>
        </div>
      </div>

      {/* Success Message */}
      {completedTransactionId && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-4">
          <div className="flex">
            <svg className="w-5 h-5 text-green-600 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div>
              <h3 className="text-sm font-medium text-green-800">Transação realizada com sucesso!</h3>
              <p className="text-sm text-green-700 mt-1">
                ID da transação: <span className="font-mono">{completedTransactionId}</span>
              </p>
              <button
                onClick={() => setCompletedTransactionId(null)}
                className="text-sm text-green-800 underline mt-2"
              >
                Fechar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Transaction Types Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {transactionTypes.map((transaction) => {
          const hasAccess = hasPermission(transaction.permission);

          return (
            <button
              key={transaction.type}
              onClick={() => hasAccess && setSelectedType(transaction.type)}
              disabled={!hasAccess || !transaction.available}
              className={`relative p-6 rounded-xl border-2 border-gray-200 text-left transition-all ${
                hasAccess && transaction.available
                  ? `${transaction.color} cursor-pointer transform hover:scale-105`
                  : 'bg-gray-50 text-gray-400 cursor-not-allowed'
              }`}
            >
              <div className="flex items-center justify-between mb-4">
                <div className={`p-3 rounded-lg ${hasAccess ? transaction.color.split(' ')[0] : 'bg-gray-200'}`}>
                  {transaction.icon}
                </div>
                {!hasAccess && (
                  <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 0h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                )}
              </div>

              <h3 className="text-xl font-bold mb-2">{transaction.title}</h3>
              <p className="text-sm opacity-75">{transaction.description}</p>

              {!hasAccess && (
                <div className="absolute inset-0 bg-gray-100 bg-opacity-50 rounded-xl flex items-center justify-center">
                  <span className="text-sm font-medium text-gray-600">Sem permissão</span>
                </div>
              )}
            </button>
          );
        })}
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div className="flex items-center">
            <div className="p-2 bg-blue-100 rounded-lg mr-4">
              <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
              </svg>
            </div>
            <div>
              <p className="text-sm text-gray-600">Transações Hoje</p>
              <p className="text-2xl font-bold text-gray-900">12</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div className="flex items-center">
            <div className="p-2 bg-green-100 rounded-lg mr-4">
              <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div>
              <p className="text-sm text-gray-600">Volume Hoje</p>
              <p className="text-2xl font-bold text-gray-900">R$ 2.450</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div className="flex items-center">
            <div className="p-2 bg-orange-100 rounded-lg mr-4">
              <svg className="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <p className="text-sm text-gray-600">Pendentes</p>
              <p className="text-2xl font-bold text-gray-900">3</p>
            </div>
          </div>
        </div>
      </div>

      {/* Recent Transactions Preview */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-6 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-gray-900">Transações Recentes</h3>
            <button
              onClick={() => window.location.href = '/historico'}
              className="text-blue-600 hover:text-blue-700 text-sm font-medium"
            >
              Ver todas
            </button>
          </div>
        </div>

        <div className="divide-y divide-gray-200">
          {[
            { type: 'PIX', amount: 'R$ 150,00', status: 'Concluído', time: '14:30' },
            { type: 'TED', amount: 'R$ 500,00', status: 'Processando', time: '13:15' },
            { type: 'Boleto', amount: 'R$ 1.200,00', status: 'Pendente', time: '12:00' },
          ].map((transaction, index) => (
            <div key={index} className="p-4 hover:bg-gray-50">
              <div className="flex items-center justify-between">
                <div className="flex items-center">
                  <div className={`w-3 h-3 rounded-full mr-3 ${
                    transaction.status === 'Concluído' ? 'bg-green-500' :
                    transaction.status === 'Processando' ? 'bg-yellow-500' :
                    'bg-gray-400'
                  }`}></div>
                  <div>
                    <p className="font-medium text-gray-900">{transaction.type}</p>
                    <p className="text-sm text-gray-600">{transaction.time}</p>
                  </div>
                </div>
                <div className="text-right">
                  <p className="font-medium text-gray-900">{transaction.amount}</p>
                  <p className={`text-sm ${
                    transaction.status === 'Concluído' ? 'text-green-600' :
                    transaction.status === 'Processando' ? 'text-yellow-600' :
                    'text-gray-600'
                  }`}>
                    {transaction.status}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default TransacoesPage;
