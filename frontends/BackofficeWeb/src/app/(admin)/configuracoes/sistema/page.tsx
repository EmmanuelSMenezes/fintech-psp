'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import toast from 'react-hot-toast';

interface SystemConfig {
  general: {
    systemName: string;
    systemVersion: string;
    environment: 'development' | 'staging' | 'production';
    maintenanceMode: boolean;
    debugMode: boolean;
    logLevel: 'error' | 'warn' | 'info' | 'debug';
  };
  security: {
    jwtExpirationTime: number;
    maxLoginAttempts: number;
    passwordMinLength: number;
    requireTwoFactor: boolean;
    sessionTimeout: number;
    ipWhitelist: string[];
  };
  notifications: {
    emailEnabled: boolean;
    smsEnabled: boolean;
    webhookEnabled: boolean;
    slackEnabled: boolean;
    emailProvider: 'smtp' | 'sendgrid' | 'ses';
    smtpHost: string;
    smtpPort: number;
    smtpUser: string;
    smtpPassword: string;
  };
  limits: {
    maxTransactionsPerDay: number;
    maxAmountPerTransaction: number;
    maxAccountsPerClient: number;
    rateLimitPerMinute: number;
    maxWebhooksPerClient: number;
  };
  integrations: {
    starkBankEnabled: boolean;
    sicoobEnabled: boolean;
    bancoGenialEnabled: boolean;
    efiEnabled: boolean;
    celcoinEnabled: boolean;
    cryptoEnabled: boolean;
    qrCodeEnabled: boolean;
  };
}

const ConfiguracoesSistemaPage: React.FC = () => {
  useRequireAuth('manage_system_config');
  const { user } = useAuth();
  const [config, setConfig] = useState<SystemConfig>({
    general: {
      systemName: 'FintechPSP',
      systemVersion: '2.1.0',
      environment: 'production',
      maintenanceMode: false,
      debugMode: false,
      logLevel: 'info'
    },
    security: {
      jwtExpirationTime: 3600,
      maxLoginAttempts: 5,
      passwordMinLength: 8,
      requireTwoFactor: false,
      sessionTimeout: 1800,
      ipWhitelist: []
    },
    notifications: {
      emailEnabled: true,
      smsEnabled: false,
      webhookEnabled: true,
      slackEnabled: false,
      emailProvider: 'smtp',
      smtpHost: 'smtp.gmail.com',
      smtpPort: 587,
      smtpUser: '',
      smtpPassword: ''
    },
    limits: {
      maxTransactionsPerDay: 10000,
      maxAmountPerTransaction: 100000,
      maxAccountsPerClient: 10,
      rateLimitPerMinute: 100,
      maxWebhooksPerClient: 5
    },
    integrations: {
      starkBankEnabled: true,
      sicoobEnabled: true,
      bancoGenialEnabled: true,
      efiEnabled: true,
      celcoinEnabled: true,
      cryptoEnabled: false,
      qrCodeEnabled: true
    }
  });
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [activeTab, setActiveTab] = useState('general');

  const tabs = [
    { id: 'general', label: 'Geral', icon: '⚙️' },
    { id: 'security', label: 'Segurança', icon: '🔒' },
    { id: 'notifications', label: 'Notificações', icon: '📧' },
    { id: 'limits', label: 'Limites', icon: '📊' },
    { id: 'integrations', label: 'Integrações', icon: '🔗' }
  ];

  useEffect(() => {
    loadConfig();
  }, []);

  const loadConfig = async () => {
    try {
      setIsLoading(true);
      // Simular carregamento da configuração
      await new Promise(resolve => setTimeout(resolve, 1000));
      // Config já está definido no estado inicial
    } catch (error) {
      console.error('Erro ao carregar configurações:', error);
      toast.error('Erro ao carregar configurações');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setIsSaving(true);
      // Simular salvamento
      await new Promise(resolve => setTimeout(resolve, 1500));
      toast.success('Configurações salvas com sucesso!');
    } catch (error) {
      console.error('Erro ao salvar configurações:', error);
      toast.error('Erro ao salvar configurações');
    } finally {
      setIsSaving(false);
    }
  };

  const handleConfigChange = (section: keyof SystemConfig, field: string, value: any) => {
    setConfig(prev => ({
      ...prev,
      [section]: {
        ...prev[section],
        [field]: value
      }
    }));
  };

  const handleArrayAdd = (section: keyof SystemConfig, field: string, value: string) => {
    if (!value.trim()) return;
    
    setConfig(prev => ({
      ...prev,
      [section]: {
        ...prev[section],
        [field]: [...(prev[section][field] as string[]), value.trim()]
      }
    }));
  };

  const handleArrayRemove = (section: keyof SystemConfig, field: string, index: number) => {
    setConfig(prev => ({
      ...prev,
      [section]: {
        ...prev[section],
        [field]: (prev[section][field] as string[]).filter((_, i) => i !== index)
      }
    }));
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Configurações do Sistema</h1>
          <p className="text-gray-600 mt-1">Gerencie as configurações globais do PSP</p>
        </div>
        <button
          onClick={handleSave}
          disabled={isSaving}
          className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white px-6 py-2 rounded-lg font-medium flex items-center space-x-2"
        >
          {isSaving ? (
            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
          ) : (
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          )}
          <span>{isSaving ? 'Salvando...' : 'Salvar Configurações'}</span>
        </button>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="border-b border-gray-200">
          <nav className="flex space-x-8 px-6">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`py-4 px-1 border-b-2 font-medium text-sm flex items-center space-x-2 ${
                  activeTab === tab.id
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <span>{tab.icon}</span>
                <span>{tab.label}</span>
              </button>
            ))}
          </nav>
        </div>

        <div className="p-6">
          {/* General Tab */}
          {activeTab === 'general' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Nome do Sistema
                  </label>
                  <input
                    type="text"
                    value={config.general.systemName}
                    onChange={(e) => handleConfigChange('general', 'systemName', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Versão
                  </label>
                  <input
                    type="text"
                    value={config.general.systemVersion}
                    onChange={(e) => handleConfigChange('general', 'systemVersion', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Ambiente
                  </label>
                  <select
                    value={config.general.environment}
                    onChange={(e) => handleConfigChange('general', 'environment', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="development">Desenvolvimento</option>
                    <option value="staging">Homologação</option>
                    <option value="production">Produção</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Nível de Log
                  </label>
                  <select
                    value={config.general.logLevel}
                    onChange={(e) => handleConfigChange('general', 'logLevel', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="error">Error</option>
                    <option value="warn">Warning</option>
                    <option value="info">Info</option>
                    <option value="debug">Debug</option>
                  </select>
                </div>
              </div>
              
              <div className="space-y-4">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={config.general.maintenanceMode}
                    onChange={(e) => handleConfigChange('general', 'maintenanceMode', e.target.checked)}
                    className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <span className="ml-2 text-sm text-gray-700">Modo de Manutenção</span>
                </label>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={config.general.debugMode}
                    onChange={(e) => handleConfigChange('general', 'debugMode', e.target.checked)}
                    className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <span className="ml-2 text-sm text-gray-700">Modo Debug</span>
                </label>
              </div>
            </div>
          )}

          {/* Security Tab */}
          {activeTab === 'security' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tempo de Expiração JWT (segundos)
                  </label>
                  <input
                    type="number"
                    value={config.security.jwtExpirationTime}
                    onChange={(e) => handleConfigChange('security', 'jwtExpirationTime', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Máximo de Tentativas de Login
                  </label>
                  <input
                    type="number"
                    value={config.security.maxLoginAttempts}
                    onChange={(e) => handleConfigChange('security', 'maxLoginAttempts', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tamanho Mínimo da Senha
                  </label>
                  <input
                    type="number"
                    value={config.security.passwordMinLength}
                    onChange={(e) => handleConfigChange('security', 'passwordMinLength', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Timeout de Sessão (segundos)
                  </label>
                  <input
                    type="number"
                    value={config.security.sessionTimeout}
                    onChange={(e) => handleConfigChange('security', 'sessionTimeout', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
              
              <div>
                <label className="flex items-center mb-4">
                  <input
                    type="checkbox"
                    checked={config.security.requireTwoFactor}
                    onChange={(e) => handleConfigChange('security', 'requireTwoFactor', e.target.checked)}
                    className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <span className="ml-2 text-sm text-gray-700">Exigir Autenticação de Dois Fatores</span>
                </label>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Lista Branca de IPs
                </label>
                <div className="space-y-2">
                  {config.security.ipWhitelist.map((ip, index) => (
                    <div key={index} className="flex items-center space-x-2">
                      <input
                        type="text"
                        value={ip}
                        readOnly
                        className="flex-1 px-3 py-2 border border-gray-300 rounded-lg bg-gray-50"
                      />
                      <button
                        onClick={() => handleArrayRemove('security', 'ipWhitelist', index)}
                        className="text-red-600 hover:text-red-800"
                      >
                        Remover
                      </button>
                    </div>
                  ))}
                  <div className="flex items-center space-x-2">
                    <input
                      type="text"
                      placeholder="192.168.1.1"
                      onKeyPress={(e) => {
                        if (e.key === 'Enter') {
                          handleArrayAdd('security', 'ipWhitelist', e.currentTarget.value);
                          e.currentTarget.value = '';
                        }
                      }}
                      className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                    <button
                      onClick={(e) => {
                        const input = e.currentTarget.previousElementSibling as HTMLInputElement;
                        handleArrayAdd('security', 'ipWhitelist', input.value);
                        input.value = '';
                      }}
                      className="bg-blue-600 hover:bg-blue-700 text-white px-3 py-2 rounded-lg"
                    >
                      Adicionar
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Notifications Tab */}
          {activeTab === 'notifications' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.notifications.emailEnabled}
                      onChange={(e) => handleConfigChange('notifications', 'emailEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Email Habilitado</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.notifications.smsEnabled}
                      onChange={(e) => handleConfigChange('notifications', 'smsEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">SMS Habilitado</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.notifications.webhookEnabled}
                      onChange={(e) => handleConfigChange('notifications', 'webhookEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Webhook Habilitado</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.notifications.slackEnabled}
                      onChange={(e) => handleConfigChange('notifications', 'slackEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Slack Habilitado</span>
                  </label>
                </div>

                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Provedor de Email
                    </label>
                    <select
                      value={config.notifications.emailProvider}
                      onChange={(e) => handleConfigChange('notifications', 'emailProvider', e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="smtp">SMTP</option>
                      <option value="sendgrid">SendGrid</option>
                      <option value="ses">Amazon SES</option>
                    </select>
                  </div>
                </div>
              </div>

              {config.notifications.emailProvider === 'smtp' && (
                <div className="border-t pt-6">
                  <h4 className="text-md font-medium text-gray-900 mb-4">Configurações SMTP</h4>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Host SMTP
                      </label>
                      <input
                        type="text"
                        value={config.notifications.smtpHost}
                        onChange={(e) => handleConfigChange('notifications', 'smtpHost', e.target.value)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Porta SMTP
                      </label>
                      <input
                        type="number"
                        value={config.notifications.smtpPort}
                        onChange={(e) => handleConfigChange('notifications', 'smtpPort', parseInt(e.target.value))}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Usuário SMTP
                      </label>
                      <input
                        type="text"
                        value={config.notifications.smtpUser}
                        onChange={(e) => handleConfigChange('notifications', 'smtpUser', e.target.value)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Senha SMTP
                      </label>
                      <input
                        type="password"
                        value={config.notifications.smtpPassword}
                        onChange={(e) => handleConfigChange('notifications', 'smtpPassword', e.target.value)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      />
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Limits Tab */}
          {activeTab === 'limits' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Máximo de Transações por Dia
                  </label>
                  <input
                    type="number"
                    value={config.limits.maxTransactionsPerDay}
                    onChange={(e) => handleConfigChange('limits', 'maxTransactionsPerDay', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Valor Máximo por Transação (R$)
                  </label>
                  <input
                    type="number"
                    value={config.limits.maxAmountPerTransaction}
                    onChange={(e) => handleConfigChange('limits', 'maxAmountPerTransaction', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Máximo de Contas por Cliente
                  </label>
                  <input
                    type="number"
                    value={config.limits.maxAccountsPerClient}
                    onChange={(e) => handleConfigChange('limits', 'maxAccountsPerClient', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Rate Limit por Minuto
                  </label>
                  <input
                    type="number"
                    value={config.limits.rateLimitPerMinute}
                    onChange={(e) => handleConfigChange('limits', 'rateLimitPerMinute', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Máximo de Webhooks por Cliente
                  </label>
                  <input
                    type="number"
                    value={config.limits.maxWebhooksPerClient}
                    onChange={(e) => handleConfigChange('limits', 'maxWebhooksPerClient', parseInt(e.target.value))}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
            </div>
          )}

          {/* Integrations Tab */}
          {activeTab === 'integrations' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h4 className="text-md font-medium text-gray-900">Bancos</h4>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.starkBankEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'starkBankEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Stark Bank</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.sicoobEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'sicoobEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Sicoob</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.bancoGenialEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'bancoGenialEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Banco Genial</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.efiEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'efiEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Efí (Gerencianet)</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.celcoinEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'celcoinEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Celcoin</span>
                  </label>
                </div>

                <div className="space-y-4">
                  <h4 className="text-md font-medium text-gray-900">Funcionalidades</h4>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.cryptoEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'cryptoEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">Transações Crypto</span>
                  </label>
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={config.integrations.qrCodeEnabled}
                      onChange={(e) => handleConfigChange('integrations', 'qrCodeEnabled', e.target.checked)}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                    <span className="ml-2 text-sm text-gray-700">QR Codes</span>
                  </label>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConfiguracoesSistemaPage;
