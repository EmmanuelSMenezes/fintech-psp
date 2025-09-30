'use client';

import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { transactionService, bankAccountService, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

interface BoletoTransactionFormProps {
  onSuccess: (transactionId: string) => void;
  onCancel: () => void;
}

// Schema de validação Boleto
const boletoSchema = yup.object({
  amount: yup.number()
    .required('Valor é obrigatório')
    .min(0.01, 'Valor deve ser maior que R$ 0,01')
    .max(100000, 'Valor máximo é R$ 100.000,00'),
  dueDate: yup.string()
    .required('Data de vencimento é obrigatória')
    .test('future-date', 'Data deve ser futura', (value) => {
      if (!value) return false;
      const today = new Date();
      const dueDate = new Date(value);
      return dueDate > today;
    }),
  payerTaxId: yup.string()
    .required('CPF/CNPJ do pagador é obrigatório')
    .test('valid-tax-id', 'CPF/CNPJ inválido', (value) => {
      if (!value) return false;
      const clean = value.replace(/\D/g, '');
      return clean.length === 11 || clean.length === 14;
    }),
  payerName: yup.string().required('Nome do pagador é obrigatório'),
  instructions: yup.string().max(500, 'Instruções devem ter no máximo 500 caracteres'),
  contaId: yup.string().required('Selecione uma conta'),
});

type BoletoFormData = yup.InferType<typeof boletoSchema>;

const BoletoTransactionForm: React.FC<BoletoTransactionFormProps> = ({ onSuccess, onCancel }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [accounts, setAccounts] = useState<BankAccount[]>([]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<BoletoFormData>({
    resolver: yupResolver(boletoSchema),
    defaultValues: {
      dueDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 7 dias
    },
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

  const onSubmit = async (data: BoletoFormData) => {
    try {
      setIsLoading(true);
      
      const externalId = `boleto-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      
      const response = await transactionService.createBoletoTransaction({
        externalId,
        amount: data.amount,
        dueDate: data.dueDate,
        payerTaxId: data.payerTaxId.replace(/\D/g, ''),
        payerName: data.payerName,
        instructions: data.instructions,
        contaId: data.contaId,
      });

      toast.success('Boleto gerado com sucesso!');
      onSuccess(response.data.transactionId);
    } catch (error: any) {
      console.error('Erro ao gerar boleto:', error);
      toast.error(error.response?.data?.message || 'Erro ao gerar boleto');
    } finally {
      setIsLoading(false);
    }
  };

  const selectedAccount = accounts.find(acc => acc.contaId === watch('contaId'));
  const daysUntilDue = watch('dueDate') ? 
    Math.ceil((new Date(watch('dueDate')).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)) : 0;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center">
          <div className="p-2 bg-orange-100 rounded-lg mr-3">
            <svg className="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Gerar Boleto</h3>
            <p className="text-sm text-gray-600">Cobrança via boleto bancário</p>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
        {/* Conta de Recebimento */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Conta de Recebimento *
          </label>
          <select
            {...register('contaId')}
            className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-orange-500 focus:border-orange-500 ${
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

        {/* Dados do Pagador */}
        <div className="bg-gray-50 rounded-lg p-4">
          <h4 className="font-medium text-gray-900 mb-4">Dados do Pagador</h4>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* CPF/CNPJ */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                CPF/CNPJ *
              </label>
              <input
                type="text"
                {...register('payerTaxId')}
                onChange={(e) => {
                  const formatted = formatTaxId(e.target.value);
                  setValue('payerTaxId', formatted);
                }}
                placeholder="000.000.000-00"
                className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-orange-500 focus:border-orange-500 ${
                  errors.payerTaxId ? 'border-red-500' : 'border-gray-300'
                }`}
              />
              {errors.payerTaxId && (
                <p className="text-sm text-red-500 mt-1">{errors.payerTaxId.message}</p>
              )}
            </div>

            {/* Nome */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Nome do Pagador *
              </label>
              <input
                type="text"
                {...register('payerName')}
                placeholder="Nome completo"
                className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-orange-500 focus:border-orange-500 ${
                  errors.payerName ? 'border-red-500' : 'border-gray-300'
                }`}
              />
              {errors.payerName && (
                <p className="text-sm text-red-500 mt-1">{errors.payerName.message}</p>
              )}
            </div>
          </div>
        </div>

        {/* Dados do Boleto */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                className={`w-full border rounded-lg pl-10 pr-3 py-2 focus:ring-2 focus:ring-orange-500 focus:border-orange-500 ${
                  errors.amount ? 'border-red-500' : 'border-gray-300'
                }`}
              />
            </div>
            {errors.amount && (
              <p className="text-sm text-red-500 mt-1">{errors.amount.message}</p>
            )}
          </div>

          {/* Data de Vencimento */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Data de Vencimento *
            </label>
            <input
              type="date"
              {...register('dueDate')}
              min={new Date().toISOString().split('T')[0]}
              className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-orange-500 focus:border-orange-500 ${
                errors.dueDate ? 'border-red-500' : 'border-gray-300'
              }`}
            />
            {errors.dueDate && (
              <p className="text-sm text-red-500 mt-1">{errors.dueDate.message}</p>
            )}
            {daysUntilDue > 0 && (
              <p className="text-xs text-gray-500 mt-1">
                {daysUntilDue} dia{daysUntilDue !== 1 ? 's' : ''} para vencimento
              </p>
            )}
          </div>
        </div>

        {/* Instruções */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Instruções (Opcional)
          </label>
          <textarea
            {...register('instructions')}
            rows={4}
            placeholder="Instruções para o pagamento do boleto..."
            maxLength={500}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-orange-500 focus:border-orange-500"
          />
          <p className="text-xs text-gray-500 mt-1">
            {watch('instructions')?.length || 0}/500 caracteres
          </p>
        </div>

        {/* Resumo */}
        {selectedAccount && watch('amount') > 0 && (
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-2">Resumo do Boleto</h4>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Conta de recebimento:</span>
                <span className="font-medium">{selectedAccount.description}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Pagador:</span>
                <span className="font-medium">{watch('payerName')}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Valor:</span>
                <span className="font-medium text-orange-600">
                  R$ {watch('amount')?.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) || '0,00'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Vencimento:</span>
                <span className="font-medium">
                  {watch('dueDate') ? new Date(watch('dueDate')).toLocaleDateString('pt-BR') : '-'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Taxa:</span>
                <span className="font-medium">R$ 2,50</span>
              </div>
            </div>
          </div>
        )}

        {/* Informações Importantes */}
        <div className="bg-blue-50 rounded-lg p-4">
          <div className="flex">
            <svg className="w-5 h-5 text-blue-600 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div className="text-sm text-blue-800">
              <p className="font-medium mb-1">Informações importantes:</p>
              <ul className="list-disc list-inside space-y-1">
                <li>O boleto será gerado instantaneamente</li>
                <li>Taxa de R$ 2,50 por boleto gerado</li>
                <li>Prazo mínimo de vencimento: 1 dia</li>
                <li>Valor máximo: R$ 100.000,00</li>
              </ul>
            </div>
          </div>
        </div>

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
            disabled={isLoading || !watch('amount') || !watch('contaId') || !watch('payerName')}
            className="px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 disabled:opacity-50"
          >
            {isLoading ? 'Gerando...' : 'Gerar Boleto'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default BoletoTransactionForm;
