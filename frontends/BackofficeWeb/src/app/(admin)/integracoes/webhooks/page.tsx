'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import toast from 'react-hot-toast';

interface Webhook {
  id: string;
  url: string;
  events: string[];
  status: 'active' | 'inactive' | 'failed';
  secret: string;
  createdAt: string;
  lastTriggered?: string;
  successCount: number;
  failureCount: number;
  retryCount: number;
}

interface WebhookLog {
  id: string;
  webhookId: string;
  event: string;
  status: 'success' | 'failed' | 'pending';
  responseCode?: number;
  responseTime: number;
  payload: any;
  response?: string;
  createdAt: string;
  retryCount: number;
}

const WebhooksPage: React.FC = () => {
  useRequireAuth('manage_webhooks');
  const { user } = useAuth();
  const [webhooks, setWebhooks] = useState<Webhook[]>([]);
  const [webhookLogs, setWebhookLogs] = useState<WebhookLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showLogsModal, setShowLogsModal] = useState(false);
  const [selectedWebhook, setSelectedWebhook] = useState<string>('');
  const [newWebhook, setNewWebhook] = useState({
    url: '',
    events: [] as string[],
    secret: ''
  });

  const availableEvents = [
    { value: 'transaction.created', label: 'Transação Criada' },
    { value: 'transaction.completed', label: 'Transação Concluída' },
    { value: 'transaction.failed', label: 'Transação Falhou' },
    { value: 'transaction.cancelled', label: 'Transação Cancelada' },
    { value: 'pix.received', label: 'PIX Recebido' },
    { value: 'pix.sent', label: 'PIX Enviado' },
    { value: 'boleto.paid', label: 'Boleto Pago' },
    { value: 'boleto.expired', label: 'Boleto Expirado' },
    { value: 'account.created', label: 'Conta Criada' },
    { value: 'account.updated', label: 'Conta Atualizada' },
    { value: 'balance.updated', label: 'Saldo Atualizado' }
  ];

  const mockWebhooks: Webhook[] = [
    {
      id: 'wh_001',
      url: 'https://api.cliente1.com/webhooks/psp',
      events: ['transaction.completed', 'transaction.failed', 'pix.received'],
      status: 'active',
      secret: 'wh_secret_***',
      createdAt: '2024-01-15T10:30:00Z',
      lastTriggered: '2024-01-17T14:22:00Z',
      successCount: 1250,
      failureCount: 15,
      retryCount: 3
    },
    {
      id: 'wh_002',
      url: 'https://webhook.cliente2.com/notifications',
      events: ['transaction.created', 'boleto.paid'],
      status: 'active',
      secret: 'wh_secret_***',
      createdAt: '2024-01-10T08:15:00Z',
      lastTriggered: '2024-01-17T13:45:00Z',
      successCount: 890,
      failureCount: 8,
      retryCount: 2
    },
    {
      id: 'wh_003',
      url: 'https://api.cliente3.com/psp-events',
      events: ['pix.sent', 'pix.received', 'balance.updated'],
      status: 'failed',
      secret: 'wh_secret_***',
      createdAt: '2024-01-12T16:20:00Z',
      lastTriggered: '2024-01-17T12:10:00Z',
      successCount: 450,
      failureCount: 125,
      retryCount: 5
    }
  ];

  const mockWebhookLogs: WebhookLog[] = [
    {
      id: 'log_001',
      webhookId: 'wh_001',
      event: 'transaction.completed',
      status: 'success',
      responseCode: 200,
      responseTime: 245,
      payload: { transactionId: 'txn_123', amount: 1500.00, status: 'completed' },
      response: 'OK',
      createdAt: '2024-01-17T14:22:00Z',
      retryCount: 0
    },
    {
      id: 'log_002',
      webhookId: 'wh_001',
      event: 'pix.received',
      status: 'success',
      responseCode: 200,
      responseTime: 180,
      payload: { pixId: 'pix_456', amount: 750.00, sender: 'João Silva' },
      response: 'Received',
      createdAt: '2024-01-17T14:20:00Z',
      retryCount: 0
    },
    {
      id: 'log_003',
      webhookId: 'wh_003',
      event: 'pix.sent',
      status: 'failed',
      responseCode: 500,
      responseTime: 5000,
      payload: { pixId: 'pix_789', amount: 2000.00, recipient: 'Maria Santos' },
      response: 'Internal Server Error',
      createdAt: '2024-01-17T12:10:00Z',
      retryCount: 3
    }
  ];

  useEffect(() => {
    loadWebhooks();
  }, []);

  const loadWebhooks = async () => {
    try {
      setIsLoading(true);
      // Simular carregamento
      await new Promise(resolve => setTimeout(resolve, 1000));
      setWebhooks(mockWebhooks);
    } catch (error) {
      console.error('Erro ao carregar webhooks:', error);
      toast.error('Erro ao carregar webhooks');
    } finally {
      setIsLoading(false);
    }
  };

  const loadWebhookLogs = async (webhookId: string) => {
    try {
      const logs = mockWebhookLogs.filter(log => log.webhookId === webhookId);
      setWebhookLogs(logs);
      setSelectedWebhook(webhookId);
      setShowLogsModal(true);
    } catch (error) {
      console.error('Erro ao carregar logs:', error);
      toast.error('Erro ao carregar logs');
    }
  };

  const handleCreateWebhook = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const webhook: Webhook = {
        id: `wh_${Date.now()}`,
        url: newWebhook.url,
        events: newWebhook.events,
        status: 'active',
        secret: newWebhook.secret || `wh_secret_${Math.random().toString(36).substr(2, 9)}`,
        createdAt: new Date().toISOString(),
        successCount: 0,
        failureCount: 0,
        retryCount: 3
      };
      
      setWebhooks([...webhooks, webhook]);
      toast.success('Webhook criado com sucesso!');
      setShowCreateModal(false);
      setNewWebhook({ url: '', events: [], secret: '' });
    } catch (error) {
      console.error('Erro ao criar webhook:', error);
      toast.error('Erro ao criar webhook');
    }
  };

  const handleDeleteWebhook = async (id: string) => {
    if (!confirm('Tem certeza que deseja excluir este webhook?')) return;
    
    try {
      setWebhooks(webhooks.filter(w => w.id !== id));
      toast.success('Webhook excluído com sucesso!');
    } catch (error) {
      console.error('Erro ao excluir webhook:', error);
      toast.error('Erro ao excluir webhook');
    }
  };

  const handleToggleWebhook = async (id: string) => {
    try {
      setWebhooks(webhooks.map(w => 
        w.id === id 
          ? { ...w, status: w.status === 'active' ? 'inactive' : 'active' }
          : w
      ));
      toast.success('Status do webhook atualizado!');
    } catch (error) {
      console.error('Erro ao atualizar webhook:', error);
      toast.error('Erro ao atualizar webhook');
    }
  };

  const handleTestWebhook = async (webhook: Webhook) => {
    try {
      toast.loading('Testando webhook...', { id: 'test-webhook' });
      
      // Simular teste
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      toast.success('Webhook testado com sucesso!', { id: 'test-webhook' });
    } catch (error) {
      console.error('Erro ao testar webhook:', error);
      toast.error('Erro ao testar webhook', { id: 'test-webhook' });
    }
  };

  const handleEventToggle = (event: string) => {
    const newEvents = newWebhook.events.includes(event)
      ? newWebhook.events.filter(e => e !== event)
      : [...newWebhook.events, event];
    
    setNewWebhook({ ...newWebhook, events: newEvents });
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active':
        return 'bg-green-100 text-green-800';
      case 'inactive':
        return 'bg-gray-100 text-gray-800';
      case 'failed':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'active':
        return 'Ativo';
      case 'inactive':
        return 'Inativo';
      case 'failed':
        return 'Falhou';
      default:
        return 'Desconhecido';
    }
  };

  const getLogStatusColor = (status: string) => {
    switch (status) {
      case 'success':
        return 'bg-green-100 text-green-800';
      case 'failed':
        return 'bg-red-100 text-red-800';
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('pt-BR');
  };

  const getEventLabel = (event: string) => {
    const eventInfo = availableEvents.find(e => e.value === event);
    return eventInfo?.label || event;
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
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Webhooks</h1>
          <p className="text-gray-600 mt-1">Configure e monitore notificações em tempo real</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
        >
          Novo Webhook
        </button>
      </div>

      {/* Webhooks List */}
      <div className="space-y-4">
        {webhooks.map((webhook) => (
          <div key={webhook.id} className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <div className="flex items-center space-x-3 mb-2">
                  <h3 className="text-lg font-semibold text-gray-900">{webhook.url}</h3>
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(webhook.status)}`}>
                    {getStatusLabel(webhook.status)}
                  </span>
                </div>
                
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
                  <div>
                    <div className="text-sm text-gray-500">Eventos</div>
                    <div className="text-sm font-medium text-gray-900">{webhook.events.length} configurados</div>
                  </div>
                  <div>
                    <div className="text-sm text-gray-500">Sucessos</div>
                    <div className="text-sm font-medium text-green-600">{webhook.successCount}</div>
                  </div>
                  <div>
                    <div className="text-sm text-gray-500">Falhas</div>
                    <div className="text-sm font-medium text-red-600">{webhook.failureCount}</div>
                  </div>
                  <div>
                    <div className="text-sm text-gray-500">Último Disparo</div>
                    <div className="text-sm font-medium text-gray-900">
                      {webhook.lastTriggered ? formatDate(webhook.lastTriggered) : 'Nunca'}
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2">
                  {webhook.events.map((event) => (
                    <span key={event} className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-blue-100 text-blue-800">
                      {getEventLabel(event)}
                    </span>
                  ))}
                </div>
              </div>

              <div className="flex items-center space-x-2 ml-4">
                <button
                  onClick={() => loadWebhookLogs(webhook.id)}
                  className="text-blue-600 hover:text-blue-900 text-sm"
                >
                  Ver Logs
                </button>
                <button
                  onClick={() => handleTestWebhook(webhook)}
                  className="text-green-600 hover:text-green-900 text-sm"
                >
                  Testar
                </button>
                <button
                  onClick={() => handleToggleWebhook(webhook.id)}
                  className="text-yellow-600 hover:text-yellow-900 text-sm"
                >
                  {webhook.status === 'active' ? 'Desativar' : 'Ativar'}
                </button>
                <button
                  onClick={() => handleDeleteWebhook(webhook.id)}
                  className="text-red-600 hover:text-red-900 text-sm"
                >
                  Excluir
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Empty State */}
      {webhooks.length === 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="text-gray-400 mb-4">
            <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
            </svg>
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Nenhum webhook configurado
          </h3>
          <p className="text-gray-500 mb-4">
            Configure webhooks para receber notificações em tempo real sobre eventos do sistema
          </p>
          <button
            onClick={() => setShowCreateModal(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg"
          >
            Criar Primeiro Webhook
          </button>
        </div>
      )}

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Novo Webhook</h3>
            <form onSubmit={handleCreateWebhook} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  URL do Webhook *
                </label>
                <input
                  type="url"
                  required
                  value={newWebhook.url}
                  onChange={(e) => setNewWebhook({...newWebhook, url: e.target.value})}
                  placeholder="https://api.exemplo.com/webhooks"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Secret (opcional)
                </label>
                <input
                  type="text"
                  value={newWebhook.secret}
                  onChange={(e) => setNewWebhook({...newWebhook, secret: e.target.value})}
                  placeholder="Deixe vazio para gerar automaticamente"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-3">
                  Eventos *
                </label>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-2 max-h-60 overflow-y-auto border border-gray-200 rounded-lg p-4">
                  {availableEvents.map((event) => (
                    <label key={event.value} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        checked={newWebhook.events.includes(event.value)}
                        onChange={() => handleEventToggle(event.value)}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                      <span className="text-sm text-gray-700">{event.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex justify-end space-x-3 pt-4">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={newWebhook.events.length === 0}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                >
                  Criar Webhook
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Logs Modal */}
      {showLogsModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-6xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Logs do Webhook</h3>
              <button
                onClick={() => setShowLogsModal(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Data/Hora</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Evento</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Código</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tempo</th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tentativas</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {webhookLogs.map((log) => (
                    <tr key={log.id} className="hover:bg-gray-50">
                      <td className="px-4 py-4 text-sm text-gray-900">{formatDate(log.createdAt)}</td>
                      <td className="px-4 py-4 text-sm text-gray-900">{getEventLabel(log.event)}</td>
                      <td className="px-4 py-4">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getLogStatusColor(log.status)}`}>
                          {log.status}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-sm text-gray-900">{log.responseCode || 'N/A'}</td>
                      <td className="px-4 py-4 text-sm text-gray-900">{log.responseTime}ms</td>
                      <td className="px-4 py-4 text-sm text-gray-900">{log.retryCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default WebhooksPage;
