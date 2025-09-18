'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { bankAccountService, userService, BankAccount, User } from '@/services/api';
import toast from 'react-hot-toast';

const ContasPage: React.FC = () => {
  useRequireAuth('manage_bank_accounts');
  const { user } = useAuth();
  const [contas, setContas] = useState<BankAccount[]>([]);
  const [clientes, setClientes] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedClient, setSelectedClient] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newAccount, setNewAccount] = useState({
    clienteId: '',
    banco: '',
    agencia: '',
    conta: '',
    tipoConta: 'corrente' as 'corrente' | 'poupanca',
    credentials: {
      clientId: '',
      clientSecret: '',
      apiKey: '',
      environment: 'sandbox' as 'sandbox' | 'production'
    }
  });

  const bancos = [
    { value: 'stark_bank', label: 'Stark Bank' },
    { value: 'sicoob', label: 'Sicoob' },
    { value: 'banco_genial', label: 'Banco Genial' },
    { value: 'efi', label: 'Efí (Gerencianet)' },
    { value: 'celcoin', label: 'Celcoin' }
  ];

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (selectedClient) {
      loadContasByClient(selectedClient);
    } else {
      loadContas();
    }
  }, [selectedClient]);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [contasResponse, clientesResponse] = await Promise.all([
        bankAccountService.getAccounts(),
        userService.getUsers()
      ]);
      setContas(contasResponse.data);
      setClientes(clientesResponse.data);
    } catch (error) {
      console.error('Erro ao carregar dados:', error);
      toast.error('Erro ao carregar dados');
    } finally {
      setIsLoading(false);
    }
  };

  const loadContas = async () => {
    try {
      const response = await bankAccountService.getAccounts();
      setContas(response.data);
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      toast.error('Erro ao carregar contas');
    }
  };

  const loadContasByClient = async (clienteId: string) => {
    try {
      const response = await bankAccountService.getAccountsByClient(clienteId);
      setContas(response.data);
    } catch (error) {
      console.error('Erro ao carregar contas do cliente:', error);
      toast.error('Erro ao carregar contas do cliente');
    }
  };

  const handleCreateAccount = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await bankAccountService.createAccount(newAccount);
      toast.success('Conta criada com sucesso!');
      setShowCreateModal(false);
      setNewAccount({
        clienteId: '',
        banco: '',
        agencia: '',
        conta: '',
        tipoConta: 'corrente',
        credentials: {
          clientId: '',
          clientSecret: '',
          apiKey: '',
          environment: 'sandbox'
        }
      });
      if (selectedClient) {
        loadContasByClient(selectedClient);
      } else {
        loadContas();
      }
    } catch (error) {
      console.error('Erro ao criar conta:', error);
      toast.error('Erro ao criar conta');
    }
  };

  const handleDeleteAccount = async (id: string) => {
    if (!confirm('Tem certeza que deseja excluir esta conta?')) return;
    
    try {
      await bankAccountService.deleteAccount(id);
      toast.success('Conta excluída com sucesso!');
      if (selectedClient) {
        loadContasByClient(selectedClient);
      } else {
        loadContas();
      }
    } catch (error) {
      console.error('Erro ao excluir conta:', error);
      toast.error('Erro ao excluir conta');
    }
  };

  const filteredContas = contas.filter(conta =>
    conta.banco.toLowerCase().includes(searchTerm.toLowerCase()) ||
    conta.agencia.toLowerCase().includes(searchTerm.toLowerCase()) ||
    conta.conta.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getClienteName = (clienteId: string) => {
    const cliente = clientes.find(c => c.id === clienteId);
    return cliente?.name || cliente?.email || 'N/A';
  };

  const getBancoLabel = (banco: string) => {
    const bancoInfo = bancos.find(b => b.value === banco);
    return bancoInfo?.label || banco;
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
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Contas Bancárias</h1>
          <p className="text-gray-600 mt-1">Gerencie as contas bancárias dos clientes</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
        >
          Nova Conta
        </button>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Buscar
            </label>
            <input
              type="text"
              placeholder="Banco, agência ou conta..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Filtrar por Cliente
            </label>
            <select
              value={selectedClient}
              onChange={(e) => setSelectedClient(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Todos os clientes</option>
              {clientes.map((cliente) => (
                <option key={cliente.id} value={cliente.id}>
                  {cliente.name || cliente.email}
                </option>
              ))}
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={loadData}
              className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg"
            >
              Atualizar
            </button>
          </div>
        </div>
      </div>

      {/* Contas Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Contas Bancárias ({filteredContas.length})
          </h3>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Cliente
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Banco
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Agência/Conta
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Tipo
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredContas.map((conta) => (
                <tr key={conta.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {getClienteName(conta.clienteId)}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {getBancoLabel(conta.banco)}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {conta.agencia} / {conta.conta}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {conta.tipoConta}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                      Ativa
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                    <button className="text-blue-600 hover:text-blue-900">
                      Editar
                    </button>
                    <button className="text-green-600 hover:text-green-900">
                      Testar
                    </button>
                    <button 
                      onClick={() => handleDeleteAccount(conta.id)}
                      className="text-red-600 hover:text-red-900"
                    >
                      Excluir
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Nova Conta Bancária</h3>
            <form onSubmit={handleCreateAccount} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Cliente
                  </label>
                  <select
                    required
                    value={newAccount.clienteId}
                    onChange={(e) => setNewAccount({...newAccount, clienteId: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Selecione um cliente</option>
                    {clientes.map((cliente) => (
                      <option key={cliente.id} value={cliente.id}>
                        {cliente.name || cliente.email}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Banco
                  </label>
                  <select
                    required
                    value={newAccount.banco}
                    onChange={(e) => setNewAccount({...newAccount, banco: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Selecione um banco</option>
                    {bancos.map((banco) => (
                      <option key={banco.value} value={banco.value}>
                        {banco.label}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Agência
                  </label>
                  <input
                    type="text"
                    required
                    value={newAccount.agencia}
                    onChange={(e) => setNewAccount({...newAccount, agencia: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Conta
                  </label>
                  <input
                    type="text"
                    required
                    value={newAccount.conta}
                    onChange={(e) => setNewAccount({...newAccount, conta: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tipo
                  </label>
                  <select
                    value={newAccount.tipoConta}
                    onChange={(e) => setNewAccount({...newAccount, tipoConta: e.target.value as 'corrente' | 'poupanca'})}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="corrente">Corrente</option>
                    <option value="poupanca">Poupança</option>
                  </select>
                </div>
              </div>

              <div className="border-t pt-4">
                <h4 className="text-md font-medium text-gray-900 mb-3">Credenciais de Integração</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Client ID
                    </label>
                    <input
                      type="text"
                      required
                      value={newAccount.credentials.clientId}
                      onChange={(e) => setNewAccount({
                        ...newAccount, 
                        credentials: {...newAccount.credentials, clientId: e.target.value}
                      })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Client Secret
                    </label>
                    <input
                      type="password"
                      required
                      value={newAccount.credentials.clientSecret}
                      onChange={(e) => setNewAccount({
                        ...newAccount, 
                        credentials: {...newAccount.credentials, clientSecret: e.target.value}
                      })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      API Key (opcional)
                    </label>
                    <input
                      type="text"
                      value={newAccount.credentials.apiKey}
                      onChange={(e) => setNewAccount({
                        ...newAccount, 
                        credentials: {...newAccount.credentials, apiKey: e.target.value}
                      })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Ambiente
                    </label>
                    <select
                      value={newAccount.credentials.environment}
                      onChange={(e) => setNewAccount({
                        ...newAccount, 
                        credentials: {...newAccount.credentials, environment: e.target.value as 'sandbox' | 'production'}
                      })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="sandbox">Sandbox</option>
                      <option value="production">Produção</option>
                    </select>
                  </div>
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
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Criar Conta
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ContasPage;
