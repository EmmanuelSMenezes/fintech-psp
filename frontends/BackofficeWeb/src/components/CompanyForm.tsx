'use client';

import React, { useState, useCallback } from 'react';
import MaskedInputField from './MaskedInputField';

// Componente InputField simples
interface InputFieldProps {
  label: string;
  field: string;
  value: string;
  onChange: (field: string, value: string) => void;
  type?: string;
  required?: boolean;
  placeholder?: string;
  maxLength?: number;
}

const InputField: React.FC<InputFieldProps> = ({
  label,
  field,
  value,
  onChange,
  type = 'text',
  required = false,
  placeholder = '',
  maxLength
}) => {
  return (
    <div>
      <label htmlFor={field} className="block text-sm font-medium text-gray-700 mb-1">
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </label>
      <input
        type={type}
        id={field}
        value={value}
        onChange={(e) => onChange(field, e.target.value)}
        placeholder={placeholder}
        maxLength={maxLength}
        required={required}
        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
      />
    </div>
  );
};

interface CompanyFormData {
  razaoSocial: string;
  nomeFantasia: string;
  cnpj: string;
  email: string;
  telefone: string;
  inscricaoEstadual: string;
  address: {
    cep: string;
    logradouro: string;
    numero: string;
    bairro: string;
    cidade: string;
    estado: string;
  };
  applicant: {
    cpf: string;
    nomeCompleto: string;
    email: string;
    telefone: string;
  };
}

interface CompanyFormProps {
  initialData?: Partial<CompanyFormData>;
  onSubmit: (data: CompanyFormData) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
  submitText?: string;
  isEditing?: boolean; // Novo parâmetro para indicar se é edição
}

const CompanyForm: React.FC<CompanyFormProps> = ({
  initialData,
  onSubmit,
  onCancel,
  isLoading = false,
  submitText = 'Salvar',
  isEditing = false
}) => {
  const [formData, setFormData] = useState<CompanyFormData>({
    razaoSocial: initialData?.razaoSocial || '',
    nomeFantasia: initialData?.nomeFantasia || '',
    cnpj: initialData?.cnpj || '',
    email: initialData?.email || '',
    telefone: initialData?.telefone || '',
    inscricaoEstadual: initialData?.inscricaoEstadual || '',
    address: {
      cep: initialData?.address?.cep || '',
      logradouro: initialData?.address?.logradouro || '',
      numero: initialData?.address?.numero || '',
      bairro: initialData?.address?.bairro || '',
      cidade: initialData?.address?.cidade || '',
      estado: initialData?.address?.estado || '',
    },
    applicant: {
      cpf: initialData?.applicant?.cpf || '',
      nomeCompleto: initialData?.applicant?.nomeCompleto || '',
      email: initialData?.applicant?.email || '',
      telefone: initialData?.applicant?.telefone || '',
    }
  });

  const handleInputChange = useCallback((field: string, value: string) => {
    if (field.startsWith('address.')) {
      const addressField = field.replace('address.', '');
      setFormData(prev => ({
        ...prev,
        address: {
          ...prev.address,
          [addressField]: value
        }
      }));
    } else if (field.startsWith('applicant.')) {
      const applicantField = field.replace('applicant.', '');
      setFormData(prev => ({
        ...prev,
        applicant: {
          ...prev.applicant,
          [applicantField]: value
        }
      }));
    } else {
      setFormData(prev => ({
        ...prev,
        [field]: value
      }));
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await onSubmit(formData);
  };

  // Função para obter o valor do campo
  const getFieldValue = useCallback((field: string): string => {
    if (field.startsWith('address.')) {
      return formData.address[field.replace('address.', '') as keyof typeof formData.address];
    } else if (field.startsWith('applicant.')) {
      return formData.applicant[field.replace('applicant.', '') as keyof typeof formData.applicant];
    } else {
      return formData[field as keyof CompanyFormData] as string;
    }
  }, [formData]);

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Dados da Empresa */}
      <div className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <InputField
            label="Razão Social"
            field="razaoSocial"
            value={getFieldValue('razaoSocial')}
            onChange={handleInputChange}
            required
            placeholder="Nome da empresa"
          />
          <InputField
            label="Nome Fantasia"
            field="nomeFantasia"
            value={getFieldValue('nomeFantasia')}
            onChange={handleInputChange}
            placeholder="Nome fantasia"
          />
          <MaskedInputField
            label="CNPJ"
            field="cnpj"
            value={getFieldValue('cnpj')}
            onChange={handleInputChange}
            mask="document"
            required
            placeholder="00.000.000/0000-00"
          />
          <InputField
            label="Email"
            field="email"
            value={getFieldValue('email')}
            onChange={handleInputChange}
            type="email"
            placeholder="contato@empresa.com"
          />
          <MaskedInputField
            label="Telefone"
            field="telefone"
            value={getFieldValue('telefone')}
            onChange={handleInputChange}
            mask="phone"
            placeholder="(11) 99999-9999"
          />
          <InputField
            label="Inscrição Estadual"
            field="inscricaoEstadual"
            value={getFieldValue('inscricaoEstadual')}
            onChange={handleInputChange}
            placeholder="000.000.000.000"
          />
        </div>
      </div>

      {/* Endereço */}
      <div className="border-t border-gray-200 pt-6">
        <h4 className="text-lg font-medium text-gray-900 mb-4 flex items-center">
          <svg className="w-5 h-5 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          Endereço
        </h4>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <MaskedInputField
            label="CEP"
            field="address.cep"
            value={getFieldValue('address.cep')}
            onChange={handleInputChange}
            mask="cep"
            required
            placeholder="00000-000"
          />
          <div className="md:col-span-2">
            <InputField
              label="Logradouro"
              field="address.logradouro"
              value={getFieldValue('address.logradouro')}
              onChange={handleInputChange}
              required
              placeholder="Rua, Avenida, etc."
            />
          </div>
          <InputField
            label="Número"
            field="address.numero"
            value={getFieldValue('address.numero')}
            onChange={handleInputChange}
            placeholder="123"
          />
          <InputField
            label="Bairro"
            field="address.bairro"
            value={getFieldValue('address.bairro')}
            onChange={handleInputChange}
            required
            placeholder="Nome do bairro"
          />
          <InputField
            label="Cidade"
            field="address.cidade"
            value={getFieldValue('address.cidade')}
            onChange={handleInputChange}
            required
            placeholder="Nome da cidade"
          />
          <InputField
            label="Estado"
            field="address.estado"
            value={getFieldValue('address.estado')}
            onChange={handleInputChange}
            required
            placeholder="SP"
            maxLength={2}
          />
        </div>
      </div>

      {/* Dados do Solicitante */}
      <div className="space-y-4">
        <h4 className="text-lg font-semibold text-gray-900 flex items-center">
          <svg className="w-5 h-5 mr-2 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
          Dados do Solicitante
        </h4>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <MaskedInputField
            label="CPF"
            field="applicant.cpf"
            value={getFieldValue('applicant.cpf')}
            onChange={handleInputChange}
            mask="document"
            required={!isEditing}
            placeholder="000.000.000-00"
          />
          <InputField
            label="Nome Completo"
            field="applicant.nomeCompleto"
            value={getFieldValue('applicant.nomeCompleto')}
            onChange={handleInputChange}
            required={!isEditing}
            placeholder="Nome completo do solicitante"
          />
          <InputField
            label="Email"
            field="applicant.email"
            value={getFieldValue('applicant.email')}
            onChange={handleInputChange}
            type="email"
            placeholder="email@solicitante.com"
          />
          <MaskedInputField
            label="Telefone"
            field="applicant.telefone"
            value={getFieldValue('applicant.telefone')}
            onChange={handleInputChange}
            mask="phone"
            placeholder="(11) 99999-9999"
          />
        </div>
      </div>

      {/* Botões */}
      <div className="flex justify-end space-x-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center"
        >
          {isLoading && (
            <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
          )}
          {isLoading ? 'Salvando...' : submitText}
        </button>
      </div>
    </form>
  );
};



export default CompanyForm;
