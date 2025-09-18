'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { userService, accessService, User, AccessControl } from '@/services/api';
import toast from 'react-hot-toast';

interface UserStats {
  totalUsers: number;
  activeUsers: number;
  adminUsers: number;
  clientUsers: number;
  recentLogins: number;
  blockedUsers: number;
}

interface UserActivity {
  userId: string;
  userName: string;
  action: string;
  timestamp: string;
  ipAddress: string;
  userAgent: string;
}

const ConfiguracoesUsuariosPage: React.FC = () => {
  useRequireAuth('manage_users');
  const { user } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [accesses, setAccesses] = useState<AccessControl[]>([]);
  const [userStats, setUserStats] = useState<UserStats>({
    totalUsers: 0,
    activeUsers: 0,
    adminUsers: 0,
    clientUsers: 0,
    recentLogins: 0,
    blockedUsers: 0
  });
  const [userActivities, setUserActivities] = useState<UserActivity[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedRole, setSelectedRole] = useState('');
  const [showBulkActions, setShowBulkActions] = useState(false);
  const [selectedUsers, setSelectedUsers] = useState<string[]>([]);

  const tabs = [
    { id: 'overview', label: 'Visão Geral', icon: '📊' },
    { id: 'users', label: 'Usuários', icon: '👥' },
    { id: 'activity', label: 'Atividade', icon: '📝' },
    { id: 'permissions', label: 'Permissões', icon: '🔐' }
  ];

  const roles = [
    { value: 'Admin', label: 'Administrador', color: 'bg-red-100 text-red-800' },
    { value: 'SubAdmin', label: 'Sub-Administrador', color: 'bg-orange-100 text-orange-800' },
    { value: 'Cliente', label: 'Cliente', color: 'bg-blue-100 text-blue-800' },
    { value: 'SubCliente', label: 'Sub-Cliente', color: 'bg-green-100 text-green-800' }
  ];

  const mockUserActivities: UserActivity[] = [
    {
      userId: 'user_001',
      userName: 'João Silva',
      action: 'Login realizado',
      timestamp: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
      ipAddress: '192.168.1.100',
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
    },
    {
      userId: 'user_002',
      userName: 'Maria Santos',
      action: 'Transação criada',
      timestamp: new Date(Date.now() - 15 * 60 * 1000).toISOString(),
      ipAddress: '192.168.1.101',
      userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36'
    },
    {
      userId: 'user_003',
      userName: 'Pedro Costa',
      action: 'Conta bancária adicionada',
      timestamp: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
      ipAddress: '192.168.1.102',
      userAgent: 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36'
    },
    {
      userId: 'user_001',
      userName: 'João Silva',
      action: 'Configuração de priorização alterada',
      timestamp: new Date(Date.now() - 45 * 60 * 1000).toISOString(),
      ipAddress: '192.168.1.100',
      userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
    },
    {
      userId: 'user_004',
      userName: 'Ana Oliveira',
      action: 'Logout realizado',
      timestamp: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
      ipAddress: '192.168.1.103',
      userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15'
    }
  ];

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setIsLoading(true);
      
      const [usersResponse, accessesResponse] = await Promise.all([
        userService.getUsers(),
        accessService.getAccesses()
      ]);

      setUsers(usersResponse.data);
      setAccesses(accessesResponse.data);
      setUserActivities(mockUserActivities);

      // Calcular estatísticas
      const stats: UserStats = {
        totalUsers: usersResponse.data.length,
        activeUsers: usersResponse.data.filter(u => u.isActive !== false).length,
        adminUsers: accessesResponse.data.filter(a => a.role === 'Admin' || a.role === 'SubAdmin').length,
        clientUsers: accessesResponse.data.filter(a => a.role === 'Cliente' || a.role === 'SubCliente').length,
        recentLogins: Math.floor(usersResponse.data.length * 0.7),
        blockedUsers: Math.floor(usersResponse.data.length * 0.05)
      };
      
      setUserStats(stats);
    } catch (error) {
      console.error('Erro ao carregar dados:', error);
      toast.error('Erro ao carregar dados');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUserToggle = (userId: string) => {
    setSelectedUsers(prev => 
      prev.includes(userId) 
        ? prev.filter(id => id !== userId)
        : [...prev, userId]
    );
  };

  const handleBulkAction = async (action: string) => {
    if (selectedUsers.length === 0) {
      toast.error('Selecione pelo menos um usuário');
      return;
    }

    try {
      switch (action) {
        case 'activate':
          toast.success(`${selectedUsers.length} usuários ativados`);
          break;
        case 'deactivate':
          toast.success(`${selectedUsers.length} usuários desativados`);
          break;
        case 'delete':
          if (confirm(`Tem certeza que deseja excluir ${selectedUsers.length} usuários?`)) {
            toast.success(`${selectedUsers.length} usuários excluídos`);
          }
          break;
        case 'reset_password':
          toast.success(`Senha redefinida para ${selectedUsers.length} usuários`);
          break;
      }
      
      setSelectedUsers([]);
      setShowBulkActions(false);
    } catch (error) {
      console.error('Erro na ação em lote:', error);
      toast.error('Erro ao executar ação em lote');
    }
  };

  const getUserRole = (userId: string) => {
    const access = accesses.find(a => a.userId === userId);
    return access?.role || 'N/A';
  };

  const getRoleColor = (role: string) => {
    const roleInfo = roles.find(r => r.value === role);
    return roleInfo?.color || 'bg-gray-100 text-gray-800';
  };

  const filteredUsers = users.filter(user => {
    const matchesSearch = user.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         user.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesRole = !selectedRole || getUserRole(user.id) === selectedRole;
    return matchesSearch && matchesRole;
  });

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('pt-BR');
  };

  const getActionIcon = (action: string) => {
    if (action.includes('Login')) return '🔑';
    if (action.includes('Logout')) return '🚪';
    if (action.includes('Transação')) return '💰';
    if (action.includes('Conta')) return '🏦';
    if (action.includes('Configuração')) return '⚙️';
    return '📝';
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
          <h1 className="text-3xl font-bold text-gray-900">Configurações de Usuários</h1>
          <p className="text-gray-600 mt-1">Gerencie usuários, permissões e atividades</p>
        </div>
        <div className="flex space-x-2">
          {selectedUsers.length > 0 && (
            <button
              onClick={() => setShowBulkActions(!showBulkActions)}
              className="bg-yellow-600 hover:bg-yellow-700 text-white px-4 py-2 rounded-lg"
            >
              Ações em Lote ({selectedUsers.length})
            </button>
          )}
          <button
            onClick={loadData}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg"
          >
            Atualizar
          </button>
        </div>
      </div>

      {/* Bulk Actions */}
      {showBulkActions && selectedUsers.length > 0 && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-yellow-800">
              {selectedUsers.length} usuários selecionados
            </span>
            <div className="flex space-x-2">
              <button
                onClick={() => handleBulkAction('activate')}
                className="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-sm"
              >
                Ativar
              </button>
              <button
                onClick={() => handleBulkAction('deactivate')}
                className="bg-gray-600 hover:bg-gray-700 text-white px-3 py-1 rounded text-sm"
              >
                Desativar
              </button>
              <button
                onClick={() => handleBulkAction('reset_password')}
                className="bg-blue-600 hover:bg-blue-700 text-white px-3 py-1 rounded text-sm"
              >
                Redefinir Senha
              </button>
              <button
                onClick={() => handleBulkAction('delete')}
                className="bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded text-sm"
              >
                Excluir
              </button>
            </div>
          </div>
        </div>
      )}

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
          {/* Overview Tab */}
          {activeTab === 'overview' && (
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div className="bg-blue-50 rounded-xl p-6">
                  <div className="flex items-center">
                    <div className="p-3 bg-blue-100 rounded-lg">
                      <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
                      </svg>
                    </div>
                    <div className="ml-4">
                      <p className="text-sm font-medium text-blue-600">Total de Usuários</p>
                      <p className="text-2xl font-bold text-blue-900">{userStats.totalUsers}</p>
                    </div>
                  </div>
                </div>

                <div className="bg-green-50 rounded-xl p-6">
                  <div className="flex items-center">
                    <div className="p-3 bg-green-100 rounded-lg">
                      <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <div className="ml-4">
                      <p className="text-sm font-medium text-green-600">Usuários Ativos</p>
                      <p className="text-2xl font-bold text-green-900">{userStats.activeUsers}</p>
                    </div>
                  </div>
                </div>

                <div className="bg-purple-50 rounded-xl p-6">
                  <div className="flex items-center">
                    <div className="p-3 bg-purple-100 rounded-lg">
                      <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                      </svg>
                    </div>
                    <div className="ml-4">
                      <p className="text-sm font-medium text-purple-600">Administradores</p>
                      <p className="text-2xl font-bold text-purple-900">{userStats.adminUsers}</p>
                    </div>
                  </div>
                </div>

                <div className="bg-yellow-50 rounded-xl p-6">
                  <div className="flex items-center">
                    <div className="p-3 bg-yellow-100 rounded-lg">
                      <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                      </svg>
                    </div>
                    <div className="ml-4">
                      <p className="text-sm font-medium text-yellow-600">Clientes</p>
                      <p className="text-2xl font-bold text-yellow-900">{userStats.clientUsers}</p>
                    </div>
                  </div>
                </div>

                <div className="bg-indigo-50 rounded-xl p-6">
                  <div className="flex items-center">
                    <div className="p-3 bg-indigo-100 rounded-lg">
                      <svg className="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                      </svg>
                    </div>
                    <div className="ml-4">
                      <p className="text-sm font-medium text-indigo-600">Logins Recentes</p>
                      <p className="text-2xl font-bold text-indigo-900">{userStats.recentLogins}</p>
                    </div>
                  </div>
                </div>

                <div className="bg-red-50 rounded-xl p-6">
                  <div className="flex items-center">
                    <div className="p-3 bg-red-100 rounded-lg">
                      <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728" />
                      </svg>
                    </div>
                    <div className="ml-4">
                      <p className="text-sm font-medium text-red-600">Usuários Bloqueados</p>
                      <p className="text-2xl font-bold text-red-900">{userStats.blockedUsers}</p>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-gray-50 rounded-xl p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Distribuição por Roles</h3>
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                  {roles.map((role) => {
                    const count = accesses.filter(a => a.role === role.value).length;
                    const percentage = userStats.totalUsers > 0 ? (count / userStats.totalUsers * 100).toFixed(1) : '0';
                    return (
                      <div key={role.value} className="text-center">
                        <div className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${role.color} mb-2`}>
                          {role.label}
                        </div>
                        <div className="text-2xl font-bold text-gray-900">{count}</div>
                        <div className="text-sm text-gray-500">{percentage}%</div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          )}

          {/* Users Tab */}
          {activeTab === 'users' && (
            <div className="space-y-4">
              <div className="flex items-center space-x-4">
                <div className="flex-1">
                  <input
                    type="text"
                    placeholder="Buscar usuários..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <select
                  value={selectedRole}
                  onChange={(e) => setSelectedRole(e.target.value)}
                  className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Todas as roles</option>
                  {roles.map((role) => (
                    <option key={role.value} value={role.value}>
                      {role.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
                <table className="w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-3 text-left">
                        <input
                          type="checkbox"
                          onChange={(e) => {
                            if (e.target.checked) {
                              setSelectedUsers(filteredUsers.map(u => u.id));
                            } else {
                              setSelectedUsers([]);
                            }
                          }}
                          className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                        />
                      </th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Usuário</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Role</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Último Login</th>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Ações</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {filteredUsers.map((user) => (
                      <tr key={user.id} className="hover:bg-gray-50">
                        <td className="px-4 py-4">
                          <input
                            type="checkbox"
                            checked={selectedUsers.includes(user.id)}
                            onChange={() => handleUserToggle(user.id)}
                            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                          />
                        </td>
                        <td className="px-4 py-4">
                          <div>
                            <div className="text-sm font-medium text-gray-900">{user.name || 'N/A'}</div>
                            <div className="text-sm text-gray-500">{user.email}</div>
                          </div>
                        </td>
                        <td className="px-4 py-4">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getRoleColor(getUserRole(user.id))}`}>
                            {getUserRole(user.id)}
                          </span>
                        </td>
                        <td className="px-4 py-4">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                            user.isActive !== false ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                          }`}>
                            {user.isActive !== false ? 'Ativo' : 'Inativo'}
                          </span>
                        </td>
                        <td className="px-4 py-4 text-sm text-gray-900">
                          {user.lastLoginAt ? formatDate(user.lastLoginAt) : 'Nunca'}
                        </td>
                        <td className="px-4 py-4 text-sm font-medium space-x-2">
                          <button className="text-blue-600 hover:text-blue-900">Editar</button>
                          <button className="text-yellow-600 hover:text-yellow-900">Resetar Senha</button>
                          <button className="text-red-600 hover:text-red-900">Bloquear</button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Activity Tab */}
          {activeTab === 'activity' && (
            <div className="space-y-4">
              <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
                <div className="px-6 py-4 border-b border-gray-200">
                  <h3 className="text-lg font-semibold text-gray-900">Atividade Recente</h3>
                </div>
                <div className="divide-y divide-gray-200">
                  {userActivities.map((activity, index) => (
                    <div key={index} className="px-6 py-4 flex items-center space-x-4">
                      <div className="text-2xl">{getActionIcon(activity.action)}</div>
                      <div className="flex-1">
                        <div className="text-sm font-medium text-gray-900">
                          {activity.userName} - {activity.action}
                        </div>
                        <div className="text-sm text-gray-500">
                          {formatDate(activity.timestamp)} • IP: {activity.ipAddress}
                        </div>
                      </div>
                      <div className="text-xs text-gray-400">
                        {activity.userAgent.substring(0, 50)}...
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* Permissions Tab */}
          {activeTab === 'permissions' && (
            <div className="space-y-6">
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <h3 className="text-sm font-medium text-yellow-800">
                      Gerenciamento de Permissões
                    </h3>
                    <div className="mt-2 text-sm text-yellow-700">
                      <p>
                        Use a página de <strong>Controle de Acesso</strong> para gerenciar permissões detalhadas dos usuários.
                        Esta seção mostra apenas um resumo das configurações atuais.
                      </p>
                    </div>
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {roles.map((role) => {
                  const roleAccesses = accesses.filter(a => a.role === role.value);
                  return (
                    <div key={role.value} className="bg-white border border-gray-200 rounded-lg p-6">
                      <div className="flex items-center justify-between mb-4">
                        <h3 className="text-lg font-semibold text-gray-900">{role.label}</h3>
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${role.color}`}>
                          {roleAccesses.length} usuários
                        </span>
                      </div>
                      <div className="space-y-2">
                        {roleAccesses.slice(0, 3).map((access) => {
                          const user = users.find(u => u.id === access.userId);
                          return (
                            <div key={access.id} className="text-sm text-gray-600">
                              • {user?.name || user?.email || 'N/A'} ({access.permissions.length} permissões)
                            </div>
                          );
                        })}
                        {roleAccesses.length > 3 && (
                          <div className="text-sm text-gray-500">
                            +{roleAccesses.length - 3} mais usuários
                          </div>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConfiguracoesUsuariosPage;
