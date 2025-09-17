'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useAuth } from '@/context/AuthContext';
import toast from 'react-hot-toast';

// Schema de validação
const loginSchema = yup.object({
  client_id: yup.string().required('Client ID é obrigatório'),
  client_secret: yup.string().required('Client Secret é obrigatório'),
  scope: yup.string().oneOf(['banking', 'sub-banking'], 'Scope deve ser banking ou sub-banking').required('Scope é obrigatório'),
});

type LoginFormData = yup.InferType<typeof loginSchema>;

const SignInPage: React.FC = () => {
  const router = useRouter();
  const { login, isAuthenticated, isLoading } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<LoginFormData>({
    resolver: yupResolver(loginSchema),
    defaultValues: {
      scope: 'banking',
    },
  });

  // Redirecionar se já estiver autenticado
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      router.push('/');
    }
  }, [isAuthenticated, isLoading, router]);

  const onSubmit = async (data: LoginFormData) => {
    setIsSubmitting(true);
    try {
      const success = await login(data);
      if (success) {
        router.push('/');
      }
    } catch (error) {
      console.error('Erro no login:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Função para preencher credenciais de exemplo
  const fillExampleCredentials = (type: 'cliente' | 'sub-cliente') => {
    if (type === 'cliente') {
      setValue('client_id', 'cliente_banking');
      setValue('client_secret', 'cliente_secret_000');
      setValue('scope', 'banking');
    } else {
      setValue('client_id', 'sub_cliente_banking');
      setValue('client_secret', 'sub_cliente_secret_000');
      setValue('scope', 'sub-banking');
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-blue-100 flex items-center justify-center p-4">
      <div className="max-w-md w-full space-y-8">
        <div className="bg-white rounded-2xl shadow-xl p-8">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="mx-auto h-16 w-16 bg-gradient-to-r from-green-600 to-blue-600 rounded-full flex items-center justify-center mb-4">
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
              </svg>
            </div>
            <h2 className="text-3xl font-bold text-gray-900">FintechPSP</h2>
            <p className="text-gray-600 mt-2">Internet Banking - Acesso Cliente</p>
          </div>

          {/* Botões de exemplo */}
          <div className="mb-6 space-y-2">
            <p className="text-sm text-gray-600 text-center">Credenciais de exemplo:</p>
            <div className="flex space-x-2">
              <button
                type="button"
                onClick={() => fillExampleCredentials('cliente')}
                className="flex-1 px-3 py-2 text-xs bg-green-100 text-green-700 rounded-lg hover:bg-green-200 transition-colors"
              >
                Cliente
              </button>
              <button
                type="button"
                onClick={() => fillExampleCredentials('sub-cliente')}
                className="flex-1 px-3 py-2 text-xs bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition-colors"
              >
                Sub-Cliente
              </button>
            </div>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div>
              <label htmlFor="client_id" className="block text-sm font-medium text-gray-700 mb-2">
                Client ID
              </label>
              <input
                {...register('client_id')}
                type="text"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent transition-all"
                placeholder="Digite o Client ID"
              />
              {errors.client_id && (
                <p className="mt-1 text-sm text-red-600">{errors.client_id.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="client_secret" className="block text-sm font-medium text-gray-700 mb-2">
                Client Secret
              </label>
              <input
                {...register('client_secret')}
                type="password"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent transition-all"
                placeholder="Digite o Client Secret"
              />
              {errors.client_secret && (
                <p className="mt-1 text-sm text-red-600">{errors.client_secret.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="scope" className="block text-sm font-medium text-gray-700 mb-2">
                Tipo de Acesso
              </label>
              <select
                {...register('scope')}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-green-500 focus:border-transparent transition-all"
              >
                <option value="banking">Cliente Principal</option>
                <option value="sub-banking">Sub-Cliente</option>
              </select>
              {errors.scope && (
                <p className="mt-1 text-sm text-red-600">{errors.scope.message}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-gradient-to-r from-green-600 to-blue-600 text-white py-3 px-4 rounded-lg font-medium hover:from-green-700 hover:to-blue-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSubmitting ? (
                <div className="flex items-center justify-center">
                  <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
                  Entrando...
                </div>
              ) : (
                'Acessar Internet Banking'
              )}
            </button>
          </form>

          {/* Footer */}
          <div className="mt-8 text-center">
            <p className="text-xs text-gray-500">
              FintechPSP v1.0.0 - Internet Banking
            </p>
          </div>
        </div>

        {/* Info adicional */}
        <div className="bg-white/80 backdrop-blur-sm rounded-lg p-4 text-center">
          <p className="text-sm text-gray-600">
            <strong>Funcionalidades disponíveis:</strong>
          </p>
          <div className="mt-2 text-xs text-gray-500 space-y-1">
            <p>• Dashboard pessoal com resumo financeiro</p>
            <p>• Gerenciamento de contas bancárias</p>
            <p>• Histórico de transações</p>
            <p>• Configuração de priorização</p>
            <p>• Gestão de sub-usuários</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignInPage;
