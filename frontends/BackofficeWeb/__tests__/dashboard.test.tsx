import { render, screen, waitFor } from '@testing-library/react';
import { useRouter } from 'next/navigation';
import { AuthProvider } from '@/context/AuthContext';
import DashboardPage from '@/app/(admin)/page';

// Mock do Next.js router
jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

// Mock do react-hot-toast
jest.mock('react-hot-toast', () => ({
  __esModule: true,
  default: {
    success: jest.fn(),
    error: jest.fn(),
  },
}));

// Mock do axios
jest.mock('axios', () => ({
  create: jest.fn(() => ({
    get: jest.fn().mockResolvedValue({
      data: {
        totalClientes: 150,
        totalContas: 320,
        totalTransacoes: 1250,
        volumeTransacoes: 2450000,
        transacoesHoje: 45,
        volumeHoje: 125000,
      },
    }),
    interceptors: {
      request: { use: jest.fn() },
      response: { use: jest.fn() },
    },
  })),
}));

const mockPush = jest.fn();
const mockRouter = useRouter as jest.MockedFunction<typeof useRouter>;

// Mock do AuthContext para simular usuário autenticado
jest.mock('@/context/AuthContext', () => ({
  ...jest.requireActual('@/context/AuthContext'),
  useAuth: () => ({
    user: {
      id: 'admin_backoffice',
      email: 'admin_backoffice@fintech.com',
      role: 'admin',
      permissions: ['view_dashboard', 'view_transacoes', 'view_contas', 'view_clientes'],
      scope: 'admin',
    },
    isAuthenticated: true,
    isLoading: false,
    hasPermission: (permission: string) => true,
  }),
  useRequireAuth: () => {},
}));

describe('Dashboard', () => {
  beforeEach(() => {
    mockRouter.mockReturnValue({
      push: mockPush,
      replace: jest.fn(),
      prefetch: jest.fn(),
      back: jest.fn(),
      forward: jest.fn(),
      refresh: jest.fn(),
    } as any);
    mockPush.mockClear();
  });

  it('should render dashboard header', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Backoffice PSP')).toBeInTheDocument();
      expect(screen.getByText('Painel administrativo do sistema')).toBeInTheDocument();
      expect(screen.getByText('admin_backoffice@fintech.com')).toBeInTheDocument();
    });
  });

  it('should render metrics cards', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Total de Clientes')).toBeInTheDocument();
      expect(screen.getByText('Total de Contas')).toBeInTheDocument();
      expect(screen.getByText('Transações Hoje')).toBeInTheDocument();
      expect(screen.getByText('Volume Hoje')).toBeInTheDocument();
    });
  });

  it('should render quick actions', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Ações Rápidas')).toBeInTheDocument();
      expect(screen.getByText('Gerenciar Clientes')).toBeInTheDocument();
      expect(screen.getByText('Configurar Contas')).toBeInTheDocument();
      expect(screen.getByText('Ver Transações')).toBeInTheDocument();
      expect(screen.getByText('Relatórios')).toBeInTheDocument();
    });
  });

  it('should show loading state initially', () => {
    // Mock para simular loading
    jest.doMock('@/context/AuthContext', () => ({
      ...jest.requireActual('@/context/AuthContext'),
      useAuth: () => ({
        user: null,
        isAuthenticated: false,
        isLoading: true,
        hasPermission: () => false,
      }),
      useRequireAuth: () => {},
    }));

    render(<DashboardPage />);

    expect(screen.getByRole('status')).toBeInTheDocument(); // Loading spinner
  });

  it('should display user role badge', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('admin')).toBeInTheDocument();
    });
  });
});
