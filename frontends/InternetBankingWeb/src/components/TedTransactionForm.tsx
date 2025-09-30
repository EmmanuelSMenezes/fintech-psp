'use client';

import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { transactionService, bankAccountService, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

interface TedTransactionFormProps {
  onSuccess: (transactionId: string) => void;
  onCancel: () => void;
}

// Schema de validação TED
const tedSchema = yup.object({
  bankCode: yup.string().required('Código do banco é obrigatório'),
  accountBranch: yup.string().required('Agência é obrigatória'),
  accountNumber: yup.string().required('Número da conta é obrigatório'),
  taxId: yup.string()
    .required('CPF/CNPJ é obrigatório')
    .test('valid-tax-id', 'CPF/CNPJ inválido', (value) => {
      if (!value) return false;
      const clean = value.replace(/\D/g, '');
      return clean.length === 11 || clean.length === 14;
    }),
  name: yup.string().required('Nome do beneficiário é obrigatório'),
  amount: yup.number()
    .required('Valor é obrigatório')
    .min(0.01, 'Valor deve ser maior que R$ 0,01')
    .max(500000, 'Valor máximo é R$ 500.000,00'),
  description: yup.string().max(200, 'Descrição deve ter no máximo 200 caracteres'),
  contaId: yup.string().required('Selecione uma conta'),
});

type TedFormData = yup.InferType<typeof tedSchema>;

const bancos = [
  { code: '001', name: 'Banco do Brasil' },
  { code: '033', name: 'Santander' },
  { code: '104', name: 'Caixa Econômica Federal' },
  { code: '237', name: 'Bradesco' },
  { code: '341', name: 'Itaú' },
  { code: '260', name: 'Nu Pagamentos' },
  { code: '077', name: 'Banco Inter' },
  { code: '212', name: 'Banco Original' },
  { code: '336', name: 'Banco C6' },
  { code: '290', name: 'PagSeguro' },
];

const TedTransactionForm: React.FC<TedTransactionFormProps> = ({ onSuccess, onCancel }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [accounts, setAccounts] = useState<BankAccount[]>([]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<TedFormData>({
    resolver: yupResolver(tedSchema),
  });

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      const response = await bankAccountService.getMyAccounts();
      const activeAccounts = response.data.filter(acc => acc.isActive);
      setAccounts(activeAccounts);
      
      if (activeAccounts.length > 0) {
        setValue('contaId', activeAccounts[0].contaId);
      }
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      // Não usar dados mock - deixar array vazio
      setAccounts([]);
    }
  };

  const formatTaxId = (value: string): string => {
    const clean = value.replace(/\D/g, '');
    if (clean.length <= 11) {
      return clean.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else {
      return clean.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }
  };

  const onSubmit = async (data: TedFormData) => {
    try {
      setIsLoading(true);
      
      const externalId = `ted-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      
      const response = await transactionService.createTedTransaction({
        externalId,
        amount: data.amount,
        bankCode: data.bankCode,
        accountBranch: data.accountBranch,
        accountNumber: data.accountNumber,
        taxId: data.taxId.replace(/\D/g, ''),
        name: data.name,
        description: data.description,
        contaId: data.contaId,
      });

      toast.success('TED enviada com sucesso!');
      onSuccess(response.data.transactionId);
    } catch (error: any) {
      console.error('Erro ao enviar TED:', error);
      toast.error(error.response?.data?.message || 'Erro ao enviar TED');
    } finally {
      setIsLoading(false);
    }
  };

  const selectedAccount = accounts.find(acc => acc.contaId === watch('contaId'));
  const selectedBank = bancos.find(bank => bank.code === watch('bankCode'));

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center">
          <div className="p-2 bg-green-100 rounded-lg mr-3">
            <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
            </svg>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Transferência TED</h3>
            <p className="text-sm text-gray-600">Transferência eletrônica disponível</p>
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
            className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
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

        {/* Dados do Beneficiário */}
        <div className="bg-gray-50 rounded-lg p-4">
          <h4 className="font-medium text-gray-900 mb-4">Dados do Beneficiário</h4>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Banco */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Banco *
              </label>
              <select
                {...register('bankCode')}
                className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
                  errors.bankCode ? 'border-red-500' : 'border-gray-300'
                }`}
              >
                <option value="">Selecione o banco</option>
                {bancos.map((banco) => (
                  <option key={banco.code} value={banco.code}>
                    {banco.code} - {banco.name}
                  </option>
                ))}
              </select>
              {errors.bankCode && (
                <p className="text-sm text-red-500 mt-1">{errors.bankCode.message}</p>
              )}
            </div>

            {/* Agência */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Agência *
              </label>
              <input
                type="text"
                {...register('accountBranch')}
                placeholder="Ex: 1234"
                className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
                  errors.accountBranch ? 'border-red-500' : 'border-gray-300'
                }`}
              />
              {errors.accountBranch && (
                <p className="text-sm text-red-500 mt-1">{errors.accountBranch.message}</p>
              )}
            </div>

            {/* Conta */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Conta *
              </label>
              <input
                type="text"
                {...register('accountNumber')}
                placeholder="Ex: 12345-6"
                className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
                  errors.accountNumber ? 'border-red-500' : 'border-gray-300'
                }`}
              />
              {errors.accountNumber && (
                <p className="text-sm text-red-500 mt-1">{errors.accountNumber.message}</p>
              )}
            </div>

            {/* CPF/CNPJ */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                CPF/CNPJ *
              </label>
              <input
                type="text"
                {...register('taxId')}
                onChange={(e) => {
                  const formatted = formatTaxId(e.target.value);
                  setValue('taxId', formatted);
                }}
                placeholder="000.000.000-00"
                className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
                  errors.taxId ? 'border-red-500' : 'border-gray-300'
                }`}
              />
              {errors.taxId && (
                <p className="text-sm text-red-500 mt-1">{errors.taxId.message}</p>
              )}
            </div>
          </div>

          {/* Nome */}
          <div className="mt-4">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Nome do Beneficiário *
            </label>
            <input
              type="text"
              {...register('name')}
              placeholder="Nome completo"
              className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
                errors.name ? 'border-red-500' : 'border-gray-300'
              }`}
            />
            {errors.name && (
              <p className="text-sm text-red-500 mt-1">{errors.name.message}</p>
            )}
          </div>
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
              max="500000"
              {...register('amount')}
              placeholder="0,00"
              className={`w-full border rounded-lg pl-10 pr-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500 ${
                errors.amount ? 'border-red-500' : 'border-gray-300'
              }`}
            />
          </div>
          {errors.amount && (
            <p className="text-sm text-red-500 mt-1">{errors.amount.message}</p>
          )}
          <p className="text-xs text-gray-500 mt-1">
            Limite máximo: R$ 500.000,00
          </p>
        </div>

        {/* Descrição */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Descrição (Opcional)
          </label>
          <input
            type="text"
            {...register('description')}
            placeholder="Finalidade da transferência"
            maxLength={200}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-green-500 focus:border-green-500"
          />
          <p className="text-xs text-gray-500 mt-1">
            {watch('description')?.length || 0}/200 caracteres
          </p>
        </div>

        {/* Resumo */}
        {selectedAccount && selectedBank && watch('amount') > 0 && (
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-2">Resumo da Transação</h4>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Conta de origem:</span>
                <span className="font-medium">{selectedAccount.description}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Banco destino:</span>
                <span className="font-medium">{selectedBank.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Beneficiário:</span>
                <span className="font-medium">{watch('name')}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Valor:</span>
                <span className="font-medium text-green-600">
                  R$ {watch('amount')?.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) || '0,00'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Taxa:</span>
                <span className="font-medium">R$ 0,00</span>
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
            disabled={isLoading || !watch('bankCode') || !watch('amount') || !watch('contaId')}
            className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
          >
            {isLoading ? 'Enviando...' : 'Enviar TED'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default TedTransactionForm;
