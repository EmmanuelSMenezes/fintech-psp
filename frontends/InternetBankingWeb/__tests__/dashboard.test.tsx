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
        minhasContas: 3,
        saldoTotal: 15750.00,
        transacoesHoje: 12,
        volumeHoje: 2450.00,
        subUsuarios: 2,
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

// Mock do AuthContext para simular cliente autenticado
jest.mock('@/context/AuthContext', () => ({
  ...jest.requireActual('@/context/AuthContext'),
  useAuth: () => ({
    user: {
      id: 'cliente_banking',
      email: 'cliente_banking@fintech.com',
      role: 'cliente',
      permissions: ['view_dashboard', 'view_transacoes', 'view_contas', 'create_transactions', 'manage_sub_users'],
      scope: 'banking',
    },
    isAuthenticated: true,
    isLoading: false,
    hasPermission: (permission: string) => true,
  }),
  useRequireAuth: () => {},
}));

describe('Internet Banking Dashboard', () => {
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

  it('should render internet banking dashboard header', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Internet Banking')).toBeInTheDocument();
      expect(screen.getByText('Bem-vindo ao seu painel financeiro')).toBeInTheDocument();
      expect(screen.getByText('cliente_banking@fintech.com')).toBeInTheDocument();
    });
  });

  it('should render personal metrics cards', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Minhas Contas')).toBeInTheDocument();
      expect(screen.getByText('Saldo Total')).toBeInTheDocument();
      expect(screen.getByText('Transações Hoje')).toBeInTheDocument();
      expect(screen.getByText('Sub-usuários')).toBeInTheDocument();
    });
  });

  it('should display account balance', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('R$ 15.750,00')).toBeInTheDocument();
      expect(screen.getByText('Disponível')).toBeInTheDocument();
    });
  });

  it('should render quick actions for banking', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('Ações Rápidas')).toBeInTheDocument();
      expect(screen.getByText('Nova Transação')).toBeInTheDocument();
      expect(screen.getByText('PIX ou TED')).toBeInTheDocument();
      expect(screen.getByText('Gerenciar Contas')).toBeInTheDocument();
      expect(screen.getByText('Adicionar/Editar')).toBeInTheDocument();
      expect(screen.getByText('Ver Histórico')).toBeInTheDocument();
      expect(screen.getByText('Transações')).toBeInTheDocument();
      expect(screen.getByText('Configurações')).toBeInTheDocument();
      expect(screen.getByText('Priorização')).toBeInTheDocument();
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

  it('should display user role badge as cliente', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('cliente')).toBeInTheDocument();
    });
  });

  it('should show transaction metrics', async () => {
    render(<DashboardPage />);

    await waitFor(() => {
      expect(screen.getByText('12')).toBeInTheDocument(); // Transações hoje
      expect(screen.getByText('R$ 2.450,00')).toBeInTheDocument(); // Volume hoje
      expect(screen.getByText('2')).toBeInTheDocument(); // Sub-usuários
      expect(screen.getByText('Ativos')).toBeInTheDocument();
    });
  });
});
