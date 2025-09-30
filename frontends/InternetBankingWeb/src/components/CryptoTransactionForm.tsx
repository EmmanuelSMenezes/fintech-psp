'use client';

import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { transactionService, bankAccountService, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

interface CryptoTransactionFormProps {
  onSuccess: (transactionId: string) => void;
  onCancel: () => void;
}

// Schema de validação Crypto
const cryptoSchema = yup.object({
  cryptoType: yup.string()
    .oneOf(['bitcoin', 'ethereum', 'usdt', 'usdc'], 'Tipo de criptomoeda inválido')
    .required('Selecione uma criptomoeda'),
  walletAddress: yup.string()
    .required('Endereço da carteira é obrigatório')
    .min(26, 'Endereço inválido')
    .max(62, 'Endereço inválido'),
  amount: yup.number()
    .required('Valor é obrigatório')
    .min(0.01, 'Valor deve ser maior que R$ 0,01')
    .max(1000000, 'Valor máximo é R$ 1.000.000,00'),
  description: yup.string().max(200, 'Descrição deve ter no máximo 200 caracteres'),
  contaId: yup.string().required('Selecione uma conta'),
});

type CryptoFormData = yup.InferType<typeof cryptoSchema>;

const cryptoOptions = [
  { 
    value: 'bitcoin', 
    label: 'Bitcoin (BTC)', 
    icon: '₿',
    color: 'text-orange-500',
    rate: 350000 // Mock rate
  },
  { 
    value: 'ethereum', 
    label: 'Ethereum (ETH)', 
    icon: 'Ξ',
    color: 'text-blue-500',
    rate: 12000 // Mock rate
  },
  { 
    value: 'usdt', 
    label: 'Tether (USDT)', 
    icon: '₮',
    color: 'text-green-500',
    rate: 5.2 // Mock rate
  },
  { 
    value: 'usdc', 
    label: 'USD Coin (USDC)', 
    icon: '$',
    color: 'text-blue-600',
    rate: 5.2 // Mock rate
  },
];

const CryptoTransactionForm: React.FC<CryptoTransactionFormProps> = ({ onSuccess, onCancel }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [accounts, setAccounts] = useState<BankAccount[]>([]);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<CryptoFormData>({
    resolver: yupResolver(cryptoSchema),
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

  const validateWalletAddress = (address: string, cryptoType: string): boolean => {
    if (!address || !cryptoType) return false;
    
    switch (cryptoType) {
      case 'bitcoin':
        return /^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$|^bc1[a-z0-9]{39,59}$/.test(address);
      case 'ethereum':
      case 'usdt':
      case 'usdc':
        return /^0x[a-fA-F0-9]{40}$/.test(address);
      default:
        return false;
    }
  };

  const onSubmit = async (data: CryptoFormData) => {
    try {
      setIsLoading(true);
      
      // Validar endereço da carteira
      if (!validateWalletAddress(data.walletAddress, data.cryptoType)) {
        toast.error('Endereço da carteira inválido para esta criptomoeda');
        return;
      }
      
      const externalId = `crypto-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      
      const response = await transactionService.createCryptoTransaction({
        externalId,
        amount: data.amount,
        cryptoType: data.cryptoType as 'bitcoin' | 'ethereum' | 'usdt' | 'usdc',
        walletAddress: data.walletAddress,
        fiatCurrency: 'BRL',
        description: data.description,
        contaId: data.contaId,
      });

      toast.success('Transação cripto iniciada com sucesso!');
      onSuccess(response.data.transactionId);
    } catch (error: any) {
      console.error('Erro ao criar transação cripto:', error);
      toast.error(error.response?.data?.message || 'Erro ao criar transação cripto');
    } finally {
      setIsLoading(false);
    }
  };

  const selectedAccount = accounts.find(acc => acc.contaId === watch('contaId'));
  const selectedCrypto = cryptoOptions.find(crypto => crypto.value === watch('cryptoType'));
  const cryptoAmount = selectedCrypto && watch('amount') ? 
    (watch('amount') / selectedCrypto.rate).toFixed(8) : '0';

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center">
          <div className="p-2 bg-purple-100 rounded-lg mr-3">
            <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
            </svg>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">Transação Cripto</h3>
            <p className="text-sm text-gray-600">Compra de criptomoedas</p>
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
            className={`w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-purple-500 focus:border-purple-500 ${
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

        {/* Criptomoeda */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Criptomoeda *
          </label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {cryptoOptions.map((crypto) => (
              <label
                key={crypto.value}
                className={`relative flex items-center p-4 border rounded-lg cursor-pointer hover:bg-gray-50 ${
                  watch('cryptoType') === crypto.value 
                    ? 'border-purple-500 bg-purple-50' 
                    : 'border-gray-300'
                }`}
              >
                <input
                  type="radio"
                  {...register('cryptoType')}
                  value={crypto.value}
                  className="sr-only"
                />
                <div className="flex items-center">
                  <span className={`text-2xl mr-3 ${crypto.color}`}>
                    {crypto.icon}
                  </span>
                  <div>
                    <div className="font-medium text-gray-900">{crypto.label}</div>
                    <div className="text-sm text-gray-500">
                      R$ {crypto.rate.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                    </div>
                  </div>
                </div>
                {watch('cryptoType') === crypto.value && (
                  <div className="absolute top-2 right-2">
                    <svg className="w-5 h-5 text-purple-600" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                  </div>
                )}
              </label>
            ))}
          </div>
          {errors.cryptoType && (
            <p className="text-sm text-red-500 mt-1">{errors.cryptoType.message}</p>
          )}
        </div>

        {/* Endereço da Carteira */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Endereço da Carteira *
          </label>
          <input
            type="text"
            {...register('walletAddress')}
            placeholder={
              watch('cryptoType') === 'bitcoin' ? '1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa' :
              watch('cryptoType') === 'ethereum' || watch('cryptoType') === 'usdt' || watch('cryptoType') === 'usdc' ? 
              '0x742d35Cc6634C0532925a3b8D4C9db96590b5b8c' :
              'Selecione uma criptomoeda primeiro'
            }
            className={`w-full border rounded-lg px-3 py-2 font-mono text-sm focus:ring-2 focus:ring-purple-500 focus:border-purple-500 ${
              errors.walletAddress ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.walletAddress && (
            <p className="text-sm text-red-500 mt-1">{errors.walletAddress.message}</p>
          )}
          <p className="text-xs text-gray-500 mt-1">
            Verifique cuidadosamente o endereço da carteira. Transações não podem ser revertidas.
          </p>
        </div>

        {/* Valor */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Valor em Reais *
          </label>
          <div className="relative">
            <span className="absolute left-3 top-2 text-gray-600">R$</span>
            <input
              type="number"
              step="0.01"
              min="0.01"
              max="1000000"
              {...register('amount')}
              placeholder="0,00"
              className={`w-full border rounded-lg pl-10 pr-3 py-2 focus:ring-2 focus:ring-purple-500 focus:border-purple-500 ${
                errors.amount ? 'border-red-500' : 'border-gray-300'
              }`}
            />
          </div>
          {errors.amount && (
            <p className="text-sm text-red-500 mt-1">{errors.amount.message}</p>
          )}
          {selectedCrypto && watch('amount') > 0 && (
            <p className="text-sm text-gray-600 mt-1">
              ≈ {cryptoAmount} {selectedCrypto.value.toUpperCase()}
            </p>
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
            placeholder="Finalidade da compra"
            maxLength={200}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
          />
          <p className="text-xs text-gray-500 mt-1">
            {watch('description')?.length || 0}/200 caracteres
          </p>
        </div>

        {/* Resumo */}
        {selectedAccount && selectedCrypto && watch('amount') > 0 && (
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-2">Resumo da Transação</h4>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Conta de origem:</span>
                <span className="font-medium">{selectedAccount.description}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Criptomoeda:</span>
                <span className="font-medium">{selectedCrypto.label}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Valor em BRL:</span>
                <span className="font-medium text-purple-600">
                  R$ {watch('amount')?.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) || '0,00'}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Quantidade:</span>
                <span className="font-medium">
                  {cryptoAmount} {selectedCrypto.value.toUpperCase()}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Taxa:</span>
                <span className="font-medium">1.5%</span>
              </div>
            </div>
          </div>
        )}

        {/* Aviso de Risco */}
        <div className="bg-yellow-50 rounded-lg p-4">
          <div className="flex">
            <svg className="w-5 h-5 text-yellow-600 mt-0.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
            <div className="text-sm text-yellow-800">
              <p className="font-medium mb-1">Aviso de Risco:</p>
              <ul className="list-disc list-inside space-y-1">
                <li>Criptomoedas são investimentos de alto risco</li>
                <li>Valores podem variar significativamente</li>
                <li>Transações são irreversíveis</li>
                <li>Verifique o endereço da carteira cuidadosamente</li>
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
            disabled={isLoading || !watch('cryptoType') || !watch('amount') || !watch('contaId') || !watch('walletAddress')}
            className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50"
          >
            {isLoading ? 'Processando...' : 'Comprar Cripto'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CryptoTransactionForm;
