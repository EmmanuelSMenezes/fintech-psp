'use client';

import React, { useState, useEffect } from 'react';
import { useRequireAuth, useAuth } from '@/context/AuthContext';
import toast from 'react-hot-toast';
import LoadingSpinner from '@/components/LoadingSpinner';
import QRCode from 'qrcode';

interface QrCodeData {
  transactionId: string;
  externalId: string;
  qrcodePayload: string;
  qrcodeImage: string;
  amount: number;
  pixKey: string;
  description: string;
  expiresAt: string;
  status: string;
}

interface PixKeyData {
  key: string;
  type: string;
  name: string;
}

const QrCodePixPage: React.FC = () => {
  useRequireAuth('transacionar_pix');
  const { user } = useAuth();

  const [isLoading, setIsLoading] = useState(false);
  const [qrCodeData, setQrCodeData] = useState<QrCodeData | null>(null);
  const [qrCodeImageUrl, setQrCodeImageUrl] = useState<string>('');
  const [pixKeys, setPixKeys] = useState<PixKeyData[]>([]);
  const [formData, setFormData] = useState({
    amount: '',
    pixKey: '',
    description: '',
    expiresIn: '3600' // 1 hora
  });

  useEffect(() => {
    loadPixKeys();
  }, []);

  const loadPixKeys = async () => {
    try {
      // Simular carregamento de chaves PIX do usuário
      const mockPixKeys: PixKeyData[] = [
        {
          key: 'a59b3ad1-c78a-4382-9216-01376298b153',
          type: 'UUID',
          name: 'Chave Principal'
        },
        {
          key: '12345678000199',
          type: 'CNPJ',
          name: 'CNPJ da Empresa'
        }
      ];
      setPixKeys(mockPixKeys);
      
      if (mockPixKeys.length > 0) {
        setFormData(prev => ({ ...prev, pixKey: mockPixKeys[0].key }));
      }
    } catch (error) {
      console.error('Erro ao carregar chaves PIX:', error);
      toast.error('Erro ao carregar chaves PIX');
    }
  };

  const handleGenerateQrCode = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.amount || !formData.pixKey) {
      toast.error('Preencha todos os campos obrigatórios');
      return;
    }

    try {
      setIsLoading(true);
      
      const requestData = {
        amount: parseFloat(formData.amount),
        pixKey: formData.pixKey,
        description: formData.description || 'Cobrança PIX',
        expiresIn: parseInt(formData.expiresIn),
        externalId: `QR-${Date.now()}`
      };

      const response = await fetch('/api/transacoes/pix/qrcode/dinamico', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(requestData)
      });

      if (!response.ok) {
        throw new Error('Erro ao gerar QR Code');
      }

      const data = await response.json();
      setQrCodeData(data);

      // Gerar QR Code como imagem
      try {
        const qrCodeUrl = await QRCode.toDataURL(data.qrcodePayload, {
          width: 200,
          margin: 2,
          color: {
            dark: '#000000',
            light: '#FFFFFF'
          }
        });
        setQrCodeImageUrl(qrCodeUrl);
      } catch (qrError) {
        console.error('Erro ao gerar QR Code:', qrError);
      }

      toast.success('QR Code gerado com sucesso!');
      
    } catch (error) {
      console.error('Erro ao gerar QR Code:', error);
      toast.error('Erro ao gerar QR Code PIX');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCopyPixCode = () => {
    if (qrCodeData?.qrcodePayload) {
      navigator.clipboard.writeText(qrCodeData.qrcodePayload);
      toast.success('PIX Copia e Cola copiado!');
    }
  };

  const handleNewQrCode = () => {
    setQrCodeData(null);
    setQrCodeImageUrl('');
    setFormData({
      amount: '',
      pixKey: pixKeys[0]?.key || '',
      description: '',
      expiresIn: '3600'
    });
  };

  const formatCurrency = (value: string) => {
    const numericValue = value.replace(/\D/g, '');
    const formattedValue = (parseInt(numericValue) / 100).toFixed(2);
    return formattedValue;
  };

  const handleAmountChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    const formatted = formatCurrency(value);
    setFormData(prev => ({ ...prev, amount: formatted }));
  };

  if (isLoading) {
    return <LoadingSpinner />;
  }

  return (
    <div className="p-6 max-w-4xl mx-auto">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">QR Code PIX Dinâmico</h1>
        <p className="text-gray-600 mt-2">Gere QR Codes para receber pagamentos PIX</p>
      </div>

      {!qrCodeData ? (
        /* Formulário de Geração */
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <form onSubmit={handleGenerateQrCode} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Valor */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Valor *
                </label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-500">
                    R$
                  </span>
                  <input
                    type="text"
                    value={formData.amount}
                    onChange={handleAmountChange}
                    placeholder="0,00"
                    className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    required
                  />
                </div>
              </div>

              {/* Chave PIX */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Chave PIX *
                </label>
                <select
                  value={formData.pixKey}
                  onChange={(e) => setFormData(prev => ({ ...prev, pixKey: e.target.value }))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                >
                  {pixKeys.map((key) => (
                    <option key={key.key} value={key.key}>
                      {key.name} ({key.type})
                    </option>
                  ))}
                </select>
              </div>
            </div>

            {/* Descrição */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Descrição
              </label>
              <input
                type="text"
                value={formData.description}
                onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                placeholder="Descrição da cobrança (opcional)"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {/* Tempo de Expiração */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Tempo de Expiração
              </label>
              <select
                value={formData.expiresIn}
                onChange={(e) => setFormData(prev => ({ ...prev, expiresIn: e.target.value }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="1800">30 minutos</option>
                <option value="3600">1 hora</option>
                <option value="7200">2 horas</option>
                <option value="21600">6 horas</option>
                <option value="86400">24 horas</option>
              </select>
            </div>

            {/* Botão de Gerar */}
            <div className="flex justify-end">
              <button
                type="submit"
                disabled={isLoading}
                className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg flex items-center space-x-2 disabled:opacity-50"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 0h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z" />
                </svg>
                <span>Gerar QR Code</span>
              </button>
            </div>
          </form>
        </div>
      ) : (
        /* QR Code Gerado */
        <div className="space-y-6">
          {/* Informações da Cobrança */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900">QR Code Gerado</h2>
              <span className="px-3 py-1 bg-green-100 text-green-800 text-sm font-medium rounded-full">
                {qrCodeData.status}
              </span>
            </div>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-gray-600">Valor</p>
                <p className="text-2xl font-bold text-gray-900">
                  R$ {qrCodeData.amount.toFixed(2).replace('.', ',')}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Expira em</p>
                <p className="text-lg font-medium text-gray-900">
                  {new Date(qrCodeData.expiresAt).toLocaleString('pt-BR')}
                </p>
              </div>
            </div>
            
            {qrCodeData.description && (
              <div className="mt-4">
                <p className="text-sm text-gray-600">Descrição</p>
                <p className="text-gray-900">{qrCodeData.description}</p>
              </div>
            )}
          </div>

          {/* QR Code Visual */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="text-center">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Escaneie o QR Code
              </h3>
              <div className="flex justify-center mb-4">
                <div className="p-4 bg-white border-2 border-gray-200 rounded-lg">
                  {qrCodeImageUrl ? (
                    <img
                      src={qrCodeImageUrl}
                      alt="QR Code PIX"
                      className="w-50 h-50"
                    />
                  ) : (
                    <div className="w-50 h-50 bg-gray-200 flex items-center justify-center">
                      <span className="text-gray-500">Carregando QR Code...</span>
                    </div>
                  )}
                </div>
              </div>
              <p className="text-sm text-gray-600">
                Use o app do seu banco para escanear o código
              </p>
            </div>
          </div>

          {/* PIX Copia e Cola */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              PIX Copia e Cola
            </h3>
            <div className="flex items-center space-x-3">
              <div className="flex-1 p-3 bg-gray-50 rounded-lg border">
                <code className="text-sm text-gray-700 break-all">
                  {qrCodeData.qrcodePayload}
                </code>
              </div>
              <button
                onClick={handleCopyPixCode}
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg flex items-center space-x-2"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                </svg>
                <span>Copiar</span>
              </button>
            </div>
            <p className="text-sm text-gray-600 mt-2">
              Cole este código no app do seu banco na opção "PIX Copia e Cola"
            </p>
          </div>

          {/* Ações */}
          <div className="flex justify-center">
            <button
              onClick={handleNewQrCode}
              className="px-6 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg flex items-center space-x-2"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              <span>Gerar Novo QR Code</span>
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default QrCodePixPage;
