'use client';

import React, { useState, useEffect } from 'react';
import { useRequireAuth } from '@/context/AuthContext';
import { prioritizationService, bankAccountService, PrioritizationRule, PrioritizationConfiguration } from '@/services/api';
import PrioritizationForm from '@/components/PrioritizationForm';
import PrioritizationList from '@/components/PrioritizationList';
import toast from 'react-hot-toast';

const PriorizacaoPage: React.FC = () => {
  useRequireAuth('view_tela_priorizacao');

  const [configuration, setConfiguration] = useState<PrioritizationConfiguration | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    loadConfiguration();
  }, []);

  const loadConfiguration = async () => {
    try {
      setIsLoading(true);

      // Tentar carregar configuração existente
      const configResponse = await prioritizationService.getMyConfiguration();
      const config = configResponse.data;

      // Carregar dados das contas para enriquecer as regras
      const accountsResponse = await bankAccountService.getMyAccounts();
      const accounts = accountsResponse.data;

      // Enriquecer regras com dados das contas
      const enrichedRules = config.rules.map(rule => {
        const account = accounts.find(acc => acc.contaId === rule.contaId);
        return {
          ...rule,
          accountName: account?.description,
          accountNumber: account?.accountNumber,
          bankCode: account?.bankCode,
        };
      });

      setConfiguration({
        ...config,
        rules: enrichedRules,
      });

    } catch (error) {
      console.error('Erro ao carregar configuração:', error);

      // Se não há configuração, começar com estado vazio
      if (error.response?.status === 404) {
        setConfiguration(null);
      } else {
        toast.error('Erro ao carregar configuração de priorização');

        // Usar dados mock para demonstração
        const mockConfig: PrioritizationConfiguration = {
          configurationId: 'config-1',
          clienteId: 'client-1',
          rules: [
            {
              contaId: '1',
              percentage: 60,
              isActive: true,
              accountName: 'Conta Principal Itaú',
              accountNumber: '12345-6',
              bankCode: '341',
            },
            {
              contaId: '2',
              percentage: 30,
              isActive: true,
              accountName: 'Conta Secundária Banco do Brasil',
              accountNumber: '98765-4',
              bankCode: '001',
            },
            {
              contaId: '3',
              percentage: 10,
              isActive: true,
              accountName: 'Conta Backup Santander',
              accountNumber: '55555-1',
              bankCode: '033',
            },
          ],
          isActive: true,
          totalPercentage: 100,
          isValid: true,
          createdAt: '2024-01-15T10:00:00Z',
          updatedAt: '2024-01-20T14:30:00Z',
        };
        setConfiguration(mockConfig);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleSaveConfiguration = async (rules: PrioritizationRule[]) => {
    try {
      setIsSaving(true);

      const requestData = { rules };

      let response;
      if (configuration) {
        // Atualizar configuração existente
        response = await prioritizationService.updateConfiguration(requestData);
        toast.success('Configuração de priorização atualizada com sucesso!');
      } else {
        // Criar nova configuração
        response = await prioritizationService.createConfiguration(requestData);
        toast.success('Configuração de priorização criada com sucesso!');
      }

      // Recarregar configuração
      await loadConfiguration();
      setIsEditing(false);

    } catch (error) {
      console.error('Erro ao salvar configuração:', error);
      toast.error('Erro ao salvar configuração de priorização');

      // Para demonstração, simular sucesso
      const mockConfig: PrioritizationConfiguration = {
        configurationId: configuration?.configurationId || 'config-new',
        clienteId: 'client-1',
        rules,
        isActive: true,
        totalPercentage: rules.filter(r => r.isActive).reduce((sum, r) => sum + r.percentage, 0),
        isValid: rules.filter(r => r.isActive).reduce((sum, r) => sum + r.percentage, 0) === 100,
        createdAt: configuration?.createdAt || new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };

      setConfiguration(mockConfig);
      setIsEditing(false);
      toast.success('Configuração salva com sucesso! (modo demonstração)');
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteRule = async (contaId: string) => {
    if (!configuration) return;

    const updatedRules = configuration.rules.filter(rule => rule.contaId !== contaId);

    if (updatedRules.length === 0) {
      // Se não há mais regras, deletar toda a configuração
      try {
        await prioritizationService.deleteConfiguration();
        setConfiguration(null);
        toast.success('Configuração de priorização removida');
      } catch (error) {
        console.error('Erro ao deletar configuração:', error);
        toast.error('Erro ao remover configuração');
      }
    } else {
      // Atualizar configuração sem a regra removida
      await handleSaveConfiguration(updatedRules);
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
          <div className="animate-pulse space-y-4">
            <div className="h-6 bg-gray-200 rounded w-1/3"></div>
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center space-x-4">
                  <div className="h-12 bg-gray-200 rounded w-1/2"></div>
                  <div className="h-12 bg-gray-200 rounded w-20"></div>
                  <div className="h-6 w-6 bg-gray-200 rounded"></div>
                </div>
              ))}
            </div>
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
          <h1 className="text-3xl font-bold text-gray-900">Priorização</h1>
          <p className="text-gray-600 mt-1">Configure a priorização de contas para transações automáticas</p>
        </div>
        {!isEditing && (
          <button
            onClick={handleStartEditing}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
          >
            {configuration ? 'Editar Configuração' : 'Nova Configuração'}
          </button>
        )}
      </div>

      {/* Conteúdo Principal */}
      {isEditing ? (
        <PrioritizationForm
          onSave={handleSaveConfiguration}
          onCancel={handleCancelEditing}
          initialRules={configuration?.rules || []}
          isLoading={isSaving}
        />
      ) : (
        <PrioritizationList
          rules={configuration?.rules || []}
          onEdit={handleStartEditing}
          onDelete={handleDeleteRule}
          isLoading={isLoading}
        />
      )}

      {/* Informações Adicionais */}
      {!isEditing && configuration && (
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-6">
          <div className="flex items-start space-x-3">
            <div className="p-2 bg-blue-100 rounded-lg">
              <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="flex-1">
              <h3 className="text-sm font-medium text-blue-900 mb-1">Como funciona a priorização</h3>
              <div className="text-sm text-blue-800 space-y-1">
                <p>• As transações serão distribuídas automaticamente entre suas contas conforme os percentuais configurados</p>
                <p>• A soma dos percentuais das contas ativas deve ser exatamente 100%</p>
                <p>• Contas inativas não receberão transações automáticas</p>
                <p>• Você pode ajustar os percentuais a qualquer momento</p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PriorizacaoPage;
