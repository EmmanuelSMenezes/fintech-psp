'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { CreateBankAccountRequest } from '@/services/api';
import toast from 'react-hot-toast';

interface BankAccountFormProps {
  onSubmit: (data: Omit<CreateBankAccountRequest, 'clienteId'>) => Promise<void>;
  onCancel: () => void;
  initialData?: Partial<CreateBankAccountRequest>;
  isLoading?: boolean;
}

// Schema de validação
const bankAccountSchema = yup.object({
  bankCode: yup.string().required('Código do banco é obrigatório'),
  accountNumber: yup.string().required('Número da conta é obrigatório'),
  description: yup.string().required('Descrição é obrigatória'),
  credentials: yup.object({
    clientId: yup.string().required('Client ID é obrigatório'),
    clientSecret: yup.string().required('Client Secret é obrigatório'),
    apiKey: yup.string(),
    mtlsCert: yup.string(),
    additionalData: yup.object(),
  }).required(),
});

type BankAccountFormData = yup.InferType<typeof bankAccountSchema>;

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

const BankAccountForm: React.FC<BankAccountFormProps> = ({
  onSubmit,
  onCancel,
  initialData,
  isLoading = false
}) => {
  const [activeTab, setActiveTab] = useState<'basic' | 'credentials'>('basic');

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<BankAccountFormData>({
    resolver: yupResolver(bankAccountSchema),
    defaultValues: {
      bankCode: initialData?.bankCode || '',
      accountNumber: initialData?.accountNumber || '',
      description: initialData?.description || '',
      credentials: {
        clientId: initialData?.credentials?.clientId || '',
        clientSecret: initialData?.credentials?.clientSecret || '',
        apiKey: initialData?.credentials?.apiKey || '',
        mtlsCert: initialData?.credentials?.mtlsCert || '',
        additionalData: initialData?.credentials?.additionalData || {},
      },
    },
  });

  const selectedBankCode = watch('bankCode');

  const handleBankSelect = (bankCode: string) => {
    const selectedBank = bancos.find(b => b.code === bankCode);
    setValue('bankCode', bankCode);
    if (selectedBank && !initialData) {
      setValue('description', `Conta ${selectedBank.name}`);
    }
  };

  const onFormSubmit = async (data: BankAccountFormData) => {
    try {
      await onSubmit(data);
    } catch (error) {
      console.error('Erro ao salvar conta bancária:', error);
      toast.error('Erro ao salvar conta bancária');
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900">
          {initialData ? 'Editar Conta Bancária' : 'Nova Conta Bancária'}
        </h3>
      </div>

      <div className="p-6">
        {/* Tabs */}
        <div className="flex space-x-1 mb-6 bg-gray-100 p-1 rounded-lg">
          <button
            type="button"
            onClick={() => setActiveTab('basic')}
            className={`flex-1 py-2 px-4 rounded-md text-sm font-medium transition-colors ${
              activeTab === 'basic'
                ? 'bg-white text-blue-600 shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Dados Básicos
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('credentials')}
            className={`flex-1 py-2 px-4 rounded-md text-sm font-medium transition-colors ${
              activeTab === 'credentials'
                ? 'bg-white text-blue-600 shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Credenciais
          </button>
        </div>

        <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-6">
          {/* Tab: Dados Básicos */}
          {activeTab === 'basic' && (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Banco *
                </label>
                <select
                  value={selectedBankCode}
                  onChange={(e) => handleBankSelect(e.target.value)}
                  className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                    errors.bankCode ? 'border-red-500' : 'border-gray-300'
                  }`}
                >
                  <option value="">Selecione um banco</option>
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

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Número da Conta *
                </label>
                <input
                  type="text"
                  {...register('accountNumber')}
                  placeholder="Ex: 12345-6"
                  className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                    errors.accountNumber ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.accountNumber && (
                  <p className="text-sm text-red-500 mt-1">{errors.accountNumber.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Descrição *
                </label>
                <input
                  type="text"
                  {...register('description')}
                  placeholder="Ex: Conta Principal Itaú"
                  className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                    errors.description ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.description && (
                  <p className="text-sm text-red-500 mt-1">{errors.description.message}</p>
                )}
              </div>
            </div>
          )}

          {/* Tab: Credenciais */}
          {activeTab === 'credentials' && (
            <div className="space-y-4">
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-4">
                <div className="flex items-start">
                  <svg className="w-5 h-5 text-yellow-600 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
                  </svg>
                  <div>
                    <h4 className="text-sm font-medium text-yellow-800">Informações Sensíveis</h4>
                    <p className="text-sm text-yellow-700 mt-1">
                      As credenciais são criptografadas e armazenadas com segurança.
                    </p>
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Client ID *
                </label>
                <input
                  type="text"
                  {...register('credentials.clientId')}
                  placeholder="Client ID da integração bancária"
                  className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                    errors.credentials?.clientId ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.credentials?.clientId && (
                  <p className="text-sm text-red-500 mt-1">{errors.credentials.clientId.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Client Secret *
                </label>
                <input
                  type="password"
                  {...register('credentials.clientSecret')}
                  placeholder="Client Secret da integração bancária"
                  className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 ${
                    errors.credentials?.clientSecret ? 'border-red-500' : 'border-gray-300'
                  }`}
                />
                {errors.credentials?.clientSecret && (
                  <p className="text-sm text-red-500 mt-1">{errors.credentials.clientSecret.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  API Key (Opcional)
                </label>
                <input
                  type="text"
                  {...register('credentials.apiKey')}
                  placeholder="API Key adicional se necessário"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Certificado mTLS (Opcional)
                </label>
                <textarea
                  {...register('credentials.mtlsCert')}
                  placeholder="Certificado mTLS em formato PEM"
                  rows={4}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
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
              disabled={isLoading}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {isLoading ? 'Salvando...' : initialData ? 'Atualizar' : 'Criar Conta'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default BankAccountForm;
