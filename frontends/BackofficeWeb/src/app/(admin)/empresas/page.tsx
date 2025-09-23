'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import {
  userService,
  User,
  CreateUserRequest,
  UpdateUserRequest
} from '@/services/api';
import toast from 'react-hot-toast';
import ConfirmModal from '@/components/ConfirmModal';
import LoadingSpinner from '@/components/LoadingSpinner';
import Pagination from '@/components/Pagination';
import CustomModal from '@/components/CustomModal';
import UserForm from '@/components/UserForm';
import { formatDate, formatDocument, getInitials, getColorFromString } from '@/utils/formatters';



// Função para obter cor do status
const getStatusColor = (isActive: boolean) => {
  return isActive
    ? 'bg-green-100 text-green-800'
    : 'bg-gray-100 text-gray-600';
};

// Função para obter texto do status
const getStatusText = (isActive: boolean) => {
  return isActive ? 'Ativo' : 'Inativo';
};

const ClientesPage: React.FC = () => {
  useRequireAuth('manage_users');
  const { user } = useAuth();

  // Estados principais
  const [users, setUsers] = useState<User[]>([]);
  const [totalUsers, setTotalUsers] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Estados de filtros e paginação
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<boolean | ''>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  // Estados de modais
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);

  // Carregar usuários quando componente monta ou filtros mudam
  useEffect(() => {
    loadUsers();
  }, [currentPage, searchTerm, statusFilter]);

  const loadUsers = async () => {
    try {
      setIsLoading(true);
      console.log('🔄 Carregando usuários...', {
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm,
        status: statusFilter
      });

      const response = await userService.getUsers({
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm || undefined,
        isActive: statusFilter !== '' ? statusFilter : undefined
      });

      console.log('📋 Resposta da API de usuários:', response.data);

      if (response.data && response.data.users) {
        setUsers(response.data.users);
        setTotalUsers(response.data.total || 0);
      } else {
        setUsers([]);
        setTotalUsers(0);
      }
    } catch (error: any) {
      console.error('❌ Erro ao carregar usuários:', error);

      if (error.response?.status === 401) {
        toast.error('Sessão expirada. Redirecionando para login...');
      } else if (error.response?.status === 403) {
        toast.error('Você não tem permissão para acessar esta funcionalidade');
      } else if (error.response?.status >= 500) {
        toast.error('Erro interno do servidor. Tente novamente mais tarde.');
      } else {
        toast.error('Erro ao carregar usuários');
      }

      setUsers([]);
      setTotalUsers(0);
    } finally {
      setIsLoading(false);
    }
  };





  // Excluir usuário
  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    try {
      setIsDeleting(true);
      console.log('🗑️ Excluindo usuário:', selectedUser.id);

      await userService.deleteUser(selectedUser.id);

      toast.success('Usuário excluído com sucesso!');
      setShowDeleteModal(false);
      setSelectedUser(null);
      loadUsers();
    } catch (error: any) {
      console.error('❌ Erro ao excluir usuário:', error);

      if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else {
        toast.error('Erro ao excluir usuário');
      }
    } finally {
      setIsDeleting(false);
    }
  };

  // Handlers para UI
  const handleEditClick = (user: User) => {
    console.log('🔍 Dados do usuário selecionado para edição:', user);
    setSelectedUser(user);
    setShowEditModal(true);
  };

  const handleDeleteClick = (user: User) => {
    setSelectedUser(user);
    setShowDeleteModal(true);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1);
  };

  const handleStatusFilterChange = (value: string) => {
    setStatusFilter(value === '' ? '' : value === 'true');
    setCurrentPage(1);
  };

  // Loading state
  if (isLoading && users.length === 0) {
    return <LoadingSpinner fullScreen text="Carregando usuários..." />;
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Empresas</h1>
          <p className="text-gray-600 mt-1">
            Gerencie as empresas clientes do PSP com CNPJ ({totalUsers} total)
          </p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium flex items-center"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          Novo Cliente
        </button>
      </div>

      {/* Filtros */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Busca */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Buscar
            </label>
            <div className="relative">
              <input
                type="text"
                placeholder="Nome, CPF/CNPJ, email..."
                value={searchTerm}
                onChange={(e) => handleSearchChange(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <svg className="absolute left-3 top-2.5 h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>

          {/* Filtro por Status */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Status
            </label>
            <select
              value={statusFilter === '' ? '' : statusFilter.toString()}
              onChange={(e) => handleStatusFilterChange(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Todos os status</option>
              <option value="true">Ativo</option>
              <option value="false">Inativo</option>
            </select>
          </div>

          {/* Botão Atualizar */}
          <div className="flex items-end">
            <button
              onClick={loadUsers}
              disabled={isLoading}
              className="w-full bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg font-medium flex items-center justify-center disabled:opacity-50"
            >
              {isLoading ? (
                <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-gray-700" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              ) : (
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
              )}
              Atualizar
            </button>
          </div>
        </div>
      </div>

      {/* Usuários Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Clientes ({users.length})
          </h3>
        </div>

        {isLoading ? (
          <div className="p-8 text-center">
            <LoadingSpinner size="lg" text="Carregando clientes..." />
          </div>
        ) : users.length === 0 ? (
          <div className="p-8 text-center">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">Nenhum cliente encontrado</h3>
            <p className="mt-1 text-sm text-gray-500">
              {searchTerm || statusFilter !== '' ? 'Tente ajustar os filtros de busca.' : 'Comece criando um novo cliente.'}
            </p>
            {!searchTerm && statusFilter === '' && (
              <div className="mt-6">
                <button
                  onClick={() => setShowCreateModal(true)}
                  className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                >
                  <svg className="-ml-1 mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Novo Cliente
                </button>
              </div>
            )}
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Cliente
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      CPF/CNPJ
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Telefone
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Criado em
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Ações
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {users.map((user) => (
                    <tr key={user.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div
                            className="flex-shrink-0 h-10 w-10 rounded-full flex items-center justify-center text-white text-sm font-medium"
                            style={{ backgroundColor: getColorFromString(user.name) }}
                          >
                            {getInitials(user.name)}
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {user.name}
                            </div>
                            <div className="text-xs text-gray-400">{user.email}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {formatDocument(user.document)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {user.phone}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(user.isActive)}`}>
                          {getStatusText(user.isActive)}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(user.createdAt)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleEditClick(user)}
                            className="text-blue-600 hover:text-blue-900"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => handleDeleteClick(user)}
                            className="text-red-600 hover:text-red-900"
                          >
                            Excluir
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Paginação */}
            <Pagination
              currentPage={currentPage}
              totalPages={Math.ceil(totalUsers / itemsPerPage)}
              totalItems={totalUsers}
              itemsPerPage={itemsPerPage}
              onPageChange={handlePageChange}
            />
          </>
        )}
      </div>

      {/* Create Modal */}
      <CustomModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        title="Novo Cliente"
        subtitle="Cadastre um novo cliente"
        size="lg"
      >
        <UserForm
          onSubmit={async (data) => {
            try {
              setIsCreating(true);

              const createRequest: CreateUserRequest = {
                name: data.name,
                email: data.email,
                document: data.document,
                phone: data.phone,
                isActive: data.isActive
              };

              await userService.createUser(createRequest);
              toast.success('Cliente criado com sucesso!');
              setShowCreateModal(false);
              loadUsers();
            } catch (error: any) {
              console.error('Erro ao criar cliente:', error);
              toast.error(error.response?.data?.message || 'Erro ao criar cliente');
            } finally {
              setIsCreating(false);
            }
          }}
          onCancel={() => setShowCreateModal(false)}
          isLoading={isCreating}
          submitText="Criar Cliente"
        />
      </CustomModal>

      {/* Edit Modal */}
      <CustomModal
        isOpen={showEditModal && !!selectedUser}
        onClose={() => {
          setShowEditModal(false);
          setSelectedUser(null);
        }}
        title="Editar Cliente"
        subtitle="Atualize os dados do cliente"
        size="lg"
      >
        {selectedUser && (
          <UserForm
            isEditing={true}
            initialData={{
              name: selectedUser.name || '',
              email: selectedUser.email || '',
              document: selectedUser.document || '',
              phone: selectedUser.phone || '',
              isActive: selectedUser.isActive
            }}
            onSubmit={async (data) => {
              try {
                setIsUpdating(true);

                const updateRequest: UpdateUserRequest = {
                  name: data.name,
                  email: data.email,
                  document: data.document,
                  phone: data.phone,
                  isActive: data.isActive
                };

                await userService.updateUser(selectedUser.id, updateRequest);
                toast.success('Cliente atualizado com sucesso!');
                setShowEditModal(false);
                setSelectedUser(null);
                loadUsers();
              } catch (error: any) {
                console.error('Erro ao atualizar cliente:', error);
                if (error.response?.status === 400 && error.response?.data?.errors) {
                  const errors = error.response.data.errors;
                  const errorMessages = Object.entries(errors)
                    .map(([field, messages]) => `${field}: ${(messages as string[]).join(', ')}`)
                    .join('\n');
                  toast.error(`Erro de validação:\n${errorMessages}`);
                } else {
                  toast.error(error.response?.data?.message || 'Erro ao atualizar cliente');
                }
              } finally {
                setIsUpdating(false);
              }
            }}
            onCancel={() => {
              setShowEditModal(false);
              setSelectedUser(null);
            }}
            isLoading={isUpdating}
            submitText="Salvar Alterações"
          />
        )}
      </CustomModal>


      {/* Delete Confirmation Modal */}
      <ConfirmModal
        isOpen={showDeleteModal}
        onCancel={() => {
          setShowDeleteModal(false);
          setSelectedUser(null);
        }}
        onConfirm={handleDeleteUser}
        title="Excluir Cliente"
        message={`Tem certeza que deseja excluir o cliente "${selectedUser?.name}"? Esta ação não pode ser desfeita.`}
        confirmText="Excluir"
        cancelText="Cancelar"
        isLoading={isDeleting}
        type="danger"
      />
    </div>
  );
};

export default ClientesPage;
