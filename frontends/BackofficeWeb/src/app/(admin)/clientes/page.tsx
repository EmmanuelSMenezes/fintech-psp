'use client';

import React, { useEffect, useState } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { userService, User, CreateUserRequest, UpdateUserRequest } from '@/services/api';
import toast from 'react-hot-toast';
import ConfirmModal from '@/components/ConfirmModal';
import LoadingSpinner from '@/components/LoadingSpinner';
import Pagination from '@/components/Pagination';

// Esquemas de validação
const createUserSchema = Yup.object().shape({
  email: Yup.string()
    .email('Email inválido')
    .required('Email é obrigatório'),
  name: Yup.string()
    .min(2, 'Nome deve ter pelo menos 2 caracteres')
    .required('Nome é obrigatório'),
  role: Yup.string()
    .oneOf(['cliente', 'admin', 'sub-admin'], 'Role inválido')
    .required('Role é obrigatório'),
  document: Yup.string()
    .matches(/^\d{11}$|^\d{14}$/, 'Documento deve ter 11 (CPF) ou 14 (CNPJ) dígitos'),
  phone: Yup.string()
    .matches(/^\+?[\d\s\-\(\)]+$/, 'Telefone inválido'),
  password: Yup.string()
    .min(6, 'Senha deve ter pelo menos 6 caracteres')
});

const updateUserSchema = Yup.object().shape({
  email: Yup.string()
    .email('Email inválido'),
  name: Yup.string()
    .min(2, 'Nome deve ter pelo menos 2 caracteres'),
  role: Yup.string()
    .oneOf(['cliente', 'admin', 'sub-admin'], 'Role inválido'),
  document: Yup.string()
    .matches(/^\d{11}$|^\d{14}$/, 'Documento deve ter 11 (CPF) ou 14 (CNPJ) dígitos'),
  phone: Yup.string()
    .matches(/^\+?[\d\s\-\(\)]+$/, 'Telefone inválido')
});

const ClientesPage: React.FC = () => {
  useRequireAuth('manage_users');
  const { user } = useAuth();

  // Estados principais
  const [clientes, setClientes] = useState<User[]>([]);
  const [totalClientes, setTotalClientes] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Estados de filtros e paginação
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  // Estados de modais
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedClient, setSelectedClient] = useState<User | null>(null);

  // Carregar clientes quando componente monta ou filtros mudam
  useEffect(() => {
    loadClientes();
  }, [currentPage, searchTerm, roleFilter]);

  const loadClientes = async () => {
    try {
      setIsLoading(true);
      console.log('🔄 Carregando clientes...', {
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm,
        role: roleFilter
      });

      const response = await userService.getUsers({
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm || undefined,
        role: roleFilter || undefined
      });

      console.log('📋 Resposta da API de usuários:', response.data);

      // Verificar se a resposta tem a estrutura esperada
      if (response.data && typeof response.data === 'object') {
        if (Array.isArray(response.data)) {
          // Resposta é um array direto
          setClientes(response.data);
          setTotalClientes(response.data.length);
        } else if (response.data.users && Array.isArray(response.data.users)) {
          // Resposta tem estrutura paginada
          setClientes(response.data.users);
          setTotalClientes(response.data.total || response.data.users.length);
        } else {
          console.warn('Estrutura de resposta inesperada:', response.data);
          setClientes([]);
          setTotalClientes(0);
        }
      } else {
        setClientes([]);
        setTotalClientes(0);
      }
    } catch (error: any) {
      console.error('❌ Erro ao carregar clientes:', error);

      // Tratamento específico de erros
      if (error.response?.status === 401) {
        toast.error('Sessão expirada. Redirecionando para login...');
        // O interceptor já vai redirecionar
      } else if (error.response?.status === 403) {
        toast.error('Você não tem permissão para acessar esta funcionalidade');
      } else if (error.response?.status >= 500) {
        toast.error('Erro interno do servidor. Tente novamente mais tarde.');
      } else {
        toast.error('Erro ao carregar clientes');
      }

      setClientes([]);
      setTotalClientes(0);
    } finally {
      setIsLoading(false);
    }
  };

  // Handlers para CRUD
  const handleCreateClient = async (values: CreateUserRequest) => {
    try {
      setIsCreating(true);
      console.log('🚀 Criando cliente:', values);

      const response = await userService.createUser(values);
      console.log('✅ Cliente criado:', response.data);

      toast.success('Cliente criado com sucesso! Email de boas-vindas enviado.');
      setShowCreateModal(false);
      loadClientes();
    } catch (error: any) {
      console.error('❌ Erro ao criar cliente:', error);

      if (error.response?.status === 400) {
        const errorMsg = error.response.data?.message || 'Dados inválidos';
        toast.error(`Erro de validação: ${errorMsg}`);
      } else if (error.response?.status === 409) {
        toast.error('Email já está em uso por outro usuário');
      } else {
        toast.error('Erro ao criar cliente');
      }
    } finally {
      setIsCreating(false);
    }
  };

  const handleUpdateClient = async (values: UpdateUserRequest) => {
    if (!selectedClient) return;

    try {
      setIsUpdating(true);
      console.log('✏️ Atualizando cliente:', selectedClient.id, values);

      const response = await userService.updateUser(selectedClient.id, values);
      console.log('✅ Cliente atualizado:', response.data);

      toast.success('Cliente atualizado com sucesso!');
      setShowEditModal(false);
      setSelectedClient(null);
      loadClientes();
    } catch (error: any) {
      console.error('❌ Erro ao atualizar cliente:', error);

      if (error.response?.status === 400) {
        const errorMsg = error.response.data?.message || 'Dados inválidos';
        toast.error(`Erro de validação: ${errorMsg}`);
      } else if (error.response?.status === 404) {
        toast.error('Cliente não encontrado');
      } else {
        toast.error('Erro ao atualizar cliente');
      }
    } finally {
      setIsUpdating(false);
    }
  };

  const handleDeleteClient = async () => {
    if (!selectedClient) return;

    try {
      setIsDeleting(true);
      console.log('🗑️ Deletando cliente:', selectedClient.id);

      await userService.deleteUser(selectedClient.id);
      console.log('✅ Cliente deletado');

      toast.success('Cliente excluído com sucesso!');
      setShowDeleteModal(false);
      setSelectedClient(null);
      loadClientes();
    } catch (error: any) {
      console.error('❌ Erro ao excluir cliente:', error);

      if (error.response?.status === 404) {
        toast.error('Cliente não encontrado');
      } else if (error.response?.status === 409) {
        toast.error('Não é possível excluir cliente com transações ativas');
      } else {
        toast.error('Erro ao excluir cliente');
      }
    } finally {
      setIsDeleting(false);
    }
  };

  // Handlers para UI
  const handleEditClick = (cliente: User) => {
    setSelectedClient(cliente);
    setShowEditModal(true);
  };

  const handleDeleteClick = (cliente: User) => {
    setSelectedClient(cliente);
    setShowDeleteModal(true);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset para primeira página ao buscar
  };

  const handleRoleFilterChange = (value: string) => {
    setRoleFilter(value);
    setCurrentPage(1); // Reset para primeira página ao filtrar
  };

  // Loading state
  if (isLoading && clientes.length === 0) {
    return <LoadingSpinner fullScreen text="Carregando clientes..." />;
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Clientes</h1>
          <p className="text-gray-600 mt-1">
            Gerencie os clientes do sistema PSP ({totalClientes} total)
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
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Buscar
            </label>
            <input
              type="text"
              placeholder="Nome, email ou documento..."
              value={searchTerm}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Filtrar por Role
            </label>
            <select
              value={roleFilter}
              onChange={(e) => handleRoleFilterChange(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Todos os roles</option>
              <option value="cliente">Cliente</option>
              <option value="admin">Admin</option>
              <option value="sub-admin">Sub-Admin</option>
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={loadClientes}
              disabled={isLoading}
              className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-lg disabled:opacity-50 flex items-center"
            >
              {isLoading ? (
                <LoadingSpinner size="sm" color="gray" />
              ) : (
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
              )}
              Atualizar
            </button>
          </div>
        </div>
      </div>

      {/* Clientes Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Clientes ({clientes.length})
          </h3>
        </div>

        {isLoading ? (
          <div className="p-8 text-center">
            <LoadingSpinner size="lg" text="Carregando clientes..." />
          </div>
        ) : clientes.length === 0 ? (
          <div className="p-8 text-center">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">Nenhum cliente encontrado</h3>
            <p className="mt-1 text-sm text-gray-500">
              {searchTerm || roleFilter ? 'Tente ajustar os filtros de busca.' : 'Comece criando um novo cliente.'}
            </p>
            {!searchTerm && !roleFilter && (
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
                      Role
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Último Login
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
                  {clientes.map((cliente) => (
                    <tr key={cliente.id || cliente.userId} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {cliente.name || 'N/A'}
                          </div>
                          <div className="text-sm text-gray-500">{cliente.email}</div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          cliente.role === 'admin' ? 'bg-purple-100 text-purple-800' :
                          cliente.role === 'sub-admin' ? 'bg-blue-100 text-blue-800' :
                          'bg-green-100 text-green-800'
                        }`}>
                          {cliente.role}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {cliente.lastLogin ? new Date(cliente.lastLogin).toLocaleDateString('pt-BR') : 'Nunca'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          cliente.isActive !== false ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }`}>
                          {cliente.isActive !== false ? 'Ativo' : 'Inativo'}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleEditClick(cliente)}
                            className="text-blue-600 hover:text-blue-900"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => handleDeleteClick(cliente)}
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
              totalPages={Math.ceil(totalClientes / itemsPerPage)}
              totalItems={totalClientes}
              itemsPerPage={itemsPerPage}
              onPageChange={handlePageChange}
            />
          </>
        )}
      </div>

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Novo Cliente</h3>
            <Formik
              initialValues={{
                email: '',
                name: '',
                role: 'cliente',
                document: '',
                phone: '',
                password: ''
              }}
              validationSchema={createUserSchema}
              onSubmit={handleCreateClient}
            >
              {({ isSubmitting }) => (
                <Form className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Nome *
                    </label>
                    <Field
                      name="name"
                      type="text"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="Nome completo"
                    />
                    <ErrorMessage name="name" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Email *
                    </label>
                    <Field
                      name="email"
                      type="email"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="email@exemplo.com"
                    />
                    <ErrorMessage name="email" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Role *
                    </label>
                    <Field
                      name="role"
                      as="select"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="cliente">Cliente</option>
                      <option value="admin">Admin</option>
                      <option value="sub-admin">Sub-Admin</option>
                    </Field>
                    <ErrorMessage name="role" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Documento (CPF/CNPJ)
                    </label>
                    <Field
                      name="document"
                      type="text"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="12345678901 ou 12345678000123"
                    />
                    <ErrorMessage name="document" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Telefone
                    </label>
                    <Field
                      name="phone"
                      type="text"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="+5511999887766"
                    />
                    <ErrorMessage name="phone" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Senha (opcional)
                    </label>
                    <Field
                      name="password"
                      type="password"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="Deixe vazio para gerar automaticamente"
                    />
                    <ErrorMessage name="password" component="div" className="text-red-500 text-sm mt-1" />
                    <p className="text-xs text-gray-500 mt-1">
                      Se não informada, será gerada automaticamente e enviada por email
                    </p>
                  </div>

                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => setShowCreateModal(false)}
                      disabled={isSubmitting || isCreating}
                      className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 disabled:opacity-50"
                    >
                      Cancelar
                    </button>
                    <button
                      type="submit"
                      disabled={isSubmitting || isCreating}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center"
                    >
                      {isCreating ? (
                        <>
                          <LoadingSpinner size="sm" color="white" />
                          <span className="ml-2">Criando...</span>
                        </>
                      ) : (
                        'Criar Cliente'
                      )}
                    </button>
                  </div>
                </Form>
              )}
            </Formik>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {showEditModal && selectedClient && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Editar Cliente</h3>
            <Formik
              initialValues={{
                email: selectedClient.email || '',
                name: selectedClient.name || '',
                role: selectedClient.role || 'cliente',
                document: selectedClient.document || '',
                phone: selectedClient.phone || '',
                isActive: selectedClient.isActive !== false
              }}
              validationSchema={updateUserSchema}
              onSubmit={handleUpdateClient}
            >
              {({ isSubmitting }) => (
                <Form className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Nome
                    </label>
                    <Field
                      name="name"
                      type="text"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="Nome completo"
                    />
                    <ErrorMessage name="name" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Email
                    </label>
                    <Field
                      name="email"
                      type="email"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="email@exemplo.com"
                    />
                    <ErrorMessage name="email" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Role
                    </label>
                    <Field
                      name="role"
                      as="select"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="cliente">Cliente</option>
                      <option value="admin">Admin</option>
                      <option value="sub-admin">Sub-Admin</option>
                    </Field>
                    <ErrorMessage name="role" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Documento (CPF/CNPJ)
                    </label>
                    <Field
                      name="document"
                      type="text"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="12345678901 ou 12345678000123"
                    />
                    <ErrorMessage name="document" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Telefone
                    </label>
                    <Field
                      name="phone"
                      type="text"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      placeholder="+5511999887766"
                    />
                    <ErrorMessage name="phone" component="div" className="text-red-500 text-sm mt-1" />
                  </div>

                  <div>
                    <label className="flex items-center">
                      <Field
                        name="isActive"
                        type="checkbox"
                        className="rounded border-gray-300 text-blue-600 shadow-sm focus:border-blue-300 focus:ring focus:ring-blue-200 focus:ring-opacity-50"
                      />
                      <span className="ml-2 text-sm text-gray-700">Cliente ativo</span>
                    </label>
                  </div>

                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => {
                        setShowEditModal(false);
                        setSelectedClient(null);
                      }}
                      disabled={isSubmitting || isUpdating}
                      className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 disabled:opacity-50"
                    >
                      Cancelar
                    </button>
                    <button
                      type="submit"
                      disabled={isSubmitting || isUpdating}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center"
                    >
                      {isUpdating ? (
                        <>
                          <LoadingSpinner size="sm" color="white" />
                          <span className="ml-2">Atualizando...</span>
                        </>
                      ) : (
                        'Atualizar Cliente'
                      )}
                    </button>
                  </div>
                </Form>
              )}
            </Formik>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      <ConfirmModal
        isOpen={showDeleteModal}
        title="Excluir Cliente"
        message={`Tem certeza que deseja excluir o cliente "${selectedClient?.name || selectedClient?.email}"? Esta ação não pode ser desfeita.`}
        confirmText="Excluir"
        cancelText="Cancelar"
        onConfirm={handleDeleteClient}
        onCancel={() => {
          setShowDeleteModal(false);
          setSelectedClient(null);
        }}
        isLoading={isDeleting}
        type="danger"
      />
    </div>
  );
};

export default ClientesPage;
