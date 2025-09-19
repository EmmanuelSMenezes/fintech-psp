'use client';

import React, { useEffect, useState } from 'react';
import { Formik, Form, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import { useAuth, useRequireAuth } from '@/context/AuthContext';
import { 
  companyService, 
  Company, 
  CreateCompanyRequest, 
  CompanyData, 
  ApplicantData, 
  LegalRepresentativeData,
  CompanyStatus,
  RepresentationType
} from '@/services/api';
import toast from 'react-hot-toast';
import ConfirmModal from '@/components/ConfirmModal';
import LoadingSpinner from '@/components/LoadingSpinner';
import Pagination from '@/components/Pagination';
import FormField from '@/components/FormField';
import { formatDate, formatDocument, formatPhone, getInitials, getColorFromString } from '@/utils/formatters';

// Esquemas de validação para empresa
const createCompanySchema = Yup.object().shape({
  // Dados da empresa
  'company.razaoSocial': Yup.string()
    .min(2, 'Razão social deve ter pelo menos 2 caracteres')
    .max(200, 'Razão social deve ter no máximo 200 caracteres')
    .required('Razão social é obrigatória'),
  'company.cnpj': Yup.string()
    .matches(/^\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$/, 'CNPJ deve estar no formato 00.000.000/0000-00')
    .required('CNPJ é obrigatório'),
  'company.email': Yup.string()
    .email('Email inválido')
    .optional(),
  'company.address.cep': Yup.string()
    .matches(/^\d{5}-\d{3}$/, 'CEP deve estar no formato 00000-000')
    .required('CEP é obrigatório'),
  'company.address.logradouro': Yup.string()
    .required('Logradouro é obrigatório'),
  'company.address.numero': Yup.string()
    .required('Número é obrigatório'),
  'company.address.bairro': Yup.string()
    .required('Bairro é obrigatório'),
  'company.address.cidade': Yup.string()
    .required('Cidade é obrigatória'),
  'company.address.estado': Yup.string()
    .length(2, 'Estado deve ter 2 caracteres')
    .required('Estado é obrigatório'),

  // Dados do solicitante
  'applicant.nomeCompleto': Yup.string()
    .min(2, 'Nome deve ter pelo menos 2 caracteres')
    .max(100, 'Nome deve ter no máximo 100 caracteres')
    .required('Nome do solicitante é obrigatório'),
  'applicant.cpf': Yup.string()
    .matches(/^\d{3}\.\d{3}\.\d{3}-\d{2}$/, 'CPF deve estar no formato 000.000.000-00')
    .required('CPF do solicitante é obrigatório'),
  'applicant.email': Yup.string()
    .email('Email inválido')
    .optional(),
  'applicant.address.cep': Yup.string()
    .matches(/^\d{5}-\d{3}$/, 'CEP deve estar no formato 00000-000')
    .required('CEP do solicitante é obrigatório'),
  'applicant.address.logradouro': Yup.string()
    .required('Logradouro do solicitante é obrigatório'),
  'applicant.address.numero': Yup.string()
    .required('Número do solicitante é obrigatório'),
  'applicant.address.bairro': Yup.string()
    .required('Bairro do solicitante é obrigatório'),
  'applicant.address.cidade': Yup.string()
    .required('Cidade do solicitante é obrigatória'),
  'applicant.address.estado': Yup.string()
    .length(2, 'Estado deve ter 2 caracteres')
    .required('Estado do solicitante é obrigatório')
});

// Função para obter cor do status
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
      return 'bg-gray-100 text-gray-800';
  }
};

// Função para obter texto do status
const getStatusText = (status: CompanyStatus) => {
  switch (status) {
    case CompanyStatus.Active:
      return 'Ativa';
    case CompanyStatus.Approved:
      return 'Aprovada';
    case CompanyStatus.UnderReview:
      return 'Em Análise';
    case CompanyStatus.PendingDocuments:
      return 'Aguardando Docs';
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

const ClientesPage: React.FC = () => {
  useRequireAuth('manage_users');
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
  const [selectedCompany, setSelectedCompany] = useState<Company | null>(null);

  // Carregar empresas quando componente monta ou filtros mudam
  useEffect(() => {
    loadCompanies();
  }, [currentPage, searchTerm, statusFilter]);

  const loadCompanies = async () => {
    try {
      setIsLoading(true);
      console.log('🔄 Carregando empresas...', {
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm,
        status: statusFilter
      });

      const response = await companyService.getCompanies({
        page: currentPage,
        limit: itemsPerPage,
        search: searchTerm || undefined,
        status: statusFilter || undefined
      });

      console.log('📋 Resposta da API de empresas:', response.data);

      if (response.data && response.data.companies) {
        setCompanies(response.data.companies);
        setTotalCompanies(response.data.total || 0);
      } else {
        setCompanies([]);
        setTotalCompanies(0);
      }
    } catch (error: any) {
      console.error('❌ Erro ao carregar empresas:', error);

      if (error.response?.status === 401) {
        toast.error('Sessão expirada. Redirecionando para login...');
      } else if (error.response?.status === 403) {
        toast.error('Você não tem permissão para acessar esta funcionalidade');
      } else if (error.response?.status >= 500) {
        toast.error('Erro interno do servidor. Tente novamente mais tarde.');
      } else {
        toast.error('Erro ao carregar empresas');
      }

      setCompanies([]);
      setTotalCompanies(0);
    } finally {
      setIsLoading(false);
    }
  };

  // Criar empresa
  const handleCreateCompany = async (values: any) => {
    try {
      setIsCreating(true);
      console.log('🚀 Criando empresa:', values);

      const createRequest: CreateCompanyRequest = {
        company: values.company,
        applicant: values.applicant,
        legalRepresentatives: values.legalRepresentatives || []
      };

      await companyService.createCompany(createRequest);
      
      toast.success('Empresa criada com sucesso!');
      setShowCreateModal(false);
      loadCompanies();
    } catch (error: any) {
      console.error('❌ Erro ao criar empresa:', error);
      
      if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else {
        toast.error('Erro ao criar empresa');
      }
    } finally {
      setIsCreating(false);
    }
  };

  // Atualizar empresa
  const handleUpdateCompany = async (values: CompanyData) => {
    if (!selectedCompany) return;

    try {
      setIsUpdating(true);
      console.log('✏️ Atualizando empresa:', selectedCompany.id, values);

      await companyService.updateCompany(selectedCompany.id, values);
      
      toast.success('Empresa atualizada com sucesso!');
      setShowEditModal(false);
      setSelectedCompany(null);
      loadCompanies();
    } catch (error: any) {
      console.error('❌ Erro ao atualizar empresa:', error);
      
      if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else {
        toast.error('Erro ao atualizar empresa');
      }
    } finally {
      setIsUpdating(false);
    }
  };

  // Excluir empresa
  const handleDeleteCompany = async () => {
    if (!selectedCompany) return;

    try {
      setIsDeleting(true);
      console.log('🗑️ Excluindo empresa:', selectedCompany.id);

      await companyService.deleteCompany(selectedCompany.id);
      
      toast.success('Empresa excluída com sucesso!');
      setShowDeleteModal(false);
      setSelectedCompany(null);
      loadCompanies();
    } catch (error: any) {
      console.error('❌ Erro ao excluir empresa:', error);
      
      if (error.response?.data?.message) {
        toast.error(error.response.data.message);
      } else {
        toast.error('Erro ao excluir empresa');
      }
    } finally {
      setIsDeleting(false);
    }
  };

  // Handlers para UI
  const handleEditClick = (company: Company) => {
    setSelectedCompany(company);
    setShowEditModal(true);
  };

  const handleDeleteClick = (company: Company) => {
    setSelectedCompany(company);
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
    setStatusFilter(value as CompanyStatus | '');
    setCurrentPage(1);
  };

  // Loading state
  if (isLoading && companies.length === 0) {
    return <LoadingSpinner fullScreen text="Carregando empresas..." />;
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Empresas Clientes</h1>
          <p className="text-gray-600 mt-1">
            Gerencie as empresas clientes do sistema PSP ({totalCompanies} total)
          </p>
        </div>
        <button
          onClick={() => setShowCreateModal(true)}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium flex items-center"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          Nova Empresa
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
                placeholder="Razão social, CNPJ, email..."
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
              value={statusFilter}
              onChange={(e) => handleStatusFilterChange(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              <option value="">Todos os status</option>
              <option value={CompanyStatus.Active}>Ativa</option>
              <option value={CompanyStatus.Approved}>Aprovada</option>
              <option value={CompanyStatus.UnderReview}>Em Análise</option>
              <option value={CompanyStatus.PendingDocuments}>Aguardando Docs</option>
              <option value={CompanyStatus.Rejected}>Rejeitada</option>
              <option value={CompanyStatus.Suspended}>Suspensa</option>
              <option value={CompanyStatus.Inactive}>Inativa</option>
            </select>
          </div>

          {/* Botão Atualizar */}
          <div className="flex items-end">
            <button
              onClick={loadCompanies}
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

      {/* Empresas Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Empresas ({companies.length})
          </h3>
        </div>

        {isLoading ? (
          <div className="p-8 text-center">
            <LoadingSpinner size="lg" text="Carregando empresas..." />
          </div>
        ) : companies.length === 0 ? (
          <div className="p-8 text-center">
            <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
            <h3 className="mt-2 text-sm font-medium text-gray-900">Nenhuma empresa encontrada</h3>
            <p className="mt-1 text-sm text-gray-500">
              {searchTerm || statusFilter ? 'Tente ajustar os filtros de busca.' : 'Comece criando uma nova empresa.'}
            </p>
            {!searchTerm && !statusFilter && (
              <div className="mt-6">
                <button
                  onClick={() => setShowCreateModal(true)}
                  className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                >
                  <svg className="-ml-1 mr-2 h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Nova Empresa
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
                      Empresa
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      CNPJ
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
                  {companies.map((company) => (
                    <tr key={company.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div
                            className="flex-shrink-0 h-10 w-10 rounded-full flex items-center justify-center text-white text-sm font-medium"
                            style={{ backgroundColor: getColorFromString(company.razaoSocial) }}
                          >
                            {getInitials(company.razaoSocial)}
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {company.razaoSocial}
                            </div>
                            {company.nomeFantasia && (
                              <div className="text-sm text-gray-500">{company.nomeFantasia}</div>
                            )}
                            {company.email && (
                              <div className="text-xs text-gray-400">{company.email}</div>
                            )}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {formatDocument(company.cnpj)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(company.status)}`}>
                          {getStatusText(company.status)}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {formatDate(company.createdAt)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleEditClick(company)}
                            className="text-blue-600 hover:text-blue-900"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() => handleDeleteClick(company)}
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
              totalPages={Math.ceil(totalCompanies / itemsPerPage)}
              totalItems={totalCompanies}
              itemsPerPage={itemsPerPage}
              onPageChange={handlePageChange}
            />
          </>
        )}
      </div>

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Nova Empresa Cliente</h3>
            <Formik
              initialValues={{
                company: {
                  razaoSocial: '',
                  nomeFantasia: '',
                  cnpj: '',
                  inscricaoEstadual: '',
                  inscricaoMunicipal: '',
                  email: '',
                  telefone: '',
                  website: '',
                  address: {
                    cep: '',
                    logradouro: '',
                    numero: '',
                    complemento: '',
                    bairro: '',
                    cidade: '',
                    estado: '',
                    pais: 'Brasil'
                  },
                  contractData: {
                    numeroContrato: '',
                    dataContrato: '',
                    juntaComercial: '',
                    nire: '',
                    capitalSocial: 0,
                    atividadePrincipal: '',
                    atividadesSecundarias: []
                  },
                  observacoes: ''
                },
                applicant: {
                  nomeCompleto: '',
                  cpf: '',
                  rg: '',
                  orgaoExpedidor: '',
                  dataNascimento: '',
                  estadoCivil: '',
                  nacionalidade: 'Brasileira',
                  profissao: '',
                  email: '',
                  telefone: '',
                  celular: '',
                  address: {
                    cep: '',
                    logradouro: '',
                    numero: '',
                    complemento: '',
                    bairro: '',
                    cidade: '',
                    estado: '',
                    pais: 'Brasil'
                  },
                  rendaMensal: 0,
                  cargo: '',
                  isMainRepresentative: true
                },
                legalRepresentatives: []
              }}
              validationSchema={createCompanySchema}
              onSubmit={handleCreateCompany}
            >
              {({ isSubmitting, values, setFieldValue }) => (
                <Form className="space-y-6">
                  {/* Dados da Empresa */}
                  <div className="border-b border-gray-200 pb-6">
                    <h4 className="text-md font-medium text-gray-900 mb-4">Dados da Empresa</h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <FormField
                        name="company.razaoSocial"
                        label="Razão Social"
                        placeholder="Razão social da empresa"
                        required
                      />
                      <FormField
                        name="company.nomeFantasia"
                        label="Nome Fantasia"
                        placeholder="Nome fantasia (opcional)"
                      />
                      <FormField
                        name="company.cnpj"
                        label="CNPJ"
                        mask="cnpj"
                        placeholder="00.000.000/0000-00"
                        required
                      />
                      <FormField
                        name="company.inscricaoEstadual"
                        label="Inscrição Estadual"
                        placeholder="Inscrição estadual (opcional)"
                      />
                      <FormField
                        name="company.email"
                        label="Email"
                        type="email"
                        placeholder="contato@empresa.com"
                      />
                      <FormField
                        name="company.telefone"
                        label="Telefone"
                        mask="phone"
                        placeholder="(11) 3000-0000"
                      />
                    </div>
                  </div>

                  {/* Endereço da Empresa */}
                  <div className="border-b border-gray-200 pb-6">
                    <h4 className="text-md font-medium text-gray-900 mb-4">Endereço da Empresa</h4>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <FormField
                        name="company.address.cep"
                        label="CEP"
                        mask="cep"
                        placeholder="00000-000"
                        required
                      />
                      <FormField
                        name="company.address.logradouro"
                        label="Logradouro"
                        placeholder="Rua, Avenida, etc."
                        required
                        className="md:col-span-2"
                      />
                      <FormField
                        name="company.address.numero"
                        label="Número"
                        placeholder="123"
                        required
                      />
                      <FormField
                        name="company.address.complemento"
                        label="Complemento"
                        placeholder="Sala, Andar, etc."
                      />
                      <FormField
                        name="company.address.bairro"
                        label="Bairro"
                        placeholder="Nome do bairro"
                        required
                      />
                      <FormField
                        name="company.address.cidade"
                        label="Cidade"
                        placeholder="Nome da cidade"
                        required
                      />
                      <FormField
                        name="company.address.estado"
                        label="Estado"
                        placeholder="SP"
                        maxLength={2}
                        required
                      />
                    </div>
                  </div>

                  {/* Dados do Solicitante */}
                  <div className="border-b border-gray-200 pb-6">
                    <h4 className="text-md font-medium text-gray-900 mb-4">Dados do Solicitante (Pessoa Física)</h4>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <FormField
                        name="applicant.nomeCompleto"
                        label="Nome Completo"
                        placeholder="Nome completo do solicitante"
                        required
                      />
                      <FormField
                        name="applicant.cpf"
                        label="CPF"
                        mask="cpf"
                        placeholder="000.000.000-00"
                        required
                      />
                      <FormField
                        name="applicant.rg"
                        label="RG"
                        placeholder="00.000.000-0"
                      />
                      <FormField
                        name="applicant.orgaoExpedidor"
                        label="Órgão Expedidor"
                        placeholder="SSP/SP"
                      />
                      <FormField
                        name="applicant.email"
                        label="Email"
                        type="email"
                        placeholder="solicitante@email.com"
                      />
                      <FormField
                        name="applicant.telefone"
                        label="Telefone"
                        mask="phone"
                        placeholder="(11) 99999-9999"
                      />
                    </div>
                  </div>

                  {/* Endereço do Solicitante */}
                  <div className="border-b border-gray-200 pb-6">
                    <h4 className="text-md font-medium text-gray-900 mb-4">Endereço do Solicitante</h4>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <FormField
                        name="applicant.address.cep"
                        label="CEP"
                        mask="cep"
                        placeholder="00000-000"
                        required
                      />
                      <FormField
                        name="applicant.address.logradouro"
                        label="Logradouro"
                        placeholder="Rua, Avenida, etc."
                        required
                        className="md:col-span-2"
                      />
                      <FormField
                        name="applicant.address.numero"
                        label="Número"
                        placeholder="123"
                        required
                      />
                      <FormField
                        name="applicant.address.complemento"
                        label="Complemento"
                        placeholder="Apto, Casa, etc."
                      />
                      <FormField
                        name="applicant.address.bairro"
                        label="Bairro"
                        placeholder="Nome do bairro"
                        required
                      />
                      <FormField
                        name="applicant.address.cidade"
                        label="Cidade"
                        placeholder="Nome da cidade"
                        required
                      />
                      <FormField
                        name="applicant.address.estado"
                        label="Estado"
                        placeholder="SP"
                        maxLength={2}
                        required
                      />
                    </div>
                  </div>

                  {/* Botões */}
                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => setShowCreateModal(false)}
                      className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                    >
                      Cancelar
                    </button>
                    <button
                      type="submit"
                      disabled={isSubmitting || isCreating}
                      className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 disabled:opacity-50"
                    >
                      {isSubmitting || isCreating ? 'Criando...' : 'Criar Empresa'}
                    </button>
                  </div>
                </Form>
              )}
            </Formik>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {showEditModal && selectedCompany && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Editar Empresa</h3>
            <Formik
              initialValues={{
                razaoSocial: selectedCompany.razaoSocial || '',
                nomeFantasia: selectedCompany.nomeFantasia || '',
                cnpj: selectedCompany.cnpj || '',
                inscricaoEstadual: selectedCompany.inscricaoEstadual || '',
                inscricaoMunicipal: selectedCompany.inscricaoMunicipal || '',
                email: selectedCompany.email || '',
                telefone: selectedCompany.telefone || '',
                website: selectedCompany.website || '',
                address: {
                  cep: selectedCompany.address?.cep || '',
                  logradouro: selectedCompany.address?.logradouro || '',
                  numero: selectedCompany.address?.numero || '',
                  complemento: selectedCompany.address?.complemento || '',
                  bairro: selectedCompany.address?.bairro || '',
                  cidade: selectedCompany.address?.cidade || '',
                  estado: selectedCompany.address?.estado || '',
                  pais: selectedCompany.address?.pais || 'Brasil'
                },
                contractData: {
                  numeroContrato: selectedCompany.contractData?.numeroContrato || '',
                  dataContrato: selectedCompany.contractData?.dataContrato || '',
                  juntaComercial: selectedCompany.contractData?.juntaComercial || '',
                  nire: selectedCompany.contractData?.nire || '',
                  capitalSocial: selectedCompany.contractData?.capitalSocial || 0,
                  atividadePrincipal: selectedCompany.contractData?.atividadePrincipal || '',
                  atividadesSecundarias: selectedCompany.contractData?.atividadesSecundarias || []
                },
                observacoes: selectedCompany.observacoes || ''
              }}
              onSubmit={handleUpdateCompany}
            >
              {({ isSubmitting }) => (
                <Form className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      name="razaoSocial"
                      label="Razão Social"
                      placeholder="Razão social da empresa"
                      required
                    />
                    <FormField
                      name="nomeFantasia"
                      label="Nome Fantasia"
                      placeholder="Nome fantasia (opcional)"
                    />
                    <FormField
                      name="cnpj"
                      label="CNPJ"
                      mask="cnpj"
                      placeholder="00.000.000/0000-00"
                      required
                    />
                    <FormField
                      name="inscricaoEstadual"
                      label="Inscrição Estadual"
                      placeholder="Inscrição estadual (opcional)"
                    />
                    <FormField
                      name="email"
                      label="Email"
                      type="email"
                      placeholder="contato@empresa.com"
                    />
                    <FormField
                      name="telefone"
                      label="Telefone"
                      mask="phone"
                      placeholder="(11) 3000-0000"
                    />
                  </div>

                  <div className="border-t border-gray-200 pt-4">
                    <h4 className="text-md font-medium text-gray-900 mb-4">Endereço</h4>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <FormField
                        name="address.cep"
                        label="CEP"
                        mask="cep"
                        placeholder="00000-000"
                        required
                      />
                      <FormField
                        name="address.logradouro"
                        label="Logradouro"
                        placeholder="Rua, Avenida, etc."
                        required
                        className="md:col-span-2"
                      />
                      <FormField
                        name="address.numero"
                        label="Número"
                        placeholder="123"
                        required
                      />
                      <FormField
                        name="address.complemento"
                        label="Complemento"
                        placeholder="Sala, Andar, etc."
                      />
                      <FormField
                        name="address.bairro"
                        label="Bairro"
                        placeholder="Nome do bairro"
                        required
                      />
                      <FormField
                        name="address.cidade"
                        label="Cidade"
                        placeholder="Nome da cidade"
                        required
                      />
                      <FormField
                        name="address.estado"
                        label="Estado"
                        placeholder="SP"
                        maxLength={2}
                        required
                      />
                    </div>
                  </div>

                  <FormField
                    name="observacoes"
                    label="Observações"
                    as="textarea"
                    rows={3}
                    placeholder="Observações adicionais..."
                  />

                  <div className="flex justify-end space-x-3 pt-4">
                    <button
                      type="button"
                      onClick={() => {
                        setShowEditModal(false);
                        setSelectedCompany(null);
                      }}
                      className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                    >
                      Cancelar
                    </button>
                    <button
                      type="submit"
                      disabled={isSubmitting || isUpdating}
                      className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 disabled:opacity-50"
                    >
                      {isSubmitting || isUpdating ? 'Salvando...' : 'Salvar Alterações'}
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
    </div>
  );
};

export default ClientesPage;
