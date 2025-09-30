'use client';

import React, { useState, useEffect } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { bankAccountService, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

interface PrioritizationRule {
  contaId: string;
  percentage: number;
  isActive: boolean;
}

interface PrioritizationFormProps {
  onSave: (rules: PrioritizationRule[]) => void;
  onCancel: () => void;
  initialRules?: PrioritizationRule[];
  isLoading?: boolean;
}

// Schema de validação
const prioritizationSchema = yup.object({
  rules: yup.array().of(
    yup.object({
      contaId: yup.string().required('Conta é obrigatória'),
      percentage: yup.number()
        .required('Percentual é obrigatório')
        .min(0, 'Percentual deve ser maior ou igual a 0')
        .max(100, 'Percentual deve ser menor ou igual a 100'),
      isActive: yup.boolean().required(),
    })
  ).test('total-percentage', 'A soma dos percentuais deve ser 100%', function(rules) {
    if (!rules) return true;
    const activeRules = rules.filter(rule => rule.isActive);
    const total = activeRules.reduce((sum, rule) => sum + (rule.percentage || 0), 0);
    return total === 100;
  }),
});

type FormData = yup.InferType<typeof prioritizationSchema>;

const PrioritizationForm: React.FC<PrioritizationFormProps> = ({ 
  onSave, 
  onCancel, 
  initialRules = [], 
  isLoading = false 
}) => {
  const [accounts, setAccounts] = useState<BankAccount[]>([]);
  const [isLoadingAccounts, setIsLoadingAccounts] = useState(true);

  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
    trigger,
  } = useForm<FormData>({
    resolver: yupResolver(prioritizationSchema),
    defaultValues: {
      rules: initialRules.length > 0 ? initialRules : [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'rules',
  });

  const watchedRules = watch('rules');

  useEffect(() => {
    loadAccounts();
  }, []);

  // Auto-adicionar primeira regra quando contas são carregadas
  useEffect(() => {
    if (accounts.length > 0 && initialRules.length === 0 && fields.length === 0) {
      append({
        contaId: accounts[0].contaId,
        percentage: 100,
        isActive: true,
      });
    }
  }, [accounts, initialRules, fields.length, append]);

  const loadAccounts = async () => {
    try {
      setIsLoadingAccounts(true);
      const response = await bankAccountService.getMyAccounts();
      const activeAccounts = response.data.filter(acc => acc.isActive);
      setAccounts(activeAccounts);
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      // Não usar dados mock - deixar array vazio
      setAccounts([]);
    } finally {
      setIsLoadingAccounts(false);
    }
  };

  const addNewRule = () => {
    const usedAccountIds = watchedRules?.map(rule => rule.contaId) || [];
    const availableAccount = accounts.find(acc => !usedAccountIds.includes(acc.contaId));
    
    if (availableAccount) {
      append({
        contaId: availableAccount.contaId,
        percentage: 0,
        isActive: true,
      });
    } else {
      toast.error('Todas as contas já estão sendo utilizadas');
    }
  };

  const onSubmit = async (data: FormData) => {
    try {
      await trigger();
      onSave(data.rules || []);
    } catch (error) {
      console.error('Erro ao salvar regras:', error);
      toast.error('Erro ao salvar regras de priorização');
    }
  };

  // Calcular total dos percentuais ativos
  const totalPercentage = watchedRules?.reduce((sum, rule) => {
    return rule.isActive ? sum + (rule.percentage || 0) : sum;
  }, 0) || 0;

  const isValidTotal = totalPercentage === 100;

  if (isLoadingAccounts) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <span className="ml-2 text-gray-600">Carregando contas...</span>
      </div>
    );
  }

  if (accounts.length === 0) {
    return (
      <div className="text-center p-8">
        <div className="p-4 bg-yellow-100 rounded-full w-16 h-16 mx-auto mb-4">
          <svg className="w-8 h-8 text-yellow-600 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">Nenhuma conta ativa encontrada</h3>
        <p className="text-gray-600 mb-4">
          Você precisa ter pelo menos uma conta bancária ativa para configurar regras de priorização.
        </p>
        <button
          onClick={onCancel}
          className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600"
        >
          Voltar
        </button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-medium text-gray-900">
          Configurar Regras de Priorização
        </h3>
        <button
          type="button"
          onClick={addNewRule}
          disabled={fields.length >= accounts.length}
          className="px-3 py-1 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
        >
          + Adicionar Regra
        </button>
      </div>

      <div className="space-y-4">
        {fields.map((field, index) => {
          const account = accounts.find(acc => acc.contaId === watchedRules?.[index]?.contaId);
          
          return (
            <div key={field.id} className="p-4 border border-gray-200 rounded-lg">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-3">
                  <input
                    type="checkbox"
                    {...register(`rules.${index}.isActive`)}
                    className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <span className="text-sm font-medium text-gray-700">
                    Regra {index + 1}
                  </span>
                </div>
                
                {fields.length > 1 && (
                  <button
                    type="button"
                    onClick={() => remove(index)}
                    className="text-red-600 hover:text-red-700 text-sm"
                  >
                    Remover
                  </button>
                )}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Seleção de Conta */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Conta Bancária
                  </label>
                  <select
                    {...register(`rules.${index}.contaId`)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="">Selecione uma conta</option>
                    {accounts.map((account) => (
                      <option key={account.contaId} value={account.contaId}>
                        {account.description} - {account.accountNumber}
                      </option>
                    ))}
                  </select>
                  {errors.rules?.[index]?.contaId && (
                    <p className="text-sm text-red-500 mt-1">
                      {errors.rules[index]?.contaId?.message}
                    </p>
                  )}
                </div>

                {/* Percentual */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Percentual (%)
                  </label>
                  <input
                    type="number"
                    min="0"
                    max="100"
                    step="0.01"
                    {...register(`rules.${index}.percentage`, { valueAsNumber: true })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="0.00"
                  />
                  {errors.rules?.[index]?.percentage && (
                    <p className="text-sm text-red-500 mt-1">
                      {errors.rules[index]?.percentage?.message}
                    </p>
                  )}
                </div>
              </div>

              {/* Informações da Conta */}
              {account && (
                <div className="mt-3 p-3 bg-gray-50 rounded-lg">
                  <div className="text-sm text-gray-600">
                    <p><strong>Banco:</strong> {account.bankCode}</p>
                    <p><strong>Conta:</strong> {account.accountNumber}</p>
                    <p><strong>Status:</strong> 
                      <span className={`ml-1 ${account.isActive ? 'text-green-600' : 'text-red-600'}`}>
                        {account.isActive ? 'Ativa' : 'Inativa'}
                      </span>
                    </p>
                  </div>
                </div>
              )}
            </div>
          );
        })}

        {fields.length === 0 && (
          <div className="text-center py-8">
            <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mx-auto mb-4">
              <svg className="w-8 h-8 text-gray-400 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
            </div>
            <p className="text-gray-600">Nenhuma regra de priorização configurada</p>
            <button
              type="button"
              onClick={addNewRule}
              className="mt-2 text-blue-600 hover:text-blue-700"
            >
              Adicionar primeira regra
            </button>
          </div>
        )}
      </div>

      {/* Resumo */}
      {fields.length > 0 && (
        <div className="mt-6 p-4 bg-gray-50 rounded-lg">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-gray-700">Total dos percentuais:</span>
            <span className={`text-lg font-bold ${isValidTotal ? 'text-green-600' : 'text-red-600'}`}>
              {totalPercentage}%
            </span>
          </div>

          {!isValidTotal && (
            <p className="text-sm text-red-600">
              {totalPercentage < 100
                ? `Faltam ${100 - totalPercentage}% para completar 100%`
                : `Excesso de ${totalPercentage - 100}% sobre 100%`
              }
            </p>
          )}

          {errors.rules && (
            <p className="text-sm text-red-600 mt-2">
              {errors.rules.message}
            </p>
          )}
        </div>
      )}

      {/* Botões */}
      <div className="flex justify-end space-x-3 pt-4 border-t">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={isLoading || !isValidTotal || fields.length === 0}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
        >
          {isLoading ? 'Salvando...' : 'Salvar Regras'}
        </button>
      </div>
    </form>
  );
};

export default PrioritizationForm;
