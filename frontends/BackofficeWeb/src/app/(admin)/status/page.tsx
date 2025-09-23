'use client';

import React from 'react';

const StatusPage: React.FC = () => {
  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
          Status das Transações
        </h1>
        <p className="text-gray-600 dark:text-gray-400 mt-2">
          Monitore o status em tempo real das transações
        </p>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <div className="text-center py-12">
          <div className="text-6xl text-gray-300 dark:text-gray-600 mb-4">
            🔄
          </div>
          <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
            Página de Status
          </h3>
          <p className="text-gray-500 dark:text-gray-400">
            Status real-time com polling de transações pendentes/confirmadas será implementado aqui.
            <br />
            Permissões: 'view_status'
          </p>
        </div>
      </div>
    </div>
  );
};

export default StatusPage;
