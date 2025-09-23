'use client';

import React, { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import MaskedInput from '@/components/MaskedInput';
import { CreateBankAccountRequest, User } from '@/services/api';
import { toast } from 'react-hot-toast';

interface BankAccountFormProps {
  onSubmit: (data: CreateBankAccountRequest) => Promise<void>;
  onCancel: () => void;
  clientes: User[];
  isLoading?: boolean;
}

const bancos = [
  { code: '001', name: 'Banco do Brasil' },
  { code: '033', name: 'Santander' },
  { code: '104', name: 'Caixa Econômica Federal' },
  { code: '237', name: 'Bradesco' },
  { code: '341', name: 'Itaú' },
  { code: '260', name: 'Nu Pagamentos' },
  { code: '077', name: 'Banco Inter' },
  { code: '212', name: 'Banco Original' },
  { code: '336', name: 'Banco C6' },
  { code: '290', name: 'PagSeguro' },
];

const BankAccountForm: React.FC<BankAccountFormProps> = ({
  onSubmit,
  onCancel,
  clientes,
  isLoading = false
}) => {
  const [formData, setFormData] = useState<CreateBankAccountRequest>({
    clienteId: '',
    banco: '',
    agencia: '',
    conta: '',
    tipoConta: 'corrente',
    bankCode: '',
    accountNumber: '',
    description: '',
    credentials: {
      clientId: '',
      clientSecret: '',
      apiKey: '',
      environment: 'sandbox',
      mtlsCert: '',
      additionalData: {}
    }
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.clienteId) newErrors.clienteId = 'Cliente é obrigatório';
    if (!formData.banco) newErrors.banco = 'Banco é obrigatório';
    if (!formData.agencia) newErrors.agencia = 'Agência é obrigatória';
    if (!formData.conta) newErrors.conta = 'Conta é obrigatória';
    if (!formData.bankCode) newErrors.bankCode = 'Código do banco é obrigatório';
    if (!formData.accountNumber) newErrors.accountNumber = 'Número da conta é obrigatório';
    if (!formData.description) newErrors.description = 'Descrição é obrigatória';
    if (!formData.credentials.clientId) newErrors.clientId = 'Client ID é obrigatório';
    if (!formData.credentials.clientSecret) newErrors.clientSecret = 'Client Secret é obrigatório';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      toast.error('Por favor, corrija os erros no formulário');
      return;
    }

    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Erro ao criar conta bancária:', error);
    }
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
    
    // Limpar erro do campo quando o usuário começar a digitar
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }
  };

  const handleCredentialsChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      credentials: {
        ...prev.credentials,
        [field]: value
      }
    }));
    
    if (errors[field]) {
      setErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }
  };

  const handleBankSelect = (bankCode: string) => {
    const selectedBank = bancos.find(b => b.code === bankCode);
    setFormData(prev => ({
      ...prev,
      bankCode,
      banco: selectedBank?.name || '',
      description: selectedBank ? `Conta ${selectedBank.name}` : ''
    }));
  };

  return (
    <Card className="w-full max-w-4xl mx-auto">
      <CardHeader>
        <CardTitle>Nova Conta Bancária</CardTitle>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          <Tabs defaultValue="basic" className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="basic">Dados Básicos</TabsTrigger>
              <TabsTrigger value="credentials">Credenciais</TabsTrigger>
            </TabsList>
            
            <TabsContent value="basic" className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="clienteId">Cliente *</Label>
                  <Select 
                    value={formData.clienteId} 
                    onValueChange={(value) => handleInputChange('clienteId', value)}
                  >
                    <SelectTrigger className={errors.clienteId ? 'border-red-500' : ''}>
                      <SelectValue placeholder="Selecione um cliente" />
                    </SelectTrigger>
                    <SelectContent>
                      {clientes.map((cliente) => (
                        <SelectItem key={cliente.id} value={cliente.id}>
                          {cliente.name} ({cliente.email})
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.clienteId && <p className="text-sm text-red-500">{errors.clienteId}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="banco">Banco *</Label>
                  <Select 
                    value={formData.bankCode} 
                    onValueChange={handleBankSelect}
                  >
                    <SelectTrigger className={errors.banco ? 'border-red-500' : ''}>
                      <SelectValue placeholder="Selecione um banco" />
                    </SelectTrigger>
                    <SelectContent>
                      {bancos.map((banco) => (
                        <SelectItem key={banco.code} value={banco.code}>
                          {banco.code} - {banco.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.banco && <p className="text-sm text-red-500">{errors.banco}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="agencia">Agência *</Label>
                  <MaskedInput
                    mask="bankAgency"
                    value={formData.agencia}
                    onChange={(value) => handleInputChange('agencia', value)}
                    className={errors.agencia ? 'border-red-500' : ''}
                  />
                  {errors.agencia && <p className="text-sm text-red-500">{errors.agencia}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="conta">Conta *</Label>
                  <MaskedInput
                    mask="bankAccount"
                    value={formData.conta}
                    onChange={(value) => handleInputChange('conta', value)}
                    className={errors.conta ? 'border-red-500' : ''}
                  />
                  {errors.conta && <p className="text-sm text-red-500">{errors.conta}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="tipoConta">Tipo de Conta *</Label>
                  <Select 
                    value={formData.tipoConta} 
                    onValueChange={(value: 'corrente' | 'poupanca') => handleInputChange('tipoConta', value)}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="corrente">Conta Corrente</SelectItem>
                      <SelectItem value="poupanca">Conta Poupança</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="accountNumber">Número da Conta (API) *</Label>
                  <Input
                    value={formData.accountNumber}
                    onChange={(e) => handleInputChange('accountNumber', e.target.value)}
                    placeholder="Número da conta para integração"
                    className={errors.accountNumber ? 'border-red-500' : ''}
                  />
                  {errors.accountNumber && <p className="text-sm text-red-500">{errors.accountNumber}</p>}
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="description">Descrição *</Label>
                <Textarea
                  value={formData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  placeholder="Descrição da conta bancária"
                  className={errors.description ? 'border-red-500' : ''}
                />
                {errors.description && <p className="text-sm text-red-500">{errors.description}</p>}
              </div>
            </TabsContent>

            <TabsContent value="credentials" className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="clientId">Client ID *</Label>
                  <Input
                    value={formData.credentials.clientId}
                    onChange={(e) => handleCredentialsChange('clientId', e.target.value)}
                    placeholder="Client ID da API"
                    className={errors.clientId ? 'border-red-500' : ''}
                  />
                  {errors.clientId && <p className="text-sm text-red-500">{errors.clientId}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="clientSecret">Client Secret *</Label>
                  <Input
                    type="password"
                    value={formData.credentials.clientSecret}
                    onChange={(e) => handleCredentialsChange('clientSecret', e.target.value)}
                    placeholder="Client Secret da API"
                    className={errors.clientSecret ? 'border-red-500' : ''}
                  />
                  {errors.clientSecret && <p className="text-sm text-red-500">{errors.clientSecret}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="apiKey">API Key</Label>
                  <Input
                    value={formData.credentials.apiKey || ''}
                    onChange={(e) => handleCredentialsChange('apiKey', e.target.value)}
                    placeholder="API Key (opcional)"
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="environment">Ambiente</Label>
                  <Select 
                    value={formData.credentials.environment} 
                    onValueChange={(value: 'sandbox' | 'production') => handleCredentialsChange('environment', value)}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="sandbox">Sandbox</SelectItem>
                      <SelectItem value="production">Produção</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="mtlsCert">Certificado mTLS</Label>
                <Textarea
                  value={formData.credentials.mtlsCert || ''}
                  onChange={(e) => handleCredentialsChange('mtlsCert', e.target.value)}
                  placeholder="Certificado mTLS (opcional)"
                  rows={4}
                />
              </div>
            </TabsContent>
          </Tabs>

          <div className="flex justify-end space-x-2 pt-4">
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancelar
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? 'Criando...' : 'Criar Conta'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};

export default BankAccountForm;
