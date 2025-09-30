'use client';

import React, { useEffect, useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { configService, userService, bankAccountService, PriorityConfig, User, BankAccount } from '@/services/api';
import toast from 'react-hot-toast';

const PriorizacaoPage: React.FC = () => {
  const { user } = useAuth();
  const [clientes, setClientes] = useState<User[]>([]);
  const [selectedClient, setSelectedClient] = useState('');
  const [contas, setContas] = useState<BankAccount[]>([]);
  const [priorityConfig, setPriorityConfig] = useState<PriorityConfig | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const bancos = [
    { value: 'stark_bank', label: 'Stark Bank' },
    { value: 'sicoob', label: 'Sicoob' },
    { value: 'banco_genial', label: 'Banco Genial' },
    { value: 'efi', label: 'Efí (Gerencianet)' },
    { value: 'celcoin', label: 'Celcoin' }
  ];

  useEffect(() => {
    loadClientes();
  }, []);

  useEffect(() => {
    if (selectedClient) {
      loadClientData(selectedClient);
    } else {
      setContas([]);
      setPriorityConfig(null);
    }
  }, [selectedClient]);

  const loadClientes = async () => {
    try {
      console.log('🔍 [Priorização] Carregando clientes...');
      const response = await userService.getUsers();
      console.log('📦 [Priorização] Resposta clientes:', response);

      const users = response?.data?.users || response?.data || [];
      console.log('👥 [Priorização] Users processados:', users);

      if (!Array.isArray(users)) {
        console.warn('⚠️ [Priorização] Users não é um array:', typeof users, users);
        setClientes([]);
      } else {
        setClientes(users);
      }
    } catch (error) {
      console.error('❌ [Priorização] Erro ao carregar clientes:', error);
      setClientes([]);
      toast.error('Erro ao carregar clientes');
    }
  };

  const loadClientData = async (clienteId: string) => {
    try {
      setIsLoading(true);
      
      // Carregar contas do cliente
      console.log('🔍 [Priorização] Carregando contas do cliente:', clienteId);
      const contasResponse = await bankAccountService.getAccountsByClient(clienteId);
      console.log('📦 [Priorização] Resposta contas:', contasResponse);

      const contas = contasResponse?.data?.data || contasResponse?.data || [];
      console.log('🏦 [Priorização] Contas processadas:', contas);

      if (!Array.isArray(contas)) {
        console.warn('⚠️ [Priorização] Contas não é um array:', typeof contas, contas);
        setContas([]);
      } else {
        setContas(contas);
      }

      // Carregar configuração de priorização
      try {
        const configResponse = await configService.getPriorityConfig(clienteId);
        setPriorityConfig(configResponse.data);
      } catch (error) {
        // Se não existe configuração, criar uma nova
        const contasArray = Array.isArray(contas) ? contas : [];
        const newConfig: PriorityConfig = {
          clienteId,
          prioridades: contasArray.map(conta => ({
            contaId: conta.id,
            banco: conta.banco,
            percentual: 0,
            ativo: true
          })),
          totalPercentual: 0,
          isValid: false,
          updatedAt: new Date().toISOString()
        };
        setPriorityConfig(newConfig);
      }
    } catch (error) {
      console.error('Erro ao carregar dados do cliente:', error);
      toast.error('Erro ao carregar dados do cliente');
    } finally {
      setIsLoading(false);
    }
  };

  const handlePercentualChange = (contaId: string, percentual: number) => {
    if (!priorityConfig) return;

    const newPrioridades = priorityConfig.prioridades.map(p => 
      p.contaId === contaId ? { ...p, percentual } : p
    );

    const totalPercentual = newPrioridades.reduce((sum, p) => sum + p.percentual, 0);
    const isValid = totalPercentual === 100;

    setPriorityConfig({
      ...priorityConfig,
      prioridades: newPrioridades,
      totalPercentual,
      isValid
    });
  };

  const handleAtivoChange = (contaId: string, ativo: boolean) => {
    if (!priorityConfig) return;

    const newPrioridades = priorityConfig.prioridades.map(p => 
      p.contaId === contaId ? { ...p, ativo, percentual: ativo ? p.percentual : 0 } : p
    );

    const totalPercentual = newPrioridades.reduce((sum, p) => sum + p.percentual, 0);
    const isValid = totalPercentual === 100;

    setPriorityConfig({
      ...priorityConfig,
      prioridades: newPrioridades,
      totalPercentual,
      isValid
    });
  };

  const handleSave = async () => {
    if (!priorityConfig || !selectedClient) return;

    if (!priorityConfig.isValid) {
      toast.error('A soma dos percentuais deve ser exatamente 100%');
      return;
    }

    try {
      setIsSaving(true);
      await configService.updatePriorityConfig(selectedClient, {
        prioridades: priorityConfig.prioridades
      });
      toast.success('Configuração salva com sucesso!');
      loadClientData(selectedClient);
    } catch (error) {
      console.error('Erro ao salvar configuração:', error);
      toast.error('Erro ao salvar configuração');
    } finally {
      setIsSaving(false);
    }
  };

  const handleDistributeEqually = () => {
    if (!priorityConfig) return;

    const contasAtivas = priorityConfig.prioridades.filter(p => p.ativo);
    if (contasAtivas.length === 0) {
      toast.error('Pelo menos uma conta deve estar ativa');
      return;
    }

    const percentualPorConta = Math.floor(100 / contasAtivas.length);
    const resto = 100 - (percentualPorConta * contasAtivas.length);

    const newPrioridades = priorityConfig.prioridades.map((p, index) => {
      if (!p.ativo) return { ...p, percentual: 0 };
      
      const isFirstActive = contasAtivas.findIndex(ca => ca.contaId === p.contaId) === 0;
      return {
        ...p,
        percentual: percentualPorConta + (isFirstActive ? resto : 0)
      };
    });

    setPriorityConfig({
      ...priorityConfig,
      prioridades: newPrioridades,
      totalPercentual: 100,
      isValid: true
    });
  };

  const getBancoLabel = (banco: string) => {
    const bancoInfo = bancos.find(b => b.value === banco);
    return bancoInfo?.label || banco;
  };

  const getContaInfo = (contaId: string) => {
    const conta = contas.find(c => c.id === contaId);
    return conta ? `${conta.agencia}/${conta.conta}` : 'N/A';
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Configuração de Priorização</h1>
          <p className="text-gray-600 mt-1">Configure o roteamento ponderado por percentuais</p>
        </div>
      </div>

      {/* Client Selection */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Selecionar Cliente
            </label>
            <select
              value={selectedClient}
              onChange={(e) => setSelectedClient(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Selecione um cliente</option>
              {clientes.map((cliente) => (
                <option key={cliente.id} value={cliente.id}>
                  {cliente.name || cliente.email}
                </option>
              ))}
            </select>
          </div>
          {selectedClient && (
            <div className="flex items-end">
              <button
                onClick={() => loadClientData(selectedClient)}
                className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg"
              >
                Recarregar
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Priority Configuration */}
      {selectedClient && priorityConfig && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
            <h3 className="text-lg font-semibold text-gray-900">
              Configuração de Roteamento
            </h3>
            <div className="flex items-center space-x-4">
              <div className={`text-sm font-medium ${priorityConfig.isValid ? 'text-green-600' : 'text-red-600'}`}>
                Total: {priorityConfig.totalPercentual}%
                {priorityConfig.isValid ? ' ✓' : ' (deve ser 100%)'}
              </div>
              <button
                onClick={handleDistributeEqually}
                className="bg-blue-100 hover:bg-blue-200 text-blue-700 px-3 py-1 rounded text-sm"
              >
                Distribuir Igualmente
              </button>
            </div>
          </div>

          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            </div>
          ) : (
            <div className="p-6">
              <div className="space-y-4">
                {priorityConfig.prioridades.map((prioridade) => (
                  <div key={prioridade.contaId} className="flex items-center space-x-4 p-4 border border-gray-200 rounded-lg">
                    <div className="flex items-center">
                      <input
                        type="checkbox"
                        checked={prioridade.ativo}
                        onChange={(e) => handleAtivoChange(prioridade.contaId, e.target.checked)}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                    </div>
                    
                    <div className="flex-1 grid grid-cols-1 md:grid-cols-3 gap-4">
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {getBancoLabel(prioridade.banco)}
                        </div>
                        <div className="text-sm text-gray-500">
                          {getContaInfo(prioridade.contaId)}
                        </div>
                      </div>
                      
                      <div className="flex items-center space-x-2">
                        <input
                          type="number"
                          min="0"
                          max="100"
                          value={prioridade.percentual}
                          onChange={(e) => handlePercentualChange(prioridade.contaId, parseInt(e.target.value) || 0)}
                          disabled={!prioridade.ativo}
                          className="w-20 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
                        />
                        <span className="text-sm text-gray-500">%</span>
                      </div>
                      
                      <div className="flex items-center">
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div
                            className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                            style={{ width: `${prioridade.percentual}%` }}
                          ></div>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              <div className="mt-6 flex justify-end space-x-3">
                <button
                  onClick={() => loadClientData(selectedClient)}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleSave}
                  disabled={!priorityConfig.isValid || isSaving}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSaving ? 'Salvando...' : 'Salvar Configuração'}
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Empty State */}
      {!selectedClient && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="text-gray-400 mb-4">
            <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Selecione um Cliente
          </h3>
          <p className="text-gray-500">
            Escolha um cliente para configurar a priorização de suas contas bancárias
          </p>
        </div>
      )}

      {/* No Accounts State */}
      {selectedClient && contas.length === 0 && !isLoading && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
          <div className="text-gray-400 mb-4">
            <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M12 9v3m0 0v3m0-3h3m-3 0H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Nenhuma Conta Encontrada
          </h3>
          <p className="text-gray-500 mb-4">
            Este cliente não possui contas bancárias cadastradas
          </p>
          <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg">
            Cadastrar Conta
          </button>
        </div>
      )}
    </div>
  );
};

export default PriorizacaoPage;
