'use client';

import React from 'react';
import { BankAccount } from '@/services/api';

interface BankAccountListProps {
  accounts: BankAccount[];
  onEdit: (account: BankAccount) => void;
  onDelete: (accountId: string) => void;
  onToggleStatus: (accountId: string, isActive: boolean) => void;
  isLoading?: boolean;
}

const bancos: Record<string, string> = {
  '001': 'Banco do Brasil',
  '033': 'Santander',
  '104': 'Caixa Econômica Federal',
  '237': 'Bradesco',
  '341': 'Itaú',
  '260': 'Nu Pagamentos',
  '077': 'Banco Inter',
  '212': 'Banco Original',
  '336': 'Banco C6',
  '290': 'PagSeguro',
};

const getBankColor = (bankCode: string): string => {
  const colors: Record<string, string> = {
    '001': 'bg-yellow-100 text-yellow-800',
    '033': 'bg-red-100 text-red-800',
    '104': 'bg-blue-100 text-blue-800',
    '237': 'bg-red-100 text-red-800',
    '341': 'bg-orange-100 text-orange-800',
    '260': 'bg-purple-100 text-purple-800',
    '077': 'bg-orange-100 text-orange-800',
    '212': 'bg-green-100 text-green-800',
    '336': 'bg-gray-100 text-gray-800',
    '290': 'bg-blue-100 text-blue-800',
  };
  return colors[bankCode] || 'bg-gray-100 text-gray-800';
};

const BankAccountList: React.FC<BankAccountListProps> = ({
  accounts,
  onEdit,
  onDelete,
  onToggleStatus,
  isLoading = false
}) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-6">
          <div className="animate-pulse space-y-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="flex items-center space-x-4">
                <div className="w-12 h-12 bg-gray-200 rounded-lg"></div>
                <div className="flex-1 space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-1/4"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
                <div className="w-20 h-8 bg-gray-200 rounded"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (accounts.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-8 text-center">
          <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mx-auto mb-4">
            <svg className="w-8 h-8 text-gray-400 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Nenhuma conta encontrada</h3>
          <p className="text-gray-600">
            Você ainda não possui contas bancárias cadastradas. Clique em "Nova Conta" para começar.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900">
          Suas Contas Bancárias ({accounts.length})
        </h3>
      </div>

      <div className="divide-y divide-gray-200">
        {accounts.map((account) => (
          <div key={account.contaId} className="p-6 hover:bg-gray-50 transition-colors">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                {/* Ícone do Banco */}
                <div className={`p-3 rounded-lg ${getBankColor(account.bankCode)}`}>
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                  </svg>
                </div>

                {/* Informações da Conta */}
                <div>
                  <h4 className="font-semibold text-gray-900">{account.description}</h4>
                  <div className="flex items-center space-x-4 mt-1">
                    <span className="text-sm text-gray-600">
                      {bancos[account.bankCode] || `Banco ${account.bankCode}`}
                    </span>
                    <span className="text-sm text-gray-600">
                      Conta: {account.accountNumber}
                    </span>
                    <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                      account.isActive 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-red-100 text-red-800'
                    }`}>
                      {account.isActive ? 'Ativa' : 'Inativa'}
                    </span>
                  </div>
                  <p className="text-xs text-gray-500 mt-1">
                    Criada em {new Date(account.createdAt).toLocaleDateString('pt-BR')}
                    {account.updatedAt && (
                      <span> • Atualizada em {new Date(account.updatedAt).toLocaleDateString('pt-BR')}</span>
                    )}
                  </p>
                </div>
              </div>

              {/* Ações */}
              <div className="flex items-center space-x-2">
                {/* Toggle Status */}
                <button
                  onClick={() => onToggleStatus(account.contaId, !account.isActive)}
                  className={`px-3 py-1 rounded-lg text-sm font-medium transition-colors ${
                    account.isActive
                      ? 'bg-red-100 text-red-700 hover:bg-red-200'
                      : 'bg-green-100 text-green-700 hover:bg-green-200'
                  }`}
                >
                  {account.isActive ? 'Desativar' : 'Ativar'}
                </button>

                {/* Editar */}
                <button
                  onClick={() => onEdit(account)}
                  className="px-3 py-1 bg-blue-100 text-blue-700 rounded-lg text-sm font-medium hover:bg-blue-200 transition-colors"
                >
                  Editar
                </button>

                {/* Excluir */}
                <button
                  onClick={() => {
                    if (window.confirm('Tem certeza que deseja excluir esta conta? Esta ação não pode ser desfeita.')) {
                      onDelete(account.contaId);
                    }
                  }}
                  className="px-3 py-1 bg-red-100 text-red-700 rounded-lg text-sm font-medium hover:bg-red-200 transition-colors"
                >
                  Excluir
                </button>
              </div>
            </div>

            {/* Informações Adicionais */}
            <div className="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="flex items-center justify-between">
                  <span className="text-gray-600">Status da Integração</span>
                  <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                    account.isActive 
                      ? 'bg-green-100 text-green-800' 
                      : 'bg-gray-100 text-gray-800'
                  }`}>
                    {account.isActive ? 'Conectada' : 'Desconectada'}
                  </span>
                </div>
              </div>

              <div className="bg-gray-50 rounded-lg p-3">
                <div className="flex items-center justify-between">
                  <span className="text-gray-600">Última Sincronização</span>
                  <span className="text-gray-900 font-medium">
                    {account.updatedAt 
                      ? new Date(account.updatedAt).toLocaleString('pt-BR')
                      : 'Nunca'
                    }
                  </span>
                </div>
              </div>

              <div className="bg-gray-50 rounded-lg p-3">
                <div className="flex items-center justify-between">
                  <span className="text-gray-600">Transações Hoje</span>
                  <span className="text-gray-900 font-medium">
                    {Math.floor(Math.random() * 10)} {/* Mock data */}
                  </span>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default BankAccountList;
