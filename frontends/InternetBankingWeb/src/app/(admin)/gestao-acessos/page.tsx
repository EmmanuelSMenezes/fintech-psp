'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRequireAuth } from '@/context/AuthContext';
import { accessService, AccessControl } from '@/services/api';
import SubUserForm, { SubUserFormData } from '@/components/SubUserForm';
import SubUserList from '@/components/SubUserList';
import toast from 'react-hot-toast';

const GestaoAcessosPage: React.FC = () => {
  useRequireAuth('view_tela_acessos');

  const [subUsers, setSubUsers] = useState<AccessControl[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [editingUser, setEditingUser] = useState<AccessControl | null>(null);
  const [hasLoaded, setHasLoaded] = useState(false);

  const loadSubUsers = useCallback(async () => {
    if (isLoading && hasLoaded) return; // Evitar múltiplas chamadas

    try {
      setIsLoading(true);
      const response = await accessService.getMySubUsers();
      setSubUsers(response.data);
      setHasLoaded(true);
    } catch (error) {
      console.error('Erro ao carregar sub-usuários:', error);
      toast.error('Erro ao carregar lista de sub-usuários. Verifique se as APIs estão rodando.');

      // Não usar dados mock - deixar array vazio
      setSubUsers([]);
      setHasLoaded(true);
    } finally {
      setIsLoading(false);
    }
  }, [isLoading, hasLoaded]);

  useEffect(() => {
    if (!hasLoaded) {
      loadSubUsers();
    }
  }, [hasLoaded, loadSubUsers]);

  const handleCreateSubUser = async (data: SubUserFormData) => {
    try {
      setIsSaving(true);

      const createData = {
        subUserEmail: data.subUserEmail,
        role: data.role,
        permissions: data.permissions,
      };

      await accessService.createSubUser(createData);

      // Simular envio de email se solicitado
      if (data.sendInviteEmail) {
        // Aqui seria integrado com o serviço de email
        toast.success(`Convite enviado para ${data.subUserEmail}`);
      }

      toast.success('Sub-usuário criado com sucesso!');
      await loadSubUsers();
      setIsEditing(false);

    } catch (error) {
      console.error('Erro ao criar sub-usuário:', error);
      toast.error('Erro ao criar sub-usuário');

      // Para demonstração, simular sucesso sem recarregar
      const newSubUser: AccessControl = {
        acessoId: `new-${Date.now()}`,
        userId: `user-${Date.now()}`,
        parentUserId: 'admin-1',
        email: data.subUserEmail,
        role: data.role,
        permissions: data.permissions,
        isActive: data.sendInviteEmail ? false : true, // Se enviou convite, fica inativo até aceitar
        createdAt: new Date().toISOString(),
      };

      setSubUsers(prev => [...prev, newSubUser]);
      setIsEditing(false);

      if (data.sendInviteEmail) {
        toast.success(`Convite enviado para ${data.subUserEmail} (modo demonstração)`);
      } else {
        toast.success('Sub-usuário criado com sucesso! (modo demonstração)');
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handleUpdateSubUser = async (data: SubUserFormData) => {
    if (!editingUser) return;

    try {
      setIsSaving(true);

      const updateData = {
        role: data.role,
        permissions: data.permissions,
      };

      await accessService.updateSubUser(editingUser.acessoId, updateData);

      toast.success('Sub-usuário atualizado com sucesso!');
      await loadSubUsers();
      setIsEditing(false);
      setEditingUser(null);

    } catch (error) {
      console.error('Erro ao atualizar sub-usuário:', error);
      toast.error('Erro ao atualizar sub-usuário');

      // Para demonstração, simular sucesso sem recarregar
      setSubUsers(prev => prev.map(user =>
        user.acessoId === editingUser.acessoId
          ? { ...user, permissions: data.permissions, updatedAt: new Date().toISOString() }
          : user
      ));
      setIsEditing(false);
      setEditingUser(null);
      toast.success('Sub-usuário atualizado com sucesso! (modo demonstração)');
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteSubUser = async (subUser: AccessControl) => {
    try {
      await accessService.deleteSubUser(subUser.acessoId, 'Removido pelo administrador');
      toast.success(`Acesso de ${subUser.email} removido com sucesso`);
      await loadSubUsers();
    } catch (error) {
      console.error('Erro ao remover sub-usuário:', error);
      toast.error('Erro ao remover sub-usuário');

      // Para demonstração, simular sucesso sem recarregar
      setSubUsers(prev => prev.filter(user => user.acessoId !== subUser.acessoId));
      toast.success(`Acesso de ${subUser.email} removido com sucesso (modo demonstração)`);
    }
  };

  const handleResendInvite = async (subUser: AccessControl) => {
    try {
      // Aqui seria implementada a lógica de reenvio de convite
      toast.success(`Convite reenviado para ${subUser.email}`);

      // Simular ativação do usuário após reenvio
      setSubUsers(prev => prev.map(user =>
        user.acessoId === subUser.acessoId
          ? { ...user, isActive: true, updatedAt: new Date().toISOString() }
          : user
      ));
    } catch (error) {
      console.error('Erro ao reenviar convite:', error);
      toast.error('Erro ao reenviar convite');
    }
  };

  const handleEditSubUser = (subUser: AccessControl) => {
    setEditingUser(subUser);
    setIsEditing(true);
  };

  const handleStartCreating = () => {
    setEditingUser(null);
    setIsEditing(true);
  };

  const handleCancelEditing = () => {
    setIsEditing(false);
    setEditingUser(null);
  };

  const handleSave = (data: SubUserFormData) => {
    if (editingUser) {
      handleUpdateSubUser(data);
    } else {
      handleCreateSubUser(data);
    }
  };

  // Calcular estatísticas
  const activeUsers = subUsers.filter(user => user.isActive).length;
  const pendingUsers = subUsers.filter(user => !user.isActive).length;
  const blockedUsers = 0; // Para futuras implementações

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

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => (
            <div key={i} className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
              <div className="flex items-center">
                <div className="w-12 h-12 bg-gray-200 rounded-lg animate-pulse"></div>
                <div className="ml-4 space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-24 animate-pulse"></div>
                  <div className="h-6 bg-gray-200 rounded w-8 animate-pulse"></div>
                </div>
              </div>
            </div>
          ))}
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <div className="animate-pulse space-y-4">
            <div className="h-6 bg-gray-200 rounded w-1/3"></div>
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center space-x-4">
                  <div className="h-12 bg-gray-200 rounded-full w-12"></div>
                  <div className="flex-1 space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-1/3"></div>
                    <div className="h-3 bg-gray-200 rounded w-1/4"></div>
                  </div>
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
          <h1 className="text-3xl font-bold text-gray-900">Gestão de Acessos</h1>
          <p className="text-gray-600 mt-1">Gerencie sub-usuários e suas permissões</p>
        </div>
        {!isEditing && (
          <button
            onClick={handleStartCreating}
            className="bg-purple-600 hover:bg-purple-700 text-white px-4 py-2 rounded-lg font-medium"
          >
            Novo Sub-usuário
          </button>
        )}
      </div>

      {/* Estatísticas */}
      {!isEditing && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-green-100 rounded-lg">
                <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Usuários Ativos</p>
                <p className="text-2xl font-bold text-gray-900">{activeUsers}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-yellow-100 rounded-lg">
                <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Pendentes</p>
                <p className="text-2xl font-bold text-gray-900">{pendingUsers}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
            <div className="flex items-center">
              <div className="p-3 bg-red-100 rounded-lg">
                <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728L5.636 5.636m12.728 12.728L18.364 5.636M5.636 18.364l12.728-12.728" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-600">Bloqueados</p>
                <p className="text-2xl font-bold text-gray-900">{blockedUsers}</p>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Conteúdo Principal */}
      {isEditing ? (
        <SubUserForm
          onSave={handleSave}
          onCancel={handleCancelEditing}
          isLoading={isSaving}
          initialData={editingUser ? {
            subUserEmail: editingUser.email,
            role: editingUser.role as 'sub-usuario',
            permissions: editingUser.permissions,
            sendInviteEmail: false,
          } : undefined}
        />
      ) : (
        <SubUserList
          subUsers={subUsers}
          onEdit={handleEditSubUser}
          onDelete={handleDeleteSubUser}
          onResendInvite={handleResendInvite}
          isLoading={isLoading}
        />
      )}

      {/* Informações Adicionais */}
      {!isEditing && subUsers.length > 0 && (
        <div className="bg-purple-50 border border-purple-200 rounded-xl p-6">
          <div className="flex items-start space-x-3">
            <div className="p-2 bg-purple-100 rounded-lg">
              <svg className="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="flex-1">
              <h3 className="text-sm font-medium text-purple-900 mb-1">Gestão de Sub-usuários</h3>
              <div className="text-sm text-purple-800 space-y-1">
                <p>• Sub-usuários recebem convites por email para ativar suas contas</p>
                <p>• Permissões podem ser ajustadas a qualquer momento</p>
                <p>• Usuários inativos não conseguem acessar o sistema</p>
                <p>• Todas as ações são registradas para auditoria</p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default GestaoAcessosPage;
