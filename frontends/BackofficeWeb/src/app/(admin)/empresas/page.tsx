'use client';

import React, { useEffect, useState } from 'react';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import {
  companyService,
  Company,
  CompanyStatus,
  CreateCompanyRequest,
  CompanyData
} from '@/services/api';
import toast from 'react-hot-toast';
import ConfirmModal from '@/components/ConfirmModal';
import LoadingSpinner from '@/components/LoadingSpinner';
import Pagination from '@/components/Pagination';
import CustomModal from '@/components/CustomModal';
import CompanyForm from '@/components/CompanyForm';
import { formatDate, getInitials, getColorFromString } from '@/utils/formatters';

// Função para obter cor do status da empresa
const getStatusColor = (status: CompanyStatus) => {
  switch (status) {
    case CompanyStatus.Active:
      return 'bg-green-100 text-green-800';
    case CompanyStatus.Approved:
      return 'bg-blue-100 text-blue-800';
    case CompanyStatus.UnderReview:
      return 'bg-yellow-100 text-yellow-800';
    case CompanyStatus.PendingDocuments:
      return 'bg-orange-100 text-orange-800';
    case CompanyStatus.Rejected:
      return 'bg-red-100 text-red-800';
    case CompanyStatus.Suspended:
      return 'bg-gray-100 text-gray-800';
    case CompanyStatus.Inactive:
      return 'bg-gray-100 text-gray-600';
    default:
      return 'bg-gray-100 text-gray-600';
  }
};

// Função para obter texto do status da empresa
const getStatusText = (status: CompanyStatus) => {
  switch (status) {
    case CompanyStatus.Active:
      return 'Ativa';
    case CompanyStatus.Approved:
      return 'Aprovada';
    case CompanyStatus.UnderReview:
      return 'Em Análise';
    case CompanyStatus.PendingDocuments:
      return 'Pendente Documentos';
    case CompanyStatus.Rejected:
      return 'Rejeitada';
    case CompanyStatus.Suspended:
      return 'Suspensa';
    case CompanyStatus.Inactive:
      return 'Inativa';
    default:
      return 'Desconhecido';
  }
};

const EmpresasPage: React.FC = () => {
  useRequireAuth('manage_companies');
  const { user } = useAuth();

  // Estados principais
  const [companies, setCompanies] = useState<Company[]>([]);
  const [totalCompanies, setTotalCompanies] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isCreating, setIsCreating] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  // Estados de filtros e paginação
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<CompanyStatus | ''>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  // Estados de modais
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [selectedCompany, setSelectedCompany] = useState<Company | null>(null);

  useEffect(() => {
    loadCompanies();
  }, [currentPage, searchTerm, statusFilter]);

  const loadCompanies = async () => {
    try {
      setIsLoading(true);
      const response = await companyService.getCompanies({
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm || undefined,
        status: statusFilter !== '' ? statusFilter : undefined,
      });

      setCompanies(response.data.companies || []);
      setTotalCompanies(response.data.total || 0);
    } catch (error) {
      console.error('Erro ao carregar empresas:', error);
      toast.error('Erro ao carregar empresas');
      setCompanies([]);
      setTotalCompanies(0);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateCompany = async (companyData: CreateCompanyRequest) => {
    try {
      setIsCreating(true);
      await companyService.createCompany(companyData);
      toast.success('Empresa criada com sucesso!');
      setShowCreateModal(false);
      loadCompanies();
    } catch (error: any) {
      console.error('Erro ao criar empresa:', error);
      const errorMessage = error.response?.data?.message || 'Erro ao criar empresa';
      toast.error(errorMessage);
    } finally {
      setIsCreating(false);
    }
  };

  const handleUpdateCompany = async (companyData: CompanyData) => {
    if (!selectedCompany) return;

    try {
      setIsUpdating(true);
      await companyService.updateCompany(selectedCompany.id, companyData);
      toast.success('Empresa atualizada com sucesso!');
      setShowEditModal(false);
      setSelectedCompany(null);
      loadCompanies();
    } catch (error: any) {
      console.error('Erro ao atualizar empresa:', error);
      const errorMessage = error.response?.data?.message || 'Erro ao atualizar empresa';
      toast.error(errorMessage);
    } finally {
      setIsUpdating(false);
    }
  };

  const handleDeleteCompany = async () => {
    if (!selectedCompany) return;

    try {
      setIsDeleting(true);
      await companyService.deleteCompany(selectedCompany.id);
      toast.success('Empresa excluída com sucesso!');
      setShowDeleteModal(false);
      setSelectedCompany(null);
      loadCompanies();
    } catch (error: any) {
      console.error('Erro ao excluir empresa:', error);
      const errorMessage = error.response?.data?.message || 'Erro ao excluir empresa';
      toast.error(errorMessage);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleUpdateStatus = async (status: CompanyStatus, observacoes?: string) => {
    if (!selectedCompany) return;

    try {
      setIsUpdating(true);
      await companyService.updateCompanyStatus(selectedCompany.id, status, observacoes);
      toast.success('Status da empresa atualizado com sucesso!');
      setShowStatusModal(false);
      setSelectedCompany(null);
      loadCompanies();
    } catch (error: any) {
      console.error('Erro ao atualizar status:', error);
      const errorMessage = error.response?.data?.message || 'Erro ao atualizar status';
      toast.error(errorMessage);
    } finally {
      setIsUpdating(false);
    }
  };

  const openEditModal = (company: Company) => {
    setSelectedCompany(company);
    setShowEditModal(true);
  };

  const openDeleteModal = (company: Company) => {
    setSelectedCompany(company);
    setShowDeleteModal(true);
  };

  const openStatusModal = (company: Company) => {
    setSelectedCompany(company);
    setShowStatusModal(true);
  };

  const totalPages = Math.ceil(totalCompanies / itemsPerPage);

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Empresas</h1>
          <p className="text-gray-600 mt-1">Gerencie empresas clientes do sistema</p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center space-x-2"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          <span>Nova Empresa</span>
        </button>
      </div>

      {/* Filtros */}
      <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Buscar empresas
            </label>
            <input
              type="text"
              placeholder="Razão social, CNPJ ou nome fantasia..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Status
            </label>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as CompanyStatus | '')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Todos</option>
              <option value={CompanyStatus.Active}>Ativa</option>
              <option value={CompanyStatus.Approved}>Aprovada</option>
              <option value={CompanyStatus.UnderReview}>Em Análise</option>
              <option value={CompanyStatus.PendingDocuments}>Pendente Documentos</option>
              <option value={CompanyStatus.Rejected}>Rejeitada</option>
              <option value={CompanyStatus.Suspended}>Suspensa</option>
              <option value={CompanyStatus.Inactive}>Inativa</option>
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={() => {
                setSearchTerm('');
                setStatusFilter('');
                setCurrentPage(1);
              }}
              className="px-4 py-2 text-gray-600 hover:text-gray-800 border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              Limpar Filtros
            </button>
          </div>
        </div>
      </div>

      {/* Lista de Empresas */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="px-6 py-4 border-b border-gray-100">
          <h3 className="text-lg font-semibold text-gray-900">
            Empresas ({totalCompanies})
          </h3>
        </div>

        {companies.length === 0 ? (
          <div className="text-center py-12">
            <div className="text-6xl text-gray-300 mb-4">🏢</div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              Nenhuma empresa encontrada
            </h3>
            <p className="text-gray-500">
              {searchTerm || statusFilter !== ''
                ? 'Tente ajustar os filtros de busca'
                : 'Comece criando a primeira empresa'
              }
            </p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Empresa
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      CNPJ
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Criada em
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Ações
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {companies.map((company) => (
                    <tr key={company.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div
                            className="h-10 w-10 rounded-full flex items-center justify-center text-white font-medium text-sm"
                            style={{ backgroundColor: getColorFromString(company.razaoSocial) }}
                          >
                            {getInitials(company.razaoSocial)}
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {company.razaoSocial}
                            </div>
                            <div className="text-sm text-gray-500">
                              {company.nomeFantasia || '-'}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {company.cnpj}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(company.status)}`}>
                          {getStatusText(company.status)}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(company.createdAt)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex items-center justify-end space-x-2">
                          <button
                            onClick={() => openStatusModal(company)}
                            className="text-yellow-600 hover:text-yellow-900"
                            title="Alterar status"
                          >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                          </button>
                          <button
                            onClick={() => openEditModal(company)}
                            className="text-blue-600 hover:text-blue-900"
                            title="Editar empresa"
                          >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                            </svg>
                          </button>
                          <button
                            onClick={() => openDeleteModal(company)}
                            className="text-red-600 hover:text-red-900"
                            title="Excluir empresa"
                          >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Paginação */}
            {totalPages > 1 && (
              <div className="px-6 py-4 border-t border-gray-100">
                <Pagination
                  currentPage={currentPage}
                  totalPages={totalPages}
                  onPageChange={setCurrentPage}
                />
              </div>
            )}
          </>
        )}
      </div>

      {/* Modais */}
      <CustomModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        title="Criar Nova Empresa"
        size="xl"
      >
        <CompanyForm
          onSubmit={handleCreateCompany}
          onCancel={() => setShowCreateModal(false)}
          isLoading={isCreating}
        />
      </CustomModal>

      <CustomModal
        isOpen={showEditModal}
        onClose={() => {
          setShowEditModal(false);
          setSelectedCompany(null);
        }}
        title="Editar Empresa"
        size="xl"
      >
        {selectedCompany && (
          <CompanyForm
            initialData={selectedCompany}
            onSubmit={handleUpdateCompany}
            onCancel={() => {
              setShowEditModal(false);
              setSelectedCompany(null);
            }}
            isLoading={isUpdating}
          />
        )}
      </CustomModal>

      <ConfirmModal
        isOpen={showDeleteModal}
        onClose={() => {
          setShowDeleteModal(false);
          setSelectedCompany(null);
        }}
        onConfirm={handleDeleteCompany}
        title="Excluir Empresa"
        message={`Tem certeza que deseja excluir a empresa "${selectedCompany?.razaoSocial}"? Esta ação não pode ser desfeita.`}
        confirmText="Excluir"
        cancelText="Cancelar"
        isLoading={isDeleting}
        type="danger"
      />

      {/* Modal de Status */}
      <CustomModal
        isOpen={showStatusModal}
        onClose={() => {
          setShowStatusModal(false);
          setSelectedCompany(null);
        }}
        title="Alterar Status da Empresa"
        size="md"
      >
        {selectedCompany && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Novo Status
              </label>
              <select
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                onChange={(e) => {
                  const status = e.target.value as CompanyStatus;
                  handleUpdateStatus(status);
                }}
              >
                <option value="">Selecione um status</option>
                <option value={CompanyStatus.Active}>Ativa</option>
                <option value={CompanyStatus.Approved}>Aprovada</option>
                <option value={CompanyStatus.UnderReview}>Em Análise</option>
                <option value={CompanyStatus.PendingDocuments}>Pendente Documentos</option>
                <option value={CompanyStatus.Rejected}>Rejeitada</option>
                <option value={CompanyStatus.Suspended}>Suspensa</option>
                <option value={CompanyStatus.Inactive}>Inativa</option>
              </select>
            </div>
          </div>
        )}
      </CustomModal>
    </div>
  );
};

export default EmpresasPage;
