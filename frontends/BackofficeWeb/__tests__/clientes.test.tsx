import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { jest } from '@jest/globals';
import ClientesPage from '@/app/(admin)/clientes/page';
import { userService } from '@/services/api';
import toast from 'react-hot-toast';

// Mock dos serviços
jest.mock('@/services/api', () => ({
  userService: {
    getUsers: jest.fn(),
    createUser: jest.fn(),
    updateUser: jest.fn(),
    deleteUser: jest.fn(),
  },
}));

jest.mock('react-hot-toast', () => ({
  success: jest.fn(),
  error: jest.fn(),
}));

jest.mock('@/context/AuthContext', () => ({
  useAuth: () => ({
    user: { id: '1', email: 'admin@test.com', role: 'admin' },
  }),
  useRequireAuth: () => {},
}));

// Mock dos componentes
jest.mock('@/components/ConfirmModal', () => {
  return function MockConfirmModal({ isOpen, onConfirm, onCancel }: any) {
    if (!isOpen) return null;
    return (
      <div data-testid="confirm-modal">
        <button onClick={onConfirm} data-testid="confirm-button">
          Confirmar
        </button>
        <button onClick={onCancel} data-testid="cancel-button">
          Cancelar
        </button>
      </div>
    );
  };
});

jest.mock('@/components/LoadingSpinner', () => {
  return function MockLoadingSpinner({ text }: any) {
    return <div data-testid="loading-spinner">{text}</div>;
  };
});

jest.mock('@/components/Pagination', () => {
  return function MockPagination({ onPageChange, currentPage }: any) {
    return (
      <div data-testid="pagination">
        <button onClick={() => onPageChange(currentPage + 1)}>
          Próxima
        </button>
      </div>
    );
  };
});

const mockUsers = [
  {
    id: '1',
    email: 'cliente1@test.com',
    name: 'Cliente 1',
    role: 'cliente',
    isActive: true,
    lastLogin: '2024-01-15T10:00:00Z',
    createdAt: '2024-01-01T10:00:00Z',
  },
  {
    id: '2',
    email: 'admin@test.com',
    name: 'Admin User',
    role: 'admin',
    isActive: true,
    lastLogin: '2024-01-16T10:00:00Z',
    createdAt: '2024-01-01T10:00:00Z',
  },
];

describe('ClientesPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (userService.getUsers as jest.Mock).mockResolvedValue({
      data: {
        users: mockUsers,
        total: mockUsers.length,
        page: 1,
        limit: 10,
      },
    });
  });

  it('should render the page title and create button', async () => {
    render(<ClientesPage />);
    
    expect(screen.getByText('Gestão de Clientes')).toBeInTheDocument();
    expect(screen.getByText('Novo Cliente')).toBeInTheDocument();
  });

  it('should load and display clients', async () => {
    render(<ClientesPage />);
    
    await waitFor(() => {
      expect(screen.getByText('Cliente 1')).toBeInTheDocument();
      expect(screen.getByText('cliente1@test.com')).toBeInTheDocument();
      expect(screen.getByText('Admin User')).toBeInTheDocument();
      expect(screen.getByText('admin@test.com')).toBeInTheDocument();
    });
  });

  it('should open create modal when clicking new client button', async () => {
    render(<ClientesPage />);
    
    const createButton = screen.getByText('Novo Cliente');
    fireEvent.click(createButton);
    
    expect(screen.getByText('Novo Cliente')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('Nome completo')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('email@exemplo.com')).toBeInTheDocument();
  });

  it('should create a new client successfully', async () => {
    const user = userEvent.setup();
    (userService.createUser as jest.Mock).mockResolvedValue({
      data: { id: '3', email: 'novo@test.com', name: 'Novo Cliente' },
    });

    render(<ClientesPage />);
    
    // Abrir modal de criação
    const createButton = screen.getByText('Novo Cliente');
    await user.click(createButton);
    
    // Preencher formulário
    await user.type(screen.getByPlaceholderText('Nome completo'), 'Novo Cliente');
    await user.type(screen.getByPlaceholderText('email@exemplo.com'), 'novo@test.com');
    
    // Submeter formulário
    const submitButton = screen.getByText('Criar Cliente');
    await user.click(submitButton);
    
    await waitFor(() => {
      expect(userService.createUser).toHaveBeenCalledWith({
        name: 'Novo Cliente',
        email: 'novo@test.com',
        role: 'cliente',
        document: '',
        phone: '',
        password: '',
      });
      expect(toast.success).toHaveBeenCalledWith(
        'Cliente criado com sucesso! Email de boas-vindas enviado.'
      );
    });
  });

  it('should handle create client error', async () => {
    const user = userEvent.setup();
    (userService.createUser as jest.Mock).mockRejectedValue({
      response: { status: 400, data: { message: 'Email já existe' } },
    });

    render(<ClientesPage />);
    
    // Abrir modal e tentar criar cliente
    const createButton = screen.getByText('Novo Cliente');
    await user.click(createButton);
    
    await user.type(screen.getByPlaceholderText('Nome completo'), 'Teste');
    await user.type(screen.getByPlaceholderText('email@exemplo.com'), 'teste@test.com');
    
    const submitButton = screen.getByText('Criar Cliente');
    await user.click(submitButton);
    
    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith('Erro de validação: Email já existe');
    });
  });

  it('should filter clients by search term', async () => {
    const user = userEvent.setup();
    render(<ClientesPage />);
    
    // Aguardar carregamento inicial
    await waitFor(() => {
      expect(screen.getByText('Cliente 1')).toBeInTheDocument();
    });
    
    // Filtrar por termo de busca
    const searchInput = screen.getByPlaceholderText('Nome, email ou documento...');
    await user.type(searchInput, 'Admin');
    
    await waitFor(() => {
      expect(userService.getUsers).toHaveBeenCalledWith({
        page: 1,
        limit: 10,
        search: 'Admin',
        role: undefined,
      });
    });
  });

  it('should open edit modal when clicking edit button', async () => {
    const user = userEvent.setup();
    render(<ClientesPage />);
    
    await waitFor(() => {
      expect(screen.getByText('Cliente 1')).toBeInTheDocument();
    });
    
    const editButtons = screen.getAllByText('Editar');
    await user.click(editButtons[0]);
    
    expect(screen.getByText('Editar Cliente')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Cliente 1')).toBeInTheDocument();
    expect(screen.getByDisplayValue('cliente1@test.com')).toBeInTheDocument();
  });

  it('should delete client when confirmed', async () => {
    const user = userEvent.setup();
    (userService.deleteUser as jest.Mock).mockResolvedValue({});

    render(<ClientesPage />);
    
    await waitFor(() => {
      expect(screen.getByText('Cliente 1')).toBeInTheDocument();
    });
    
    // Clicar em excluir
    const deleteButtons = screen.getAllByText('Excluir');
    await user.click(deleteButtons[0]);
    
    // Confirmar exclusão
    const confirmButton = screen.getByTestId('confirm-button');
    await user.click(confirmButton);
    
    await waitFor(() => {
      expect(userService.deleteUser).toHaveBeenCalledWith('1');
      expect(toast.success).toHaveBeenCalledWith('Cliente excluído com sucesso!');
    });
  });

  it('should handle API errors gracefully', async () => {
    (userService.getUsers as jest.Mock).mockRejectedValue({
      response: { status: 500 },
    });

    render(<ClientesPage />);
    
    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith(
        'Erro interno do servidor. Tente novamente mais tarde.'
      );
    });
  });

  it('should show empty state when no clients found', async () => {
    (userService.getUsers as jest.Mock).mockResolvedValue({
      data: { users: [], total: 0, page: 1, limit: 10 },
    });

    render(<ClientesPage />);
    
    await waitFor(() => {
      expect(screen.getByText('Nenhum cliente encontrado')).toBeInTheDocument();
      expect(screen.getByText('Comece criando um novo cliente.')).toBeInTheDocument();
    });
  });
});
