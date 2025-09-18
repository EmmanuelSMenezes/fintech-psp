'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { accessService, userService, AccessControl, User } from '@/services/api';
import toast from 'react-hot-toast';

const AcessosPage: React.FC = () => {
  useRequireAuth('manage_access_control');
  const { user } = useAuth();
  const [acessos, setAcessos] = useState<AccessControl[]>([]);
  const [usuarios, setUsuarios] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedUser, setSelectedUser] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newAccess, setNewAccess] = useState({
    userId: '',
    parentUserId: '',
    role: 'SubAdmin' as 'Admin' | 'SubAdmin' | 'Cliente' | 'SubCliente',
    permissions: [] as string[]
  });

  const roles = [
    { value: 'Admin', label: 'Administrador' },
    { value: 'SubAdmin', label: 'Sub-Administrador' },
    { value: 'Cliente', label: 'Cliente' },
    { value: 'SubCliente', label: 'Sub-Cliente' }
  ];

  const availablePermissions = [
    { value: 'view_dashboard', label: 'Ver Dashboard' },
    { value: 'manage_users', label: 'Gerenciar Usuários' },
    { value: 'manage_bank_accounts', label: 'Gerenciar Contas Bancárias' },
    { value: 'view_transactions', label: 'Ver Transações' },
    { value: 'create_transactions', label: 'Criar Transações' },
    { value: 'manage_priority_config', label: 'Gerenciar Configuração de Prioridade' },
    { value: 'view_reports', label: 'Ver Relatórios' },
    { value: 'manage_access_control', label: 'Gerenciar Controle de Acesso' },
    { value: 'view_balance', label: 'Ver Saldo' },
    { value: 'manage_webhooks', label: 'Gerenciar Webhooks' },
    { value: 'view_integrations', label: 'Ver Integrações' },
    { value: 'manage_qr_codes', label: 'Gerenciar QR Codes' },
    { value: 'view_crypto_transactions', label: 'Ver Transações Crypto' },
    { value: 'manage_crypto_transactions', label: 'Gerenciar Transações Crypto' },
    { value: 'view_audit_logs', label: 'Ver Logs de Auditoria' }
  ];

  const defaultPermissionsByRole = {
    Admin: availablePermissions.map(p => p.value),
    SubAdmin: [
      'view_dashboard', 'manage_users', 'manage_bank_accounts', 'view_transactions',
      'create_transactions', 'manage_priority_config', 'view_reports', 'view_balance'
    ],
    Cliente: [
      'view_dashboard', 'view_transactions', 'create_transactions', 'view_balance',
      'manage_qr_codes', 'view_crypto_transactions'
    ],
    SubCliente: [
      'view_dashboard', 'view_transactions', 'view_balance'
    ]
  };

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (selectedUser) {
      loadAccessesByUser(selectedUser);
    } else {
      loadAcessos();
    }
  }, [selectedUser]);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [acessosResponse, usuariosResponse] = await Promise.all([
        accessService.getAccesses(),
        userService.getUsers()
      ]);
      setAcessos(acessosResponse.data);
      setUsuarios(usuariosResponse.data);
    } catch (error) {
      console.error('Erro ao carregar dados:', error);
      toast.error('Erro ao carregar dados');
    } finally {
      setIsLoading(false);
    }
  };

  const loadAcessos = async () => {
    try {
      const response = await accessService.getAccesses();
      setAcessos(response.data);
    } catch (error) {
      console.error('Erro ao carregar acessos:', error);
      toast.error('Erro ao carregar acessos');
    }
  };

  const loadAccessesByUser = async (userId: string) => {
    try {
      const response = await accessService.getAccesses(userId);
      setAcessos(response.data);
    } catch (error) {
      console.error('Erro ao carregar acessos do usuário:', error);
      toast.error('Erro ao carregar acessos do usuário');
    }
  };

  const handleCreateAccess = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await accessService.createAccess(newAccess);
      toast.success('Acesso criado com sucesso!');
      setShowCreateModal(false);
      setNewAccess({
        userId: '',
        parentUserId: '',
        role: 'SubAdmin',
        permissions: []
      });
      if (selectedUser) {
        loadAccessesByUser(selectedUser);
      } else {
        loadAcessos();
      }
    } catch (error) {
      console.error('Erro ao criar acesso:', error);
      toast.error('Erro ao criar acesso');
    }
  };

  const handleDeleteAccess = async (id: string) => {
    if (!confirm('Tem certeza que deseja excluir este acesso?')) return;
    
    try {
      await accessService.deleteAccess(id);
      toast.success('Acesso excluído com sucesso!');
      if (selectedUser) {
        loadAccessesByUser(selectedUser);
      } else {
        loadAcessos();
      }
    } catch (error) {
      console.error('Erro ao excluir acesso:', error);
      toast.error('Erro ao excluir acesso');
    }
  };

  const handleRoleChange = (role: string) => {
    setNewAccess({
      ...newAccess,
      role: role as any,
      permissions: defaultPermissionsByRole[role] || []
    });
  };

  const handlePermissionToggle = (permission: string) => {
    const newPermissions = newAccess.permissions.includes(permission)
      ? newAccess.permissions.filter(p => p !== permission)
      : [...newAccess.permissions, permission];
    
    setNewAccess({
      ...newAccess,
      permissions: newPermissions
    });
  };

  const filteredAcessos = acessos.filter(acesso => {
    const usuario = usuarios.find(u => u.id === acesso.userId);
    const parentUser = acesso.parentUserId ? usuarios.find(u => u.id === acesso.parentUserId) : null;
    
    return (
      usuario?.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      usuario?.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      acesso.role.toLowerCase().includes(searchTerm.toLowerCase()) ||
      parentUser?.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      parentUser?.email.toLowerCase().includes(searchTerm.toLowerCase())
    );
  });

  const getUserName = (userId: string) => {
    const usuario = usuarios.find(u => u.id === userId);
    return usuario?.name || usuario?.email || 'N/A';
  };

  const getRoleLabel = (role: string) => {
    const roleInfo = roles.find(r => r.value === role);
    return roleInfo?.label || role;
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
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Acessos</h1>
          <p className="text-gray-600 mt-1">Controle de acesso baseado em roles (RBAC)</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
        >
          Novo Acesso
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
              placeholder="Nome, email ou role..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Filtrar por Usuário
            </label>
            <select
              value={selectedUser}
              onChange={(e) => setSelectedUser(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Todos os usuários</option>
              {usuarios.map((usuario) => (
                <option key={usuario.id} value={usuario.id}>
                  {usuario.name || usuario.email}
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

      {/* Acessos Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Controles de Acesso ({filteredAcessos.length})
          </h3>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Usuário
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Role
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Usuário Pai
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Permissões
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredAcessos.map((acesso) => (
                <tr key={acesso.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {getUserName(acesso.userId)}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {getRoleLabel(acesso.role)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {acesso.parentUserId ? getUserName(acesso.parentUserId) : 'N/A'}
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-xs text-gray-500">
                      {acesso.permissions.length} permissões
                    </div>
                    <div className="flex flex-wrap gap-1 mt-1">
                      {acesso.permissions.slice(0, 3).map((permission) => (
                        <span key={permission} className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                          {availablePermissions.find(p => p.value === permission)?.label || permission}
                        </span>
                      ))}
                      {acesso.permissions.length > 3 && (
                        <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                          +{acesso.permissions.length - 3} mais
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                    <button className="text-blue-600 hover:text-blue-900">
                      Editar
                    </button>
                    <button 
                      onClick={() => handleDeleteAccess(acesso.id)}
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
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Novo Controle de Acesso</h3>
            <form onSubmit={handleCreateAccess} className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Usuário
                  </label>
                  <select
                    required
                    value={newAccess.userId}
                    onChange={(e) => setNewAccess({...newAccess, userId: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Selecione um usuário</option>
                    {usuarios.map((usuario) => (
                      <option key={usuario.id} value={usuario.id}>
                        {usuario.name || usuario.email}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Role
                  </label>
                  <select
                    required
                    value={newAccess.role}
                    onChange={(e) => handleRoleChange(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  >
                    {roles.map((role) => (
                      <option key={role.value} value={role.value}>
                        {role.label}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Usuário Pai (opcional)
                </label>
                <select
                  value={newAccess.parentUserId}
                  onChange={(e) => setNewAccess({...newAccess, parentUserId: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Nenhum usuário pai</option>
                  {usuarios.map((usuario) => (
                    <option key={usuario.id} value={usuario.id}>
                      {usuario.name || usuario.email}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-3">
                  Permissões
                </label>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-2 max-h-60 overflow-y-auto border border-gray-200 rounded-lg p-4">
                  {availablePermissions.map((permission) => (
                    <label key={permission.value} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        checked={newAccess.permissions.includes(permission.value)}
                        onChange={() => handlePermissionToggle(permission.value)}
                        className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                      />
                      <span className="text-sm text-gray-700">{permission.label}</span>
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
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Criar Acesso
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default AcessosPage;
