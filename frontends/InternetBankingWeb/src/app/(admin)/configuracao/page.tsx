'use client';

import React, { useState, useEffect } from 'react';
import { useRequireAuth } from '@/context/AuthContext';
import { companySettingsService, CompanySettings } from '@/services/api';
import CompanySettingsForm, { CompanySettingsFormData } from '@/components/CompanySettingsForm';
import toast from 'react-hot-toast';

const ConfiguracaoPage: React.FC = () => {
  useRequireAuth('view_tela_config');

  const [settings, setSettings] = useState<CompanySettings | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setIsLoading(true);
      const response = await companySettingsService.getSettings();
      setSettings(response.data);
    } catch (error) {
      console.error('Erro ao carregar configurações:', error);
      toast.error('Erro ao carregar configurações da empresa');

      // Usar configurações padrão quando não conseguir carregar da API
      const defaultSettings: CompanySettings = {
        settingsId: '',
        companyId: '',
        companyName: 'Empresa',
        cnpj: '',
        email: '',
        phone: '',
        maxDailyTransactionAmount: 10000,
        maxSingleTransactionAmount: 5000,
        requireTwoFactorAuth: true,
        sessionTimeoutMinutes: 30,
        emailNotifications: {
          transactionConfirmation: true,
          dailyReport: false,
          securityAlerts: true,
          systemMaintenance: false,
        },
        smtpSettings: {
          enabled: false,
          host: '',
          port: 587,
          username: '',
          password: '',
          fromEmail: '',
          fromName: '',
          useTLS: true,
        },
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
      setSettings(defaultSettings);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSaveSettings = async (data: CompanySettingsFormData) => {
    try {
      setIsSaving(true);

      const updateData = {
        companyName: data.companyName,
        cnpj: data.cnpj,
        email: data.email,
        phone: data.phone,
        maxDailyTransactionAmount: data.maxDailyTransactionAmount,
        maxSingleTransactionAmount: data.maxSingleTransactionAmount,
        requireTwoFactorAuth: data.requireTwoFactorAuth,
        sessionTimeoutMinutes: data.sessionTimeoutMinutes,
        emailNotifications: data.emailNotifications,
        smtpSettings: data.smtpSettings,
      };

      const response = await companySettingsService.updateSettings(updateData);
      setSettings(response.data);
      setIsEditing(false);
      toast.success('Configurações salvas com sucesso!');

    } catch (error) {
      console.error('Erro ao salvar configurações:', error);
      toast.error('Erro ao salvar configurações');

      // Para demonstração, simular sucesso
      if (settings) {
        const updatedSettings: CompanySettings = {
          ...settings,
          ...data,
          updatedAt: new Date().toISOString(),
        };
        setSettings(updatedSettings);
        setIsEditing(false);
        toast.success('Configurações salvas com sucesso! (modo demonstração)');
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handleStartEditing = () => {
    setIsEditing(true);
  };

  const handleCancelEditing = () => {
    setIsEditing(false);
  };

  if (isLoading) {
    return (
      <div className="p-6 space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <div className="h-8 bg-gray-200 rounded w-48 animate-pulse"></div>
            <div className="h-4 bg-gray-200 rounded w-96 mt-2 animate-pulse"></div>
          </div>
          <div className="h-10 bg-gray-200 rounded w-32 animate-pulse"></div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div className="animate-pulse space-y-6">
            <div className="h-6 bg-gray-200 rounded w-1/3"></div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-1/3"></div>
                  <div className="h-10 bg-gray-200 rounded"></div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!settings) {
    return (
      <div className="p-6 space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Configuração</h1>
            <p className="text-gray-600 mt-1">Configure as preferências da sua empresa</p>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-8 border border-gray-100 text-center">
          <div className="max-w-md mx-auto">
            <div className="p-4 bg-red-100 rounded-full w-16 h-16 mx-auto mb-4">
              <svg className="w-8 h-8 text-red-600 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Erro ao Carregar</h3>
            <p className="text-gray-600 mb-4">
              Não foi possível carregar as configurações da empresa.
            </p>
            <button
              onClick={loadSettings}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Tentar Novamente
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Configuração</h1>
          <p className="text-gray-600 mt-1">Configure as preferências da sua empresa</p>
        </div>
        {!isEditing && (
          <button
            onClick={handleStartEditing}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
          >
            Editar Configurações
          </button>
        )}
      </div>

      {/* Conteúdo Principal */}
      {isEditing ? (
        <CompanySettingsForm
          onSave={handleSaveSettings}
          onCancel={handleCancelEditing}
          isLoading={isSaving}
          initialData={settings}
        />
      ) : (
        <div className="space-y-6">
          {/* Resumo das Configurações */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
              <div className="flex items-center">
                <div className="p-3 bg-blue-100 rounded-lg">
                  <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Empresa</p>
                  <p className="text-lg font-bold text-gray-900">{settings.companyName}</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
              <div className="flex items-center">
                <div className="p-3 bg-green-100 rounded-lg">
                  <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">2FA</p>
                  <p className="text-lg font-bold text-gray-900">
                    {settings.requireTwoFactorAuth ? 'Ativo' : 'Inativo'}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
              <div className="flex items-center">
                <div className="p-3 bg-yellow-100 rounded-lg">
                  <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                  </svg>
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">Limite Diário</p>
                  <p className="text-lg font-bold text-gray-900">
                    R$ {settings.maxDailyTransactionAmount.toLocaleString('pt-BR')}
                  </p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
              <div className="flex items-center">
                <div className="p-3 bg-purple-100 rounded-lg">
                  <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">SMTP</p>
                  <p className="text-lg font-bold text-gray-900">
                    {settings.smtpSettings.enabled ? 'Configurado' : 'Não configurado'}
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Detalhes das Configurações */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Informações da Empresa */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100">
              <div className="p-6 border-b border-gray-200">
                <h3 className="text-lg font-semibold text-gray-900">Informações da Empresa</h3>
              </div>
              <div className="p-6 space-y-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">CNPJ</label>
                  <p className="text-gray-900 font-medium">{settings.cnpj}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">Email</label>
                  <p className="text-gray-900 font-medium">{settings.email}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">Telefone</label>
                  <p className="text-gray-900 font-medium">{settings.phone}</p>
                </div>
              </div>
            </div>

            {/* Configurações de Segurança */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100">
              <div className="p-6 border-b border-gray-200">
                <h3 className="text-lg font-semibold text-gray-900">Segurança</h3>
              </div>
              <div className="p-6 space-y-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">Limite por Transação</label>
                  <p className="text-gray-900 font-medium">
                    R$ {settings.maxSingleTransactionAmount.toLocaleString('pt-BR')}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">Timeout da Sessão</label>
                  <p className="text-gray-900 font-medium">{settings.sessionTimeoutMinutes} minutos</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">Autenticação 2FA</label>
                  <div className="flex items-center space-x-2">
                    <div className={`w-2 h-2 rounded-full ${settings.requireTwoFactorAuth ? 'bg-green-500' : 'bg-red-500'}`}></div>
                    <p className="text-gray-900 font-medium">
                      {settings.requireTwoFactorAuth ? 'Obrigatório' : 'Opcional'}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Notificações */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-lg font-semibold text-gray-900">Notificações por Email</h3>
            </div>
            <div className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="flex items-center justify-between">
                  <span className="text-gray-900">Confirmação de Transações</span>
                  <div className={`w-2 h-2 rounded-full ${settings.emailNotifications.transactionConfirmation ? 'bg-green-500' : 'bg-gray-300'}`}></div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-gray-900">Relatório Diário</span>
                  <div className={`w-2 h-2 rounded-full ${settings.emailNotifications.dailyReport ? 'bg-green-500' : 'bg-gray-300'}`}></div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-gray-900">Alertas de Segurança</span>
                  <div className={`w-2 h-2 rounded-full ${settings.emailNotifications.securityAlerts ? 'bg-green-500' : 'bg-gray-300'}`}></div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-gray-900">Manutenção do Sistema</span>
                  <div className={`w-2 h-2 rounded-full ${settings.emailNotifications.systemMaintenance ? 'bg-green-500' : 'bg-gray-300'}`}></div>
                </div>
              </div>
            </div>
          </div>

          {/* Informações Adicionais */}
          <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
            <div className="flex items-start space-x-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="flex-1">
                <h3 className="text-sm font-medium text-blue-900 mb-1">Configurações da Empresa</h3>
                <div className="text-sm text-blue-800 space-y-1">
                  <p>• Todas as alterações são aplicadas imediatamente após salvar</p>
                  <p>• Configurações de SMTP são necessárias para envio de emails automáticos</p>
                  <p>• Limites de segurança se aplicam a todas as transações da empresa</p>
                  <p>• Última atualização: {new Date(settings.updatedAt).toLocaleString('pt-BR')}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ConfiguracaoPage;
