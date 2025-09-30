'use client';

import React, { useState } from 'react';
import { AccessControl } from '@/services/api';

interface SubUserListProps {
  subUsers: AccessControl[];
  onEdit: (subUser: AccessControl) => void;
  onDelete: (subUser: AccessControl) => void;
  onResendInvite: (subUser: AccessControl) => void;
  isLoading?: boolean;
}

const SubUserList: React.FC<SubUserListProps> = ({ 
  subUsers, 
  onEdit, 
  onDelete, 
  onResendInvite,
  isLoading = false 
}) => {
  const [showDeleteModal, setShowDeleteModal] = useState<AccessControl | null>(null);
  const [deleteReason, setDeleteReason] = useState('');

  const getStatusColor = (isActive: boolean) => {
    return isActive 
      ? 'bg-green-100 text-green-800' 
      : 'bg-red-100 text-red-800';
  };

  const getStatusText = (isActive: boolean) => {
    return isActive ? 'Ativo' : 'Inativo';
  };

  const getPermissionName = (permission: string) => {
    const permissionNames: { [key: string]: string } = {
      'view_tela_dashboard': 'Dashboard',
      'view_tela_contas': 'Contas',
      'view_tela_transacoes': 'Transações',
      'view_tela_historico': 'Histórico',
      'transacionar_pix': 'PIX',
      'exportar_relatorios': 'Relatórios',
    };
    return permissionNames[permission] || permission;
  };

  const handleDeleteClick = (subUser: AccessControl) => {
    setShowDeleteModal(subUser);
    setDeleteReason('');
  };

  const handleDeleteConfirm = () => {
    if (showDeleteModal) {
      onDelete(showDeleteModal);
      setShowDeleteModal(null);
      setDeleteReason('');
    }
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(null);
    setDeleteReason('');
  };

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-6 border-b border-gray-200">
          <div className="h-6 bg-gray-200 rounded w-48 animate-pulse"></div>
        </div>
        <div className="divide-y divide-gray-200">
          {[1, 2, 3].map((i) => (
            <div key={i} className="p-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <div className="w-12 h-12 bg-gray-200 rounded-full animate-pulse"></div>
                  <div className="space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-32 animate-pulse"></div>
                    <div className="h-3 bg-gray-200 rounded w-24 animate-pulse"></div>
                  </div>
                </div>
                <div className="flex space-x-2">
                  <div className="h-8 bg-gray-200 rounded w-16 animate-pulse"></div>
                  <div className="h-8 bg-gray-200 rounded w-16 animate-pulse"></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (subUsers.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-8 text-center">
          <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mx-auto mb-4">
            <svg className="w-8 h-8 text-gray-400 mx-auto mt-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Nenhum sub-usuário cadastrado</h3>
          <p className="text-gray-600">
            Adicione sub-usuários para compartilhar o acesso ao sistema com sua equipe.
          </p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Sub-usuários ({subUsers.length})
          </h3>
        </div>

        <div className="divide-y divide-gray-200">
          {subUsers.map((subUser) => (
            <div key={subUser.acessoId} className="p-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  {/* Avatar */}
                  <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </div>

                  {/* Informações do Usuário */}
                  <div>
                    <div className="flex items-center space-x-2">
                      <h4 className="text-sm font-medium text-gray-900">
                        {subUser.email}
                      </h4>
                      <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(subUser.isActive)}`}>
                        {getStatusText(subUser.isActive)}
                      </span>
                    </div>
                    
                    <div className="mt-1">
                      <p className="text-xs text-gray-600">
                        Função: {subUser.role === 'sub-usuario' ? 'Sub-usuário' : subUser.role}
                      </p>
                      <p className="text-xs text-gray-600">
                        Criado em: {new Date(subUser.createdAt).toLocaleDateString('pt-BR')}
                      </p>
                      {subUser.updatedAt && (
                        <p className="text-xs text-gray-600">
                          Atualizado em: {new Date(subUser.updatedAt).toLocaleDateString('pt-BR')}
                        </p>
                      )}
                    </div>

                    {/* Permissões */}
                    <div className="mt-3">
                      <p className="text-xs text-gray-700 font-medium mb-1">
                        Permissões ({subUser.permissions.length}):
                      </p>
                      <div className="flex flex-wrap gap-1">
                        {subUser.permissions.slice(0, 4).map((permission) => (
                          <span
                            key={permission}
                            className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                          >
                            {getPermissionName(permission)}
                          </span>
                        ))}
                        {subUser.permissions.length > 4 && (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                            +{subUser.permissions.length - 4} mais
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>

                {/* Ações */}
                <div className="flex items-center space-x-2">
                  {!subUser.isActive && (
                    <button
                      onClick={() => onResendInvite(subUser)}
                      className="text-xs text-blue-600 hover:text-blue-700 px-2 py-1 border border-blue-200 rounded"
                      title="Reenviar convite"
                    >
                      Reenviar Convite
                    </button>
                  )}
                  
                  <button
                    onClick={() => onEdit(subUser)}
                    className="text-xs text-gray-600 hover:text-gray-700 px-2 py-1 border border-gray-200 rounded"
                  >
                    Editar
                  </button>
                  
                  <button
                    onClick={() => handleDeleteClick(subUser)}
                    className="text-xs text-red-600 hover:text-red-700 px-2 py-1 border border-red-200 rounded"
                  >
                    Remover
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Modal de Confirmação de Exclusão */}
      {showDeleteModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full">
            <div className="p-6">
              <div className="flex items-center mb-4">
                <div className="p-2 bg-red-100 rounded-full mr-3">
                  <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">Remover Sub-usuário</h3>
                  <p className="text-sm text-gray-600">Esta ação não pode ser desfeita</p>
                </div>
              </div>

              <div className="mb-4">
                <p className="text-sm text-gray-700 mb-2">
                  Tem certeza que deseja remover o acesso de <strong>{showDeleteModal.email}</strong>?
                </p>
                
                <div className="mt-3">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Motivo da remoção (opcional)
                  </label>
                  <textarea
                    value={deleteReason}
                    onChange={(e) => setDeleteReason(e.target.value)}
                    rows={3}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Ex: Funcionário desligado, mudança de função..."
                  />
                </div>
              </div>

              <div className="flex justify-end space-x-3">
                <button
                  onClick={handleDeleteCancel}
                  className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleDeleteConfirm}
                  className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                >
                  Remover Sub-usuário
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default SubUserList;
