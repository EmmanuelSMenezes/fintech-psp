'use client';

import React, { useState, useEffect } from 'react';
import { useRequireAuth } from '@/context/AuthContext';
import { bankAccountService, BankAccount, CreateBankAccountRequest } from '@/services/api';
import BankAccountForm from '@/components/BankAccountForm';
import BankAccountList from '@/components/BankAccountList';
import toast from 'react-hot-toast';

const ContasPage: React.FC = () => {
  useRequireAuth('view_tela_contas');

  const [accounts, setAccounts] = useState<BankAccount[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingAccount, setEditingAccount] = useState<BankAccount | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      setIsLoading(true);
      const response = await bankAccountService.getMyAccounts();
      setAccounts(response.data);
    } catch (error) {
      console.error('Erro ao carregar contas:', error);
      toast.error('Erro ao carregar contas bancárias. Verifique se as APIs estão rodando.');
      // Não usar dados mock - deixar array vazio
      setAccounts([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateAccount = async (data: Omit<CreateBankAccountRequest, 'clienteId'>) => {
    try {
      setIsSubmitting(true);
      await bankAccountService.createMyAccount(data);
      toast.success('Conta bancária criada com sucesso!');
      setShowForm(false);
      await loadAccounts();
    } catch (error) {
      console.error('Erro ao criar conta:', error);
      toast.error('Erro ao criar conta bancária');
      throw error;
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdateAccount = async (data: Omit<CreateBankAccountRequest, 'clienteId'>) => {
    if (!editingAccount) return;

    try {
      setIsSubmitting(true);
      await bankAccountService.updateMyAccount(editingAccount.contaId, data);
      toast.success('Conta bancária atualizada com sucesso!');
      setEditingAccount(null);
      setShowForm(false);
      await loadAccounts();
    } catch (error) {
      console.error('Erro ao atualizar conta:', error);
      toast.error('Erro ao atualizar conta bancária');
      throw error;
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteAccount = async (accountId: string) => {
    try {
      await bankAccountService.deleteMyAccount(accountId);
      toast.success('Conta bancária excluída com sucesso!');
      await loadAccounts();
    } catch (error) {
      console.error('Erro ao excluir conta:', error);
      toast.error('Erro ao excluir conta bancária');
    }
  };

  const handleToggleStatus = async (accountId: string, isActive: boolean) => {
    try {
      // Em uma implementação real, haveria um endpoint específico para isso
      // Por enquanto, vamos simular a atualização local
      setAccounts(prev =>
        prev.map(account =>
          account.contaId === accountId
            ? { ...account, isActive, updatedAt: new Date().toISOString() }
            : account
        )
      );
      toast.success(`Conta ${isActive ? 'ativada' : 'desativada'} com sucesso!`);
    } catch (error) {
      console.error('Erro ao alterar status da conta:', error);
      toast.error('Erro ao alterar status da conta');
    }
  };

  const handleEdit = (account: BankAccount) => {
    setEditingAccount(account);
    setShowForm(true);
  };

  const handleCancelForm = () => {
    setShowForm(false);
    setEditingAccount(null);
  };

  const handleNewAccount = () => {
    setEditingAccount(null);
    setShowForm(true);
  };

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Contas Bancárias</h1>
          <p className="text-gray-600 mt-1">Gerencie suas contas bancárias da empresa</p>
        </div>
        {!showForm && (
          <button
            onClick={handleNewAccount}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium"
          >
            Nova Conta
          </button>
        )}
      </div>

      {/* Formulário ou Lista */}
      {showForm ? (
        <BankAccountForm
          onSubmit={editingAccount ? handleUpdateAccount : handleCreateAccount}
          onCancel={handleCancelForm}
          initialData={editingAccount || undefined}
          isLoading={isSubmitting}
        />
      ) : (
        <BankAccountList
          accounts={accounts}
          onEdit={handleEdit}
          onDelete={handleDeleteAccount}
          onToggleStatus={handleToggleStatus}
          isLoading={isLoading}
        />
      )}
    </div>
  );
};

export default ContasPage;
