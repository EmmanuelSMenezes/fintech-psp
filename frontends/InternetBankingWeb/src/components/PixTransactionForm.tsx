'use client';

import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { transactionService, bankAccountService, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

interface PixTransactionFormProps {
  onSuccess: (transactionId: string) => void;
  onCancel: () => void;
}

// Schema de validação PIX
const pixSchema = yup.object({
  pixKey: yup.string().required('Chave PIX é obrigatória'),
  amount: yup.number()
    .required('Valor é obrigatório')
    .min(0.01, 'Valor deve ser maior que R$ 0,01')
    .max(100000, 'Valor máximo é R$ 100.000,00'),
  description: yup.string().max(140, 'Descrição deve ter no máximo 140 caracteres'),
  contaId: yup.string().required('Selecione uma conta'),
});

type PixFormData = yup.InferType<typeof pixSchema>;

const PixTransactionForm: React.FC<PixTransactionFormProps> = ({ onSuccess, onCancel }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [accounts, setAccounts] = useState<BankAccount[]>([]);
  const [pixKeyType, setPixKeyType] = useState<'cpf' | 'cnpj' | 'email' | 'phone' | 'random'>('cpf');

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<PixFormData>({
    resolver: yupResolver(pixSchema),
  });

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      const response = await bankAccountService.getMyAccounts();
      const activeAccounts = response.data.filter(acc => acc.isActive);
      setAccounts(activeAccounts);
      
      // Selecionar primeira conta ativa automaticamente
      if (activeAccounts.length > 0) {
        setValue('contaId', activeAccounts[0].contaId);
      }
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      // Não usar dados mock - deixar array vazio
      setAccounts([]);
    }
  };

  const detectPixKeyType = (key: string): 'cpf' | 'cnpj' | 'email' | 'phone' | 'random' => {
    // Remove caracteres especiais
    const cleanKey = key.replace(/[^\w@.-]/g, '');
    
    if (cleanKey.includes('@')) return 'email';
    if (/^\d{11}$/.test(cleanKey)) return 'cpf';
    if (/^\d{14}$/.test(cleanKey)) return 'cnpj';
    if (/^\d{10,11}$/.test(cleanKey)) return 'phone';
    return 'random';
  };

  const formatPixKey = (key: string, type: 'cpf' | 'cnpj' | 'email' | 'phone' | 'random'): string => {
    const clean = key.replace(/\D/g, '');
    
    switch (type) {
      case 'cpf':
        return clean.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
      case 'cnpj':
        return clean.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
      case 'phone':
        if (clean.length === 11) {
          return clean.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
        } else if (clean.length === 10) {
          return clean.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
        }
        return key;
      default:
        return key;
    }
  };

  const handlePixKeyChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    const type = detectPixKeyType(value);
    setPixKeyType(type);
    setValue('pixKey', value);
  };

  const onSubmit = async (data: PixFormData) => {
    try {
      setIsLoading(true);
      
      const externalId = `pix-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      
      const response = await transactionService.createPixTransaction({
        externalId,
        amount: data.amount,
        pixKey: data.pixKey,
        description: data.description,
        contaId: data.contaId,
      });

      toast.success('PIX enviado com sucesso!');
      onSuccess(response.data.transactionId);
    } catch (error: any) {
      console.error('Erro ao enviar PIX:', error);
      toast.error(error.response?.data?.message || 'Erro ao enviar PIX');
    } finally {
      setIsLoading(false);
    }
  };

  const selectedAccount = accounts.find(acc => acc.contaId === watch('contaId'));

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center">
          <div className="p-2 bg-blue-100 rounded-lg mr-3">
            <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
            </svg>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Transferência PIX</h3>
            <p className="text-sm text-gray-600">Transferência instantânea 24h</p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
        {/* Conta de Origem */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Conta de Origem *
          </label>
          <select
            {...register('contaId')}
            className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
              errors.contaId ? 'border-red-500' : 'border-gray-300'
            }`}
          >
            <option value="">Selecione uma conta</option>
            {accounts.map((account) => (
              <option key={account.contaId} value={account.contaId}>
                {account.description} - {account.accountNumber}
              </option>
            ))}
          </select>
          {errors.contaId && (
            <p className="text-sm text-red-500 mt-1">{errors.contaId.message}</p>
          )}
        </div>

        {/* Chave PIX */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Chave PIX *
          </label>
          <div className="relative">
            <input
              type="text"
              {...register('pixKey')}
              onChange={handlePixKeyChange}
              placeholder="CPF, CNPJ, email, telefone ou chave aleatória"
              className={`w-full border rounded-lg px-3 py-2 pr-10 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                errors.pixKey ? 'border-red-500' : 'border-gray-300'
              }`}
            />
            <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
              <span className={`text-xs px-2 py-1 rounded-full ${
                pixKeyType === 'cpf' ? 'bg-blue-100 text-blue-800' :
                pixKeyType === 'cnpj' ? 'bg-green-100 text-green-800' :
                pixKeyType === 'email' ? 'bg-purple-100 text-purple-800' :
                pixKeyType === 'phone' ? 'bg-orange-100 text-orange-800' :
                'bg-gray-100 text-gray-800'
              }`}>
                {pixKeyType.toUpperCase()}
              </span>
            </div>
          </div>
          {errors.pixKey && (
            <p className="text-sm text-red-500 mt-1">{errors.pixKey.message}</p>
          )}
        </div>

        {/* Valor */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Valor *
          </label>
          <div className="relative">
            <span className="absolute left-3 top-2 text-gray-600">R$</span>
            <input
              type="number"
              step="0.01"
              min="0.01"
              max="100000"
              {...register('amount')}
              placeholder="0,00"
              className={`w-full border rounded-lg pl-10 pr-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                errors.amount ? 'border-red-500' : 'border-gray-300'
              }`}
            />
          </div>
          {errors.amount && (
            <p className="text-sm text-red-500 mt-1">{errors.amount.message}</p>
          )}
        </div>

        {/* Descrição */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Descrição (Opcional)
          </label>
          <input
            type="text"
            {...register('description')}
            placeholder="Motivo da transferência"
            maxLength={140}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
          <p className="text-xs text-gray-500 mt-1">
            {watch('description')?.length || 0}/140 caracteres
          </p>
        </div>

        {/* Resumo */}
        {selectedAccount && watch('amount') > 0 && (
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-2">Resumo da Transação</h4>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Conta de origem:</span>
                <span className="font-medium">{selectedAccount.description}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Chave PIX:</span>
                <span className="font-medium">{formatPixKey(watch('pixKey') || '', pixKeyType)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Valor:</span>
                <span className="font-medium text-green-600">
                  R$ {watch('amount')?.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) || '0,00'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Taxa:</span>
                <span className="font-medium">Gratuito</span>
              </div>
            </div>
          </div>
        )}

        {/* Botões */}
        <div className="flex justify-end space-x-3 pt-6 border-t border-gray-200">
          <button
            type="button"
            onClick={onCancel}
            disabled={isLoading}
            className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isLoading || !watch('pixKey') || !watch('amount') || !watch('contaId')}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {isLoading ? 'Enviando...' : 'Enviar PIX'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default PixTransactionForm;
