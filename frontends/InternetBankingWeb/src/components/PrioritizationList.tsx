'use client';

import React from 'react';

interface PrioritizationRule {
  contaId: string;
  percentage: number;
  isActive: boolean;
  accountName?: string;
  accountNumber?: string;
  bankCode?: string;
}

interface PrioritizationListProps {
  rules: PrioritizationRule[];
  onEdit: () => void;
  onDelete: (contaId: string) => void;
  isLoading?: boolean;
}

const PrioritizationList: React.FC<PrioritizationListProps> = ({ 
  rules, 
  onEdit, 
  onDelete, 
  isLoading = false 
}) => {
  const getBankName = (bankCode: string) => {
    const banks: { [key: string]: string } = {
      '001': 'Banco do Brasil',
      '033': 'Santander',
      '104': 'Caixa Econômica',
      '237': 'Bradesco',
      '341': 'Itaú',
      '745': 'Citibank',
      '399': 'HSBC',
      '422': 'Safra',
    };
    return banks[bankCode] || `Banco ${bankCode}`;
  };

  const getBankColor = (bankCode: string) => {
    const colors: { [key: string]: string } = {
      '001': 'bg-yellow-100 text-yellow-800',
      '033': 'bg-red-100 text-red-800',
      '104': 'bg-blue-100 text-blue-800',
      '237': 'bg-red-100 text-red-800',
      '341': 'bg-orange-100 text-orange-800',
      '745': 'bg-blue-100 text-blue-800',
      '399': 'bg-red-100 text-red-800',
      '422': 'bg-green-100 text-green-800',
    };
    return colors[bankCode] || 'bg-gray-100 text-gray-800';
  };

  const totalPercentage = rules.filter(rule => rule.isActive).reduce((sum, rule) => sum + rule.percentage, 0);
  const isValidConfiguration = totalPercentage === 100;

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-6 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <div className="h-6 bg-gray-200 rounded w-48 animate-pulse"></div>
            <div className="h-8 bg-gray-200 rounded w-20 animate-pulse"></div>
          </div>
        </div>
        <div className="divide-y divide-gray-200">
          {[1, 2, 3].map((i) => (
            <div key={i} className="p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <div className="w-12 h-12 bg-gray-200 rounded-lg animate-pulse"></div>
                  <div className="space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-32 animate-pulse"></div>
                    <div className="h-3 bg-gray-200 rounded w-24 animate-pulse"></div>
                  </div>
                </div>
                <div className="text-right space-y-2">
                  <div className="h-6 bg-gray-200 rounded w-16 animate-pulse"></div>
                  <div className="h-4 bg-gray-200 rounded w-12 animate-pulse"></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (rules.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-8 text-center">
          <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mx-auto mb-4">
            <svg className="w-8 h-8 text-gray-400 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Nenhuma configuração de priorização</h3>
          <p className="text-gray-600 mb-4">
            Configure como as transações serão distribuídas entre suas contas bancárias.
          </p>
          <button
            onClick={onEdit}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
          >
            Configurar Priorização
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Configuração de Priorização</h3>
            <div className="flex items-center mt-1 space-x-2">
              <span className="text-sm text-gray-600">Total configurado:</span>
              <span className={`text-sm font-medium ${isValidConfiguration ? 'text-green-600' : 'text-red-600'}`}>
                {totalPercentage}%
              </span>
              {isValidConfiguration ? (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  <svg className="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                  </svg>
                  Válido
                </span>
              ) : (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                  <svg className="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                  </svg>
                  Inválido
                </span>
              )}
            </div>
          </div>
          <button
            onClick={onEdit}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
          >
            Editar
          </button>
        </div>
      </div>

      <div className="divide-y divide-gray-200">
        {rules.map((rule) => (
          <div key={rule.contaId} className="p-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                {/* Ícone da Conta */}
                <div className={`p-3 rounded-lg ${getBankColor(rule.bankCode || '000')}`}>
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                  </svg>
                </div>

                {/* Informações da Conta */}
                <div>
                  <div className="flex items-center space-x-2">
                    <h4 className="text-sm font-medium text-gray-900">
                      {rule.accountName || `Conta ${rule.contaId}`}
                    </h4>
                    {!rule.isActive && (
                      <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                        Inativo
                      </span>
                    )}
                  </div>
                  <div className="flex items-center space-x-2 mt-1">
                    <span className="text-xs text-gray-600">
                      {getBankName(rule.bankCode || '000')}
                    </span>
                    {rule.accountNumber && (
                      <>
                        <span className="text-xs text-gray-400">•</span>
                        <span className="text-xs text-gray-600 font-mono">
                          {rule.accountNumber}
                        </span>
                      </>
                    )}
                  </div>
                </div>
              </div>

              {/* Percentual e Ações */}
              <div className="flex items-center space-x-4">
                <div className="text-right">
                  <div className="text-2xl font-bold text-gray-900">
                    {rule.percentage}%
                  </div>
                  <div className="text-xs text-gray-600">
                    {rule.isActive ? 'Ativo' : 'Inativo'}
                  </div>
                </div>

                {/* Barra de Progresso */}
                <div className="w-24">
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div
                      className={`h-2 rounded-full transition-all duration-300 ${
                        rule.isActive ? 'bg-blue-600' : 'bg-gray-400'
                      }`}
                      style={{ width: `${Math.min(rule.percentage, 100)}%` }}
                    ></div>
                  </div>
                  <div className="text-xs text-gray-500 mt-1 text-center">
                    {rule.percentage}/100
                  </div>
                </div>

                {/* Botão de Remover */}
                <button
                  onClick={() => onDelete(rule.contaId)}
                  className="text-gray-400 hover:text-red-600 transition-colors"
                  title="Remover regra"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Resumo no Footer */}
      <div className="p-4 bg-gray-50 border-t border-gray-200">
        <div className="flex items-center justify-between">
          <div className="text-sm text-gray-600">
            {rules.filter(r => r.isActive).length} contas ativas de {rules.length} total
          </div>
          <div className="flex items-center space-x-4">
            <div className="text-sm text-gray-600">
              Total: <span className={`font-medium ${isValidConfiguration ? 'text-green-600' : 'text-red-600'}`}>
                {totalPercentage}%
              </span>
            </div>
            {!isValidConfiguration && (
              <div className="text-xs text-red-600">
                {totalPercentage < 100 
                  ? `Faltam ${100 - totalPercentage}%`
                  : `Excesso de ${totalPercentage - 100}%`
                }
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PrioritizationList;
