'use client';

import React, { useState, useEffect } from 'react';
import { bankAccountService, BankAccount } from '@/services/api';

interface TransactionFiltersProps {
  onFiltersChange: (filters: TransactionFilters) => void;
  isLoading?: boolean;
}

export interface TransactionFilters {
  type?: 'cashin' | 'cashout' | '';
  transactionType?: 'pix' | 'ted' | 'boleto' | 'crypto' | '';
  contaId?: string;
  status?: 'pending' | 'processing' | 'completed' | 'failed' | 'cancelled' | '';
  startDate?: string;
  endDate?: string;
  minAmount?: number;
  maxAmount?: number;
  search?: string;
}

const TransactionFilters: React.FC<TransactionFiltersProps> = ({ onFiltersChange, isLoading = false }) => {
  const [filters, setFilters] = useState<TransactionFilters>({});
  const [accounts, setAccounts] = useState<BankAccount[]>([]);
  const [showAdvanced, setShowAdvanced] = useState(false);

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      const response = await bankAccountService.getMyAccounts();
      setAccounts(response.data);
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      // Não usar dados mock - deixar array vazio
      setAccounts([]);
    }
  };

  const handleFilterChange = (key: keyof TransactionFilters, value: any) => {
    const newFilters = { ...filters, [key]: value };
    setFilters(newFilters);
    onFiltersChange(newFilters);
  };

  const clearFilters = () => {
    const emptyFilters: TransactionFilters = {};
    setFilters(emptyFilters);
    onFiltersChange(emptyFilters);
  };

  const setQuickDateRange = (days: number) => {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - days);
    
    const newFilters = {
      ...filters,
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
    };
    setFilters(newFilters);
    onFiltersChange(newFilters);
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Filtros</h3>
        <div className="flex items-center space-x-2">
          <button
            onClick={() => setShowAdvanced(!showAdvanced)}
            className="text-sm text-blue-600 hover:text-blue-700"
          >
            {showAdvanced ? 'Ocultar' : 'Avançado'}
          </button>
          <button
            onClick={clearFilters}
            className="text-sm text-gray-600 hover:text-gray-700"
          >
            Limpar
          </button>
        </div>
      </div>

      <div className="space-y-4">
        {/* Busca */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Buscar
          </label>
          <input
            type="text"
            value={filters.search || ''}
            onChange={(e) => handleFilterChange('search', e.target.value)}
            placeholder="ID da transação, descrição, chave PIX..."
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>

        {/* Filtros Básicos */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Tipo de Operação */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Operação
            </label>
            <select
              value={filters.type || ''}
              onChange={(e) => handleFilterChange('type', e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Todas</option>
              <option value="cashin">Entrada</option>
              <option value="cashout">Saída</option>
            </select>
          </div>

          {/* Tipo de Transação */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Tipo
            </label>
            <select
              value={filters.transactionType || ''}
              onChange={(e) => handleFilterChange('transactionType', e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Todos</option>
              <option value="pix">PIX</option>
              <option value="ted">TED</option>
              <option value="boleto">Boleto</option>
              <option value="crypto">Cripto</option>
            </select>
          </div>

          {/* Status */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Status
            </label>
            <select
              value={filters.status || ''}
              onChange={(e) => handleFilterChange('status', e.target.value)}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">Todos</option>
              <option value="pending">Pendente</option>
              <option value="processing">Processando</option>
              <option value="completed">Concluído</option>
              <option value="failed">Falhou</option>
              <option value="cancelled">Cancelado</option>
            </select>
          </div>
        </div>

        {/* Período - Botões Rápidos */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Período
          </label>
          <div className="flex flex-wrap gap-2 mb-3">
            <button
              onClick={() => setQuickDateRange(7)}
              className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
            >
              7 dias
            </button>
            <button
              onClick={() => setQuickDateRange(30)}
              className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
            >
              30 dias
            </button>
            <button
              onClick={() => setQuickDateRange(90)}
              className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
            >
              90 dias
            </button>
            <button
              onClick={() => {
                const today = new Date();
                const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
                handleFilterChange('startDate', firstDay.toISOString().split('T')[0]);
                handleFilterChange('endDate', today.toISOString().split('T')[0]);
              }}
              className="px-3 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded-lg"
            >
              Este mês
            </button>
          </div>

          {/* Datas Personalizadas */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-gray-600 mb-1">Data inicial</label>
              <input
                type="date"
                value={filters.startDate || ''}
                onChange={(e) => handleFilterChange('startDate', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <div>
              <label className="block text-xs text-gray-600 mb-1">Data final</label>
              <input
                type="date"
                value={filters.endDate || ''}
                onChange={(e) => handleFilterChange('endDate', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
          </div>
        </div>

        {/* Filtros Avançados */}
        {showAdvanced && (
          <div className="border-t border-gray-200 pt-4 space-y-4">
            {/* Conta */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Conta
              </label>
              <select
                value={filters.contaId || ''}
                onChange={(e) => handleFilterChange('contaId', e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="">Todas as contas</option>
                {accounts.map((account) => (
                  <option key={account.contaId} value={account.contaId}>
                    {account.description} - {account.accountNumber}
                  </option>
                ))}
              </select>
            </div>

            {/* Faixa de Valores */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Valor
              </label>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs text-gray-600 mb-1">Valor mínimo</label>
                  <div className="relative">
                    <span className="absolute left-3 top-2 text-gray-600">R$</span>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      value={filters.minAmount || ''}
                      onChange={(e) => handleFilterChange('minAmount', e.target.value ? parseFloat(e.target.value) : undefined)}
                      placeholder="0,00"
                      className="w-full border border-gray-300 rounded-lg pl-10 pr-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-xs text-gray-600 mb-1">Valor máximo</label>
                  <div className="relative">
                    <span className="absolute left-3 top-2 text-gray-600">R$</span>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      value={filters.maxAmount || ''}
                      onChange={(e) => handleFilterChange('maxAmount', e.target.value ? parseFloat(e.target.value) : undefined)}
                      placeholder="0,00"
                      className="w-full border border-gray-300 rounded-lg pl-10 pr-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Resumo dos Filtros Ativos */}
      {Object.keys(filters).some(key => filters[key as keyof TransactionFilters]) && (
        <div className="mt-4 pt-4 border-t border-gray-200">
          <div className="flex flex-wrap gap-2">
            {Object.entries(filters).map(([key, value]) => {
              if (!value) return null;
              
              let label = '';
              switch (key) {
                case 'type': label = value === 'cashin' ? 'Entrada' : 'Saída'; break;
                case 'transactionType': label = value.toUpperCase(); break;
                case 'status': 
                  label = {
                    'pending': 'Pendente',
                    'processing': 'Processando', 
                    'completed': 'Concluído',
                    'failed': 'Falhou',
                    'cancelled': 'Cancelado'
                  }[value] || value;
                  break;
                case 'search': label = `"${value}"`; break;
                case 'startDate': label = `De: ${new Date(value).toLocaleDateString('pt-BR')}`; break;
                case 'endDate': label = `Até: ${new Date(value).toLocaleDateString('pt-BR')}`; break;
                case 'minAmount': label = `Min: R$ ${value.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`; break;
                case 'maxAmount': label = `Max: R$ ${value.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`; break;
                case 'contaId': 
                  const account = accounts.find(acc => acc.contaId === value);
                  label = account ? account.description : 'Conta';
                  break;
                default: label = String(value);
              }
              
              return (
                <span
                  key={key}
                  className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                >
                  {label}
                  <button
                    onClick={() => handleFilterChange(key as keyof TransactionFilters, undefined)}
                    className="ml-1 hover:text-blue-600"
                  >
                    ×
                  </button>
                </span>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};

export default TransactionFilters;
