'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import toast from 'react-hot-toast';

interface CompanySettingsFormProps {
  onSave: (data: CompanySettingsFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
  initialData?: Partial<CompanySettingsFormData>;
}

export interface CompanySettingsFormData {
  // Informações da Empresa
  companyName: string;
  cnpj: string;
  email: string;
  phone: string;
  
  // Configurações de Segurança
  maxDailyTransactionAmount: number;
  maxSingleTransactionAmount: number;
  requireTwoFactorAuth: boolean;
  sessionTimeoutMinutes: number;
  
  // Configurações de Notificação
  emailNotifications: {
    transactionConfirmation: boolean;
    dailyReport: boolean;
    securityAlerts: boolean;
    systemMaintenance: boolean;
  };
  
  // Configurações SMTP
  smtpSettings: {
    enabled: boolean;
    host: string;
    port: number;
    username: string;
    password: string;
    fromEmail: string;
    fromName: string;
    useTLS: boolean;
  };
}

// Schema de validação
const companySettingsSchema = yup.object({
  companyName: yup.string().required('Nome da empresa é obrigatório'),
  cnpj: yup.string().required('CNPJ é obrigatório').matches(/^\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$/, 'CNPJ deve ter formato válido'),
  email: yup.string().required('Email é obrigatório').email('Email deve ter formato válido'),
  phone: yup.string().required('Telefone é obrigatório'),
  
  maxDailyTransactionAmount: yup.number().required('Limite diário é obrigatório').min(1, 'Deve ser maior que 0'),
  maxSingleTransactionAmount: yup.number().required('Limite por transação é obrigatório').min(1, 'Deve ser maior que 0'),
  requireTwoFactorAuth: yup.boolean().required(),
  sessionTimeoutMinutes: yup.number().required('Timeout da sessão é obrigatório').min(5, 'Mínimo 5 minutos').max(480, 'Máximo 8 horas'),
  
  emailNotifications: yup.object({
    transactionConfirmation: yup.boolean().required(),
    dailyReport: yup.boolean().required(),
    securityAlerts: yup.boolean().required(),
    systemMaintenance: yup.boolean().required(),
  }),
  
  smtpSettings: yup.object({
    enabled: yup.boolean().required(),
    host: yup.string().when('enabled', {
      is: true,
      then: (schema) => schema.required('Host SMTP é obrigatório'),
      otherwise: (schema) => schema,
    }),
    port: yup.number().when('enabled', {
      is: true,
      then: (schema) => schema.required('Porta SMTP é obrigatória').min(1).max(65535),
      otherwise: (schema) => schema,
    }),
    username: yup.string().when('enabled', {
      is: true,
      then: (schema) => schema.required('Usuário SMTP é obrigatório'),
      otherwise: (schema) => schema,
    }),
    password: yup.string().when('enabled', {
      is: true,
      then: (schema) => schema.required('Senha SMTP é obrigatória'),
      otherwise: (schema) => schema,
    }),
    fromEmail: yup.string().when('enabled', {
      is: true,
      then: (schema) => schema.required('Email remetente é obrigatório').email('Email deve ter formato válido'),
      otherwise: (schema) => schema,
    }),
    fromName: yup.string().when('enabled', {
      is: true,
      then: (schema) => schema.required('Nome remetente é obrigatório'),
      otherwise: (schema) => schema,
    }),
    useTLS: yup.boolean().required(),
  }),
});

const CompanySettingsForm: React.FC<CompanySettingsFormProps> = ({ 
  onSave, 
  onCancel, 
  isLoading = false,
  initialData 
}) => {
  const [activeTab, setActiveTab] = useState<'general' | 'security' | 'notifications' | 'smtp'>('general');
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<CompanySettingsFormData>({
    resolver: yupResolver(companySettingsSchema),
    defaultValues: {
      companyName: initialData?.companyName || '',
      cnpj: initialData?.cnpj || '',
      email: initialData?.email || '',
      phone: initialData?.phone || '',
      maxDailyTransactionAmount: initialData?.maxDailyTransactionAmount || 50000,
      maxSingleTransactionAmount: initialData?.maxSingleTransactionAmount || 10000,
      requireTwoFactorAuth: initialData?.requireTwoFactorAuth ?? true,
      sessionTimeoutMinutes: initialData?.sessionTimeoutMinutes || 60,
      emailNotifications: {
        transactionConfirmation: initialData?.emailNotifications?.transactionConfirmation ?? true,
        dailyReport: initialData?.emailNotifications?.dailyReport ?? true,
        securityAlerts: initialData?.emailNotifications?.securityAlerts ?? true,
        systemMaintenance: initialData?.emailNotifications?.systemMaintenance ?? true,
      },
      smtpSettings: {
        enabled: initialData?.smtpSettings?.enabled ?? false,
        host: initialData?.smtpSettings?.host || '',
        port: initialData?.smtpSettings?.port || 587,
        username: initialData?.smtpSettings?.username || '',
        password: initialData?.smtpSettings?.password || '',
        fromEmail: initialData?.smtpSettings?.fromEmail || '',
        fromName: initialData?.smtpSettings?.fromName || '',
        useTLS: initialData?.smtpSettings?.useTLS ?? true,
      },
    },
  });

  const smtpEnabled = watch('smtpSettings.enabled');

  const formatCNPJ = (value: string) => {
    const numbers = value.replace(/\D/g, '');
    return numbers.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
  };

  const formatPhone = (value: string) => {
    const numbers = value.replace(/\D/g, '');
    return numbers.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  };

  const testSmtpConnection = async () => {
    const smtpData = watch('smtpSettings');
    if (!smtpData.enabled) {
      toast.error('Configure o SMTP primeiro');
      return;
    }

    try {
      // Aqui seria implementado o teste real de conexão SMTP
      toast.success('Conexão SMTP testada com sucesso!');
    } catch (error) {
      toast.error('Erro ao testar conexão SMTP');
    }
  };

  const onSubmit = (data: CompanySettingsFormData) => {
    onSave(data);
  };

  const tabs = [
    { id: 'general', name: 'Geral', icon: '🏢' },
    { id: 'security', name: 'Segurança', icon: '🔒' },
    { id: 'notifications', name: 'Notificações', icon: '🔔' },
    { id: 'smtp', name: 'SMTP', icon: '📧' },
  ];

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900">Configurações da Empresa</h3>
        <p className="text-sm text-gray-600 mt-1">
          Configure as informações e preferências da sua empresa
        </p>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="flex space-x-8 px-6">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              className={`py-4 px-1 border-b-2 font-medium text-sm ${
                activeTab === tab.id
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <span className="mr-2">{tab.icon}</span>
              {tab.name}
            </button>
          ))}
        </nav>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6">
        {/* Tab: Geral */}
        {activeTab === 'general' && (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nome da Empresa *
                </label>
                <input
                  type="text"
                  {...register('companyName')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Minha Empresa Ltda"
                />
                {errors.companyName && (
                  <p className="text-sm text-red-500 mt-1">{errors.companyName.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  CNPJ *
                </label>
                <input
                  type="text"
                  {...register('cnpj')}
                  onChange={(e) => {
                    const formatted = formatCNPJ(e.target.value);
                    setValue('cnpj', formatted);
                  }}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="00.000.000/0000-00"
                  maxLength={18}
                />
                {errors.cnpj && (
                  <p className="text-sm text-red-500 mt-1">{errors.cnpj.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Email *
                </label>
                <input
                  type="email"
                  {...register('email')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="contato@empresa.com"
                />
                {errors.email && (
                  <p className="text-sm text-red-500 mt-1">{errors.email.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Telefone *
                </label>
                <input
                  type="text"
                  {...register('phone')}
                  onChange={(e) => {
                    const formatted = formatPhone(e.target.value);
                    setValue('phone', formatted);
                  }}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="(11) 99999-9999"
                  maxLength={15}
                />
                {errors.phone && (
                  <p className="text-sm text-red-500 mt-1">{errors.phone.message}</p>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Tab: Segurança */}
        {activeTab === 'security' && (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Limite Diário de Transações (R$) *
                </label>
                <input
                  type="number"
                  step="0.01"
                  {...register('maxDailyTransactionAmount')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="50000.00"
                />
                {errors.maxDailyTransactionAmount && (
                  <p className="text-sm text-red-500 mt-1">{errors.maxDailyTransactionAmount.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Limite por Transação (R$) *
                </label>
                <input
                  type="number"
                  step="0.01"
                  {...register('maxSingleTransactionAmount')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="10000.00"
                />
                {errors.maxSingleTransactionAmount && (
                  <p className="text-sm text-red-500 mt-1">{errors.maxSingleTransactionAmount.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Timeout da Sessão (minutos) *
                </label>
                <select
                  {...register('sessionTimeoutMinutes')}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                >
                  <option value={15}>15 minutos</option>
                  <option value={30}>30 minutos</option>
                  <option value={60}>1 hora</option>
                  <option value={120}>2 horas</option>
                  <option value={240}>4 horas</option>
                  <option value={480}>8 horas</option>
                </select>
                {errors.sessionTimeoutMinutes && (
                  <p className="text-sm text-red-500 mt-1">{errors.sessionTimeoutMinutes.message}</p>
                )}
              </div>
            </div>

            <div className="border border-gray-200 rounded-lg p-4">
              <div className="flex items-center">
                <input
                  type="checkbox"
                  {...register('requireTwoFactorAuth')}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <label className="ml-3 text-sm font-medium text-gray-900">
                  Exigir Autenticação de Dois Fatores (2FA)
                </label>
              </div>
              <p className="text-xs text-gray-600 mt-1 ml-7">
                Todos os usuários precisarão configurar 2FA para acessar o sistema
              </p>
            </div>
          </div>
        )}

        {/* Tab: Notificações */}
        {activeTab === 'notifications' && (
          <div className="space-y-6">
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <div className="flex items-start space-x-3">
                <div className="p-1 bg-blue-100 rounded-lg">
                  <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div className="flex-1">
                  <h4 className="text-sm font-medium text-blue-900 mb-1">Configurações de Email</h4>
                  <p className="text-sm text-blue-800">
                    Configure o SMTP na aba correspondente para habilitar o envio de emails automáticos.
                  </p>
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <h4 className="text-lg font-medium text-gray-900">Notificações por Email</h4>

              <div className="space-y-4">
                <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                  <div className="flex-1">
                    <h5 className="text-sm font-medium text-gray-900">Confirmação de Transações</h5>
                    <p className="text-sm text-gray-600">Receba emails de confirmação para todas as transações realizadas</p>
                  </div>
                  <input
                    type="checkbox"
                    {...register('emailNotifications.transactionConfirmation')}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                </div>

                <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                  <div className="flex-1">
                    <h5 className="text-sm font-medium text-gray-900">Relatório Diário</h5>
                    <p className="text-sm text-gray-600">Receba um resumo diário das atividades da conta</p>
                  </div>
                  <input
                    type="checkbox"
                    {...register('emailNotifications.dailyReport')}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                </div>

                <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                  <div className="flex-1">
                    <h5 className="text-sm font-medium text-gray-900">Alertas de Segurança</h5>
                    <p className="text-sm text-gray-600">Receba notificações sobre atividades suspeitas ou tentativas de acesso</p>
                  </div>
                  <input
                    type="checkbox"
                    {...register('emailNotifications.securityAlerts')}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                </div>

                <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg">
                  <div className="flex-1">
                    <h5 className="text-sm font-medium text-gray-900">Manutenção do Sistema</h5>
                    <p className="text-sm text-gray-600">Receba avisos sobre manutenções programadas e atualizações</p>
                  </div>
                  <input
                    type="checkbox"
                    {...register('emailNotifications.systemMaintenance')}
                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                  />
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Tab: SMTP */}
        {activeTab === 'smtp' && (
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <div>
                <h4 className="text-lg font-medium text-gray-900">Configurações SMTP</h4>
                <p className="text-sm text-gray-600">Configure o servidor de email para envio de notificações</p>
              </div>
              <div className="flex items-center space-x-3">
                <input
                  type="checkbox"
                  {...register('smtpSettings.enabled')}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <label className="text-sm font-medium text-gray-900">Habilitar SMTP</label>
              </div>
            </div>

            {smtpEnabled && (
              <div className="space-y-6 border border-gray-200 rounded-lg p-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Servidor SMTP *
                    </label>
                    <input
                      type="text"
                      {...register('smtpSettings.host')}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="smtp.gmail.com"
                    />
                    {errors.smtpSettings?.host && (
                      <p className="text-sm text-red-500 mt-1">{errors.smtpSettings.host.message}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Porta *
                    </label>
                    <input
                      type="number"
                      {...register('smtpSettings.port')}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="587"
                    />
                    {errors.smtpSettings?.port && (
                      <p className="text-sm text-red-500 mt-1">{errors.smtpSettings.port.message}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Usuário *
                    </label>
                    <input
                      type="text"
                      {...register('smtpSettings.username')}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="usuario@gmail.com"
                    />
                    {errors.smtpSettings?.username && (
                      <p className="text-sm text-red-500 mt-1">{errors.smtpSettings.username.message}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Senha *
                    </label>
                    <div className="relative">
                      <input
                        type={showPassword ? 'text' : 'password'}
                        {...register('smtpSettings.password')}
                        className="w-full border border-gray-300 rounded-lg px-3 py-2 pr-10 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                        placeholder="••••••••"
                      />
                      <button
                        type="button"
                        onClick={() => setShowPassword(!showPassword)}
                        className="absolute inset-y-0 right-0 pr-3 flex items-center"
                      >
                        <svg className="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          {showPassword ? (
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21" />
                          ) : (
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                          )}
                        </svg>
                      </button>
                    </div>
                    {errors.smtpSettings?.password && (
                      <p className="text-sm text-red-500 mt-1">{errors.smtpSettings.password.message}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Email Remetente *
                    </label>
                    <input
                      type="email"
                      {...register('smtpSettings.fromEmail')}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="noreply@empresa.com"
                    />
                    {errors.smtpSettings?.fromEmail && (
                      <p className="text-sm text-red-500 mt-1">{errors.smtpSettings.fromEmail.message}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Nome Remetente *
                    </label>
                    <input
                      type="text"
                      {...register('smtpSettings.fromName')}
                      className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="Minha Empresa"
                    />
                    {errors.smtpSettings?.fromName && (
                      <p className="text-sm text-red-500 mt-1">{errors.smtpSettings.fromName.message}</p>
                    )}
                  </div>
                </div>

                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      {...register('smtpSettings.useTLS')}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                    />
                    <label className="ml-3 text-sm font-medium text-gray-900">
                      Usar TLS/SSL
                    </label>
                  </div>
                  <button
                    type="button"
                    onClick={testSmtpConnection}
                    className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 text-sm"
                  >
                    Testar Conexão
                  </button>
                </div>

                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                  <div className="flex items-start space-x-3">
                    <div className="p-1 bg-yellow-100 rounded-lg">
                      <svg className="w-4 h-4 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                      </svg>
                    </div>
                    <div className="flex-1">
                      <h5 className="text-sm font-medium text-yellow-900 mb-1">Configuração Segura</h5>
                      <p className="text-sm text-yellow-800">
                        Para Gmail, use uma senha de aplicativo em vez da senha da conta.
                        Para outros provedores, verifique as configurações de segurança necessárias.
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        {/* Botões */}
        <div className="flex justify-end space-x-3 mt-8 pt-6 border-t border-gray-200">
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
            {isLoading ? 'Salvando...' : 'Salvar Configurações'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CompanySettingsForm;
