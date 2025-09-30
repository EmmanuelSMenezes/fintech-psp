'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import toast from 'react-hot-toast';

interface SubUserFormProps {
  onSave: (data: SubUserFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
  initialData?: Partial<SubUserFormData>;
}

export interface SubUserFormData {
  subUserEmail: string;
  role: 'sub-usuario';
  permissions: string[];
  sendInviteEmail: boolean;
  customMessage?: string;
}

// Schema de validação
const subUserSchema = yup.object({
  subUserEmail: yup
    .string()
    .required('Email é obrigatório')
    .email('Email deve ter um formato válido'),
  role: yup
    .string()
    .required('Função é obrigatória')
    .oneOf(['sub-usuario'], 'Função inválida'),
  permissions: yup
    .array()
    .of(yup.string())
    .min(1, 'Selecione pelo menos uma permissão')
    .required('Permissões são obrigatórias'),
  sendInviteEmail: yup.boolean().required(),
  customMessage: yup.string().max(500, 'Mensagem deve ter no máximo 500 caracteres'),
});

const SubUserForm: React.FC<SubUserFormProps> = ({ 
  onSave, 
  onCancel, 
  isLoading = false,
  initialData 
}) => {
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>(
    initialData?.permissions || []
  );

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
    setValue,
  } = useForm<SubUserFormData>({
    resolver: yupResolver(subUserSchema),
    defaultValues: {
      subUserEmail: initialData?.subUserEmail || '',
      role: 'sub-usuario',
      permissions: initialData?.permissions || [],
      sendInviteEmail: initialData?.sendInviteEmail ?? true,
      customMessage: initialData?.customMessage || '',
    },
  });

  const sendInviteEmail = watch('sendInviteEmail');

  // Permissões disponíveis para sub-usuários
  const availablePermissions = [
    {
      id: 'view_tela_dashboard',
      name: 'Dashboard',
      description: 'Visualizar dashboard e métricas gerais',
      category: 'Visualização',
    },
    {
      id: 'view_tela_contas',
      name: 'Contas',
      description: 'Visualizar contas bancárias',
      category: 'Visualização',
    },
    {
      id: 'view_tela_transacoes',
      name: 'Transações',
      description: 'Visualizar página de transações',
      category: 'Visualização',
    },
    {
      id: 'view_tela_historico',
      name: 'Histórico',
      description: 'Visualizar histórico de transações',
      category: 'Visualização',
    },
    {
      id: 'transacionar_pix',
      name: 'PIX',
      description: 'Realizar transações PIX',
      category: 'Transações',
    },
    {
      id: 'exportar_relatorios',
      name: 'Exportar Relatórios',
      description: 'Exportar relatórios em CSV/PDF',
      category: 'Relatórios',
    },
  ];

  const handlePermissionChange = (permissionId: string, checked: boolean) => {
    let newPermissions: string[];
    
    if (checked) {
      newPermissions = [...selectedPermissions, permissionId];
    } else {
      newPermissions = selectedPermissions.filter(p => p !== permissionId);
    }
    
    setSelectedPermissions(newPermissions);
    setValue('permissions', newPermissions);
  };

  const selectAllPermissions = () => {
    const allPermissions = availablePermissions.map(p => p.id);
    setSelectedPermissions(allPermissions);
    setValue('permissions', allPermissions);
  };

  const clearAllPermissions = () => {
    setSelectedPermissions([]);
    setValue('permissions', []);
  };

  const onSubmit = (data: SubUserFormData) => {
    onSave({
      ...data,
      permissions: selectedPermissions,
    });
  };

  const groupedPermissions = availablePermissions.reduce((acc, permission) => {
    if (!acc[permission.category]) {
      acc[permission.category] = [];
    }
    acc[permission.category].push(permission);
    return acc;
  }, {} as Record<string, typeof availablePermissions>);

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <div className="p-6 border-b border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900">
          {initialData ? 'Editar Sub-usuário' : 'Novo Sub-usuário'}
        </h3>
        <p className="text-sm text-gray-600 mt-1">
          Configure as permissões e envie um convite por email
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
        {/* Email */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Email do Sub-usuário *
          </label>
          <input
            type="email"
            {...register('subUserEmail')}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            placeholder="usuario@empresa.com"
          />
          {errors.subUserEmail && (
            <p className="text-sm text-red-500 mt-1">{errors.subUserEmail.message}</p>
          )}
        </div>

        {/* Função */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Função
          </label>
          <select
            {...register('role')}
            className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            disabled
          >
            <option value="sub-usuario">Sub-usuário</option>
          </select>
          <p className="text-xs text-gray-500 mt-1">
            Sub-usuários têm acesso limitado conforme as permissões selecionadas
          </p>
        </div>

        {/* Permissões */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <label className="block text-sm font-medium text-gray-700">
              Permissões *
            </label>
            <div className="flex space-x-2">
              <button
                type="button"
                onClick={selectAllPermissions}
                className="text-xs text-blue-600 hover:text-blue-700"
              >
                Selecionar Todas
              </button>
              <button
                type="button"
                onClick={clearAllPermissions}
                className="text-xs text-gray-600 hover:text-gray-700"
              >
                Limpar Todas
              </button>
            </div>
          </div>

          <div className="space-y-4">
            {Object.entries(groupedPermissions).map(([category, permissions]) => (
              <div key={category} className="border border-gray-200 rounded-lg p-4">
                <h4 className="text-sm font-medium text-gray-900 mb-3">{category}</h4>
                <div className="space-y-2">
                  {permissions.map((permission) => (
                    <div key={permission.id} className="flex items-start">
                      <input
                        type="checkbox"
                        id={permission.id}
                        checked={selectedPermissions.includes(permission.id)}
                        onChange={(e) => handlePermissionChange(permission.id, e.target.checked)}
                        className="mt-1 w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                      />
                      <div className="ml-3">
                        <label
                          htmlFor={permission.id}
                          className="text-sm font-medium text-gray-900 cursor-pointer"
                        >
                          {permission.name}
                        </label>
                        <p className="text-xs text-gray-600">{permission.description}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>

          {errors.permissions && (
            <p className="text-sm text-red-500 mt-1">{errors.permissions.message}</p>
          )}
        </div>

        {/* Convite por Email */}
        <div className="border border-gray-200 rounded-lg p-4">
          <div className="flex items-center">
            <input
              type="checkbox"
              {...register('sendInviteEmail')}
              className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
            />
            <label className="ml-3 text-sm font-medium text-gray-900">
              Enviar convite por email
            </label>
          </div>
          <p className="text-xs text-gray-600 mt-1 ml-7">
            O sub-usuário receberá um email com instruções para acessar o sistema
          </p>

          {sendInviteEmail && (
            <div className="mt-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Mensagem personalizada (opcional)
              </label>
              <textarea
                {...register('customMessage')}
                rows={3}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Adicione uma mensagem personalizada ao convite..."
              />
              {errors.customMessage && (
                <p className="text-sm text-red-500 mt-1">{errors.customMessage.message}</p>
              )}
            </div>
          )}
        </div>

        {/* Resumo das Permissões */}
        {selectedPermissions.length > 0 && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <h4 className="text-sm font-medium text-blue-900 mb-2">
              Resumo das Permissões ({selectedPermissions.length})
            </h4>
            <div className="flex flex-wrap gap-2">
              {selectedPermissions.map((permissionId) => {
                const permission = availablePermissions.find(p => p.id === permissionId);
                return permission ? (
                  <span
                    key={permissionId}
                    className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                  >
                    {permission.name}
                  </span>
                ) : null;
              })}
            </div>
          </div>
        )}

        {/* Botões */}
        <div className="flex justify-end space-x-3 pt-6 border-t border-gray-200">
          <button
            type="button"
            onClick={onCancel}
            disabled={isLoading}
            className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isLoading || selectedPermissions.length === 0}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {isLoading ? 'Salvando...' : (initialData ? 'Atualizar Sub-usuário' : 'Criar Sub-usuário')}
          </button>
        </div>
      </form>
    </div>
  );
};

export default SubUserForm;
